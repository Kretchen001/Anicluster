using Serilog;
using System.Net.Http;

namespace Anicluster.Metadata {

    /// <summary>
    /// Fasst die Online-Quellen (AniList, Jikan, Kitsu) hinter <see cref="IMetadataClient"/> zusammen. Es gibt einen gemeinsamen
    /// <see cref="HttpClient"/> mit Zeitlimit; jede Quelle hält ihren eigenen Abstand zwischen Anfragen ein (AniList bremst
    /// derzeit auf 30 Anfragen pro Minute, Jikan erlaubt etwa 60). Es wird nichts im Hintergrund geladen.
    /// </summary>
    public sealed class MetadataClient : IMetadataClient, IDisposable {

        /// <summary>Größere Cover werden abgelehnt</summary>
        private const long MaxCoverBytes = 10 * 1024 * 1024;

        // Jikan antwortet oft zäh und braucht mehr Zeit, außerdem kommt bei ihm gelegentlich ein Aussetzer, den ein zweiter Versuch behebt
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(15);
        private static readonly TimeSpan JikanTimeout = TimeSpan.FromSeconds(40);

        private readonly HttpClient _client;
        private readonly MetadataHttp _coverHttp;
        private readonly Dictionary<MetadataProviderKind, IMetadataProvider> _providers;

        public MetadataClient() {
            SocketsHttpHandler handler = new SocketsHttpHandler {
                PooledConnectionLifetime = TimeSpan.FromMinutes(5),
                MaxAutomaticRedirections = 3,
                ConnectTimeout = TimeSpan.FromSeconds(10)
            };
            this._client = new HttpClient(handler) { Timeout = Timeout.InfiniteTimeSpan };
            Version? version = typeof(MetadataClient).Assembly.GetName().Version;
            this._client.DefaultRequestHeaders.UserAgent.ParseAdd($"Anicluster/{version?.ToString(3) ?? "2"}");
            this._coverHttp = new MetadataHttp(this._client, TimeSpan.Zero, DefaultTimeout, 0);
            this._providers = new Dictionary<MetadataProviderKind, IMetadataProvider> {
                [MetadataProviderKind.AniList] = new AniListProvider(new MetadataHttp(this._client, TimeSpan.FromSeconds(2), DefaultTimeout, 0)),
                [MetadataProviderKind.Jikan] = new JikanProvider(new MetadataHttp(this._client, TimeSpan.FromSeconds(1), JikanTimeout, 2)),
                [MetadataProviderKind.Kitsu] = new KitsuProvider(new MetadataHttp(this._client, TimeSpan.FromMilliseconds(500), DefaultTimeout, 0))
            };
        }

        public Task<MetadataResult<IReadOnlyList<MetadataCandidate>>> SearchAsync(
            MetadataProviderKind provider,
            string query,
            CancellationToken cancellationToken
        ) {
            return this._providers[provider].SearchAsync(query, cancellationToken);
        }

        public Task<MetadataResult<MetadataCandidate>> GetByIdAsync(
            MetadataProviderKind provider,
            string externalId,
            CancellationToken cancellationToken
        ) {
            return this._providers[provider].GetByIdAsync(externalId, cancellationToken);
        }

        public Task<MetadataResult<byte[]>> DownloadCoverAsync(string url, CancellationToken cancellationToken) {
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri) || uri.Scheme != Uri.UriSchemeHttps) {
                Log.Warning("Die Cover-Adresse wurde abgelehnt (kein gültiges https): {Url}", url);
                return Task.FromResult(MetadataResult<byte[]>.Failure(MetadataStatus.NotFound));
            }
            return this._coverHttp.SendForBytesAsync(new HttpRequestMessage(HttpMethod.Get, uri), MaxCoverBytes, cancellationToken);
        }

        public void Dispose() {
            this._client.Dispose();
        }
    }
}
