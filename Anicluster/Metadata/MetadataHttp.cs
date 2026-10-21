using Serilog;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;

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

        private const string ResetDetail = "Verbindung vom Server abgebrochen";

        private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(2);

        private readonly HttpClient _client;
        private readonly TimeSpan _minimumGap;
        private readonly TimeSpan _timeout;
        private readonly int _retries;
        private readonly SemaphoreSlim _gate = new SemaphoreSlim(1, 1);
        private DateTime _lastRequestUtc = DateTime.MinValue;
        private DateTime _blockedUntilUtc = DateTime.MinValue;

        /// <param name="timeout">Zeitgrenze pro Versuch</param>
        /// <param name="retries">Wie oft nach Zeitüberschreitung oder Serverfehler (HTTP 5xx) noch einmal versucht wird</param>
        public MetadataHttp(HttpClient client, TimeSpan minimumGap, TimeSpan timeout, int retries) {
            this._client = client;
            this._minimumGap = minimumGap;
            this._timeout = timeout;
            this._retries = retries;
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
                // Der Inhalt wird einmal gelesen, damit die Anfrage bei einem erneuten Versuch neu gebaut werden kann
                byte[]? body = request.Content is null ? null : await request.Content.ReadAsByteArrayAsync(cancellationToken);
                MetadataResult<T> result = MetadataResult<T>.Failure(MetadataStatus.ServiceError);
                for (int attempt = 0; attempt <= this._retries; attempt++) {
                    if (attempt > 0) {
                        Log.Information("Metadaten-Anfrage an {Host} wird wiederholt.", request.RequestUri?.Host);
                        await Task.Delay(RetryDelay, cancellationToken);
                    }
                    TimeSpan wait = this._lastRequestUtc + this._minimumGap - DateTime.UtcNow;
                    if (wait > TimeSpan.Zero) {
                        await Task.Delay(wait, cancellationToken);
                    }
                    this._lastRequestUtc = DateTime.UtcNow;
                    result = await this.AttemptAsync(request, body, readContent, cancellationToken);
                    if (!IsRetryable(result)) {
                        return result;
                    }
                }
                return result;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
                return MetadataResult<T>.Failure(MetadataStatus.Canceled);
            }
            finally {
                this._gate.Release();
                request.Dispose();
            }
        }

        /// <summary>Zeitüberschreitungen und Serverfehler (HTTP 5xx) sind oft nur kurze Aussetzer der Quelle.</summary>
        private static bool IsRetryable<T>(MetadataResult<T> result) {
            return result.Status == MetadataStatus.Timeout
                || (result.Status == MetadataStatus.Offline && result.Detail is not null && result.Detail.StartsWith(ResetDetail))
                || (result.Status == MetadataStatus.ServiceError && result.Detail is not null && result.Detail.StartsWith("HTTP 5"));
        }

        /// <summary>Ein einzelner Versuch mit eigener Zeitgrenze, die Anfrage wird dafür aus der Vorlage neu gebaut.</summary>
        private async Task<MetadataResult<T>> AttemptAsync<T>(
            HttpRequestMessage template,
            byte[]? body,
            Func<HttpContent, CancellationToken, Task<MetadataResult<T>>> readContent,
            CancellationToken cancellationToken
        ) {
            using (CancellationTokenSource timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            using (HttpRequestMessage message = CloneRequest(template, body)) {
                timeoutSource.CancelAfter(this._timeout);
                try {
                    using (HttpResponseMessage response = await this._client.SendAsync(
                        message,
                        HttpCompletionOption.ResponseHeadersRead,
                        timeoutSource.Token
                    )) {
                        if (response.IsSuccessStatusCode) {
                            return await readContent(response.Content, timeoutSource.Token);
                        }
                        return await this.MapErrorAsync<T>(response, timeoutSource.Token);
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
                    return MetadataResult<T>.Failure(MetadataStatus.Canceled);
                }
                catch (OperationCanceledException ex) {
                    // Nicht vom Benutzer abgebrochen: Die Zeitgrenze dieses Versuchs ist abgelaufen
                    Log.Warning(ex, "Zeitüberschreitung bei einer Metadaten-Anfrage an {Host}.", template.RequestUri?.Host);
                    return MetadataResult<T>.Failure(MetadataStatus.Timeout);
                }
                catch (HttpRequestException ex) {
                    Log.Warning(ex, "Metadaten-Anfrage an {Host} fehlgeschlagen.", template.RequestUri?.Host);
                    // Schließt der Server die Verbindung (auch beim SSL-Aufbau), ist ein zweiter Versuch mit neuer Verbindung sinnvoll
                    string detail = IsConnectionReset(ex) ? ResetDetail + ": " + ex.Message : ex.Message;
                    return MetadataResult<T>.Failure(MetadataStatus.Offline, detail: detail);
                }
                catch (IOException ex) {
                    Log.Warning(ex, "Antwort von {Host} konnte nicht gelesen werden.", template.RequestUri?.Host);
                    return MetadataResult<T>.Failure(MetadataStatus.Offline, detail: ex.Message);
                }
            }
        }

        private static bool IsConnectionReset(Exception exception) {
            for (Exception? current = exception; current is not null; current = current.InnerException) {
                if (current is SocketException { SocketErrorCode: SocketError.ConnectionReset or SocketError.ConnectionAborted }) {
                    return true;
                }
            }
            return false;
        }

        private static HttpRequestMessage CloneRequest(HttpRequestMessage template, byte[]? body) {
            HttpRequestMessage clone = new HttpRequestMessage(template.Method, template.RequestUri);
            foreach (KeyValuePair<string, IEnumerable<string>> header in template.Headers) {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
            if (body is not null && template.Content is not null) {
                ByteArrayContent content = new ByteArrayContent(body);
                foreach (KeyValuePair<string, IEnumerable<string>> header in template.Content.Headers) {
                    if (!string.Equals(header.Key, "Content-Length", StringComparison.OrdinalIgnoreCase)) {
                        content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }
                clone.Content = content;
            }
            return clone;
        }

        private async Task<MetadataResult<T>> MapErrorAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken) {
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
            // Der Anfang der Antwort steht im Log, damit sich Fehler der Quelle (zum Beispiel bei GraphQL) nachvollziehen lassen
            string body = string.Empty;
            try {
                body = await response.Content.ReadAsStringAsync(cancellationToken);
                if (body.Length > 300) {
                    body = body.Substring(0, 300);
                }
            }
            catch (Exception ex) when (ex is HttpRequestException or IOException or OperationCanceledException) {
                body = string.Empty;
            }
            Log.Warning("Metadaten-Anfrage an {Host} wurde mit HTTP {StatusCode} beantwortet: {Body}", host, statusCode, body);
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
