using Serilog;
using System.IO;
using System.Net.Http;

namespace Anicluster.Metadata {

    /// <summary>
    /// Schickt die Anfragen an eine Quelle und übersetzt alles, was schiefgehen kann, in einen <see cref="MetadataStatus"/>.
    /// Pro Quelle gibt es genau eine Instanz: Anfragen laufen nacheinander, zwischen zwei Anfragen liegt mindestens der eingestellte
    /// Abstand, und nach einer Bremse der Quelle (HTTP 429) wird bis zum Ablauf der genannten Zeit gar nicht erst angefragt.
    /// </summary>
    internal sealed class MetadataHttp {

        private const long MaxTextBytes = 2 * 1024 * 1024;
        private const int DefaultRetryAfterSeconds = 60;
        private const int MaxRetryAfterSeconds = 600;

        private readonly HttpClient _client;
        private readonly TimeSpan _minimumGap;
        private readonly SemaphoreSlim _gate = new SemaphoreSlim(1, 1);
        private DateTime _lastRequestUtc = DateTime.MinValue;
        private DateTime _blockedUntilUtc = DateTime.MinValue;

        public MetadataHttp(HttpClient client, TimeSpan minimumGap) {
            this._client = client;
            this._minimumGap = minimumGap;
        }

        /// <summary>Schickt die Anfrage und liefert die Antwort als Text (JSON).</summary>
        public Task<MetadataResult<string>> SendForTextAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            return this.SendAsync(request, async (content, token) => {
                if (content.Headers.ContentLength is long length && length > MaxTextBytes) {
                    return MetadataResult<string>.Failure(MetadataStatus.ServiceError, detail: "Antwort zu groß");
                }
                return MetadataResult<string>.Success(await content.ReadAsStringAsync(token));
            }, cancellationToken);
        }

        /// <summary>Schickt die Anfrage und liefert die Antwort als Bytes, Antworten über <paramref name="maxBytes"/> werden abgelehnt.</summary>
        public Task<MetadataResult<byte[]>> SendForBytesAsync(
            HttpRequestMessage request,
            long maxBytes,
            CancellationToken cancellationToken
        ) {
            return this.SendAsync(request, async (content, token) => {
                if (content.Headers.ContentLength is long length && length > maxBytes) {
                    return MetadataResult<byte[]>.Failure(MetadataStatus.ServiceError, detail: "Datei zu groß");
                }
                byte[] bytes = await content.ReadAsByteArrayAsync(token);
                return bytes.Length > maxBytes
                    ? MetadataResult<byte[]>.Failure(MetadataStatus.ServiceError, detail: "Datei zu groß")
                    : MetadataResult<byte[]>.Success(bytes);
            }, cancellationToken);
        }

        private async Task<MetadataResult<T>> SendAsync<T>(
            HttpRequestMessage request,
            Func<HttpContent, CancellationToken, Task<MetadataResult<T>>> readContent,
            CancellationToken cancellationToken
        ) {
            try {
                await this._gate.WaitAsync(cancellationToken);
            }
            catch (OperationCanceledException) {
                request.Dispose();
                return MetadataResult<T>.Failure(MetadataStatus.Canceled);
            }
            try {
                int blockedSeconds = (int)Math.Ceiling((this._blockedUntilUtc - DateTime.UtcNow).TotalSeconds);
                if (blockedSeconds > 0) {
                    return MetadataResult<T>.Failure(MetadataStatus.RateLimited, blockedSeconds);
                }
                TimeSpan wait = this._lastRequestUtc + this._minimumGap - DateTime.UtcNow;
                if (wait > TimeSpan.Zero) {
                    await Task.Delay(wait, cancellationToken);
                }
                this._lastRequestUtc = DateTime.UtcNow;
                using (HttpResponseMessage response = await this._client.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken
                )) {
                    if (response.IsSuccessStatusCode) {
                        return await readContent(response.Content, cancellationToken);
                    }
                    return this.MapError<T>(response);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
                return MetadataResult<T>.Failure(MetadataStatus.Canceled);
            }
            catch (OperationCanceledException ex) {
                // Nicht vom Benutzer abgebrochen: Die Zeitüberschreitung des HttpClient löst diese Ausnahme aus
                Log.Warning(ex, "Zeitüberschreitung bei einer Metadaten-Anfrage an {Host}.", request.RequestUri?.Host);
                return MetadataResult<T>.Failure(MetadataStatus.Timeout);
            }
            catch (HttpRequestException ex) {
                Log.Warning(ex, "Metadaten-Anfrage an {Host} fehlgeschlagen.", request.RequestUri?.Host);
                return MetadataResult<T>.Failure(MetadataStatus.Offline, detail: ex.Message);
            }
            catch (IOException ex) {
                Log.Warning(ex, "Antwort von {Host} konnte nicht gelesen werden.", request.RequestUri?.Host);
                return MetadataResult<T>.Failure(MetadataStatus.Offline, detail: ex.Message);
            }
            finally {
                this._gate.Release();
                request.Dispose();
            }
        }

        private MetadataResult<T> MapError<T>(HttpResponseMessage response) {
            int statusCode = (int)response.StatusCode;
            string? host = response.RequestMessage?.RequestUri?.Host;
            if (statusCode == 429) {
                int retryAfter = ReadRetryAfterSeconds(response);
                this._blockedUntilUtc = DateTime.UtcNow.AddSeconds(retryAfter);
                Log.Warning("{Host} bremst (HTTP 429), nächster Versuch frühestens in {Seconds} Sekunden.", host, retryAfter);
                return MetadataResult<T>.Failure(MetadataStatus.RateLimited, retryAfter);
            }
            if (statusCode == 404) {
                return MetadataResult<T>.Failure(MetadataStatus.NotFound);
            }
            Log.Warning("Metadaten-Anfrage an {Host} wurde mit HTTP {StatusCode} beantwortet.", host, statusCode);
            return MetadataResult<T>.Failure(MetadataStatus.ServiceError, detail: $"HTTP {statusCode}");
        }

        /// <summary>Wartezeit aus dem Header Retry-After (Sekunden oder Zeitpunkt), ohne Angabe eine Minute.</summary>
        private static int ReadRetryAfterSeconds(HttpResponseMessage response) {
            double seconds = DefaultRetryAfterSeconds;
            System.Net.Http.Headers.RetryConditionHeaderValue? retryAfter = response.Headers.RetryAfter;
            if (retryAfter?.Delta is TimeSpan delta) {
                seconds = delta.TotalSeconds;
            }
            else if (retryAfter?.Date is DateTimeOffset date) {
                seconds = (date - DateTimeOffset.UtcNow).TotalSeconds;
            }
            return (int)Math.Clamp(Math.Ceiling(seconds), 1, MaxRetryAfterSeconds);
        }
    }
}
