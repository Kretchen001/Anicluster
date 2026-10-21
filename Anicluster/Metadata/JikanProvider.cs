using Serilog;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;

namespace Anicluster.Metadata {

    /// <summary>
    /// Jikan (https://api.jikan.moe/v4), eine inoffizielle REST-Schnittstelle zu MyAnimeList. Lesen geht ohne Konto.
    /// Die Ids sind die MyAnimeList-Ids.
    /// </summary>
    internal sealed class JikanProvider : IMetadataProvider {

        private const string BaseUrl = "https://api.jikan.moe/v4/anime";
        private const int ResultLimit = 8;

        private readonly MetadataHttp _http;

        public MetadataProviderKind Kind => MetadataProviderKind.Jikan;

        public JikanProvider(MetadataHttp http) {
            this._http = http;
        }

        public async Task<MetadataResult<IReadOnlyList<MetadataCandidate>>> SearchAsync(string query, CancellationToken cancellationToken) {
            string url = $"{BaseUrl}?q={Uri.EscapeDataString(query)}&limit={ResultLimit.ToString(CultureInfo.InvariantCulture)}";
            MetadataResult<string> response = await this.GetAsync(url, cancellationToken);
            if (response.Value is null) {
                return response.AsFailure<IReadOnlyList<MetadataCandidate>>();
            }
            try {
                using (JsonDocument document = JsonDocument.Parse(response.Value)) {
                    List<MetadataCandidate> candidates = [];
                    JsonElement? data = JsonReading.Get(document.RootElement, "data");
                    if (data is { ValueKind: JsonValueKind.Array } list) {
                        foreach (JsonElement item in list.EnumerateArray()) {
                            MetadataCandidate? candidate = this.Parse(item);
                            if (candidate is not null) {
                                candidates.Add(candidate);
                            }
                        }
                    }
                    return MetadataResult<IReadOnlyList<MetadataCandidate>>.Success(candidates);
                }
            }
            catch (JsonException ex) {
                Log.Error(ex, "Die Antwort von Jikan konnte nicht gelesen werden.");
                return MetadataResult<IReadOnlyList<MetadataCandidate>>.Failure(MetadataStatus.ServiceError, detail: "Unerwartete Antwort");
            }
        }

        public async Task<MetadataResult<MetadataCandidate>> GetByIdAsync(string externalId, CancellationToken cancellationToken) {
            if (!int.TryParse(externalId, NumberStyles.None, CultureInfo.InvariantCulture, out int id)) {
                return MetadataResult<MetadataCandidate>.Failure(MetadataStatus.NotFound);
            }
            MetadataResult<string> response = await this.GetAsync($"{BaseUrl}/{id.ToString(CultureInfo.InvariantCulture)}", cancellationToken);
            if (response.Value is null) {
                return response.AsFailure<MetadataCandidate>();
            }
            try {
                using (JsonDocument document = JsonDocument.Parse(response.Value)) {
                    JsonElement? data = JsonReading.Get(document.RootElement, "data");
                    MetadataCandidate? candidate = data is { ValueKind: JsonValueKind.Object } item ? this.Parse(item) : null;
                    return candidate is null
                        ? MetadataResult<MetadataCandidate>.Failure(MetadataStatus.NotFound)
                        : MetadataResult<MetadataCandidate>.Success(candidate);
                }
            }
            catch (JsonException ex) {
                Log.Error(ex, "Die Antwort von Jikan konnte nicht gelesen werden.");
                return MetadataResult<MetadataCandidate>.Failure(MetadataStatus.ServiceError, detail: "Unerwartete Antwort");
            }
        }

        private Task<MetadataResult<string>> GetAsync(string url, CancellationToken cancellationToken) {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.ParseAdd("application/json");
            return this._http.SendForTextAsync(request, cancellationToken);
        }

        private MetadataCandidate? Parse(JsonElement item) {
            int? id = JsonReading.GetInt(item, "mal_id");
            if (id is null) {
                return null;
            }
            string? english = JsonReading.GetString(item, "title_english");
            string? japanese = JsonReading.GetString(item, "title_japanese");
            string title = JsonReading.GetString(item, "title") ?? english ?? japanese ?? "(ohne Titel)";
            string? alternative = new[] { english, japanese }
                .FirstOrDefault(x => x is not null && !string.Equals(x, title, StringComparison.OrdinalIgnoreCase));
            PartialDate? start = JsonReading.GetDateParts(item, "aired", "prop", "from");
            return new MetadataCandidate(
                this.Kind,
                id.Value.ToString(CultureInfo.InvariantCulture),
                title,
                alternative,
                MetadataText.DescribeFormat(JsonReading.GetString(item, "type")),
                JsonReading.GetInt(item, "year") ?? start?.Year,
                JsonReading.GetInt(item, "episodes"),
                MetadataText.CleanDescription(JsonReading.GetString(item, "synopsis")),
                JsonReading.GetString(item, "images", "jpg", "large_image_url") ?? JsonReading.GetString(item, "images", "jpg", "image_url"),
                start,
                JsonReading.GetDateParts(item, "aired", "prop", "to"),
                id
            );
        }
    }
}
