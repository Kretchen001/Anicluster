using Serilog;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;

namespace Anicluster.Metadata {

    /// <summary>
    /// Kitsu (https://kitsu.io/api/edge, JSON:API). Lesen geht ohne Konto. Die Ids sind Zahlen, kommen aber als Text.
    /// </summary>
    internal sealed class KitsuProvider : IMetadataProvider {

        private const string BaseUrl = "https://kitsu.io/api/edge/anime";
        private const int ResultLimit = 8;

        private readonly MetadataHttp _http;

        public MetadataProviderKind Kind => MetadataProviderKind.Kitsu;

        public KitsuProvider(MetadataHttp http) {
            this._http = http;
        }

        public async Task<MetadataResult<IReadOnlyList<MetadataCandidate>>> SearchAsync(string query, CancellationToken cancellationToken) {
            // Die eckigen Klammern der JSON:API-Parameter werden mitkodiert
            string limit = ResultLimit.ToString(CultureInfo.InvariantCulture);
            string url = $"{BaseUrl}?filter%5Btext%5D={Uri.EscapeDataString(query)}&page%5Blimit%5D={limit}";
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
                Log.Error(ex, "Die Antwort von Kitsu konnte nicht gelesen werden.");
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
                Log.Error(ex, "Die Antwort von Kitsu konnte nicht gelesen werden.");
                return MetadataResult<MetadataCandidate>.Failure(MetadataStatus.ServiceError, detail: "Unerwartete Antwort");
            }
        }

        private Task<MetadataResult<string>> GetAsync(string url, CancellationToken cancellationToken) {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.ParseAdd("application/vnd.api+json");
            return this._http.SendForTextAsync(request, cancellationToken);
        }

        private MetadataCandidate? Parse(JsonElement item) {
            string? id = JsonReading.GetString(item, "id");
            if (id is null) {
                return null;
            }
            string? english = JsonReading.GetString(item, "attributes", "titles", "en");
            string? romaji = JsonReading.GetString(item, "attributes", "titles", "en_jp");
            string? japanese = JsonReading.GetString(item, "attributes", "titles", "ja_jp");
            string title = JsonReading.GetString(item, "attributes", "canonicalTitle") ?? romaji ?? english ?? japanese ?? "(ohne Titel)";
            string? alternative = new[] { english, japanese }
                .FirstOrDefault(x => x is not null && !string.Equals(x, title, StringComparison.OrdinalIgnoreCase));
            PartialDate? start = PartialDate.FromIso(JsonReading.GetString(item, "attributes", "startDate"));
            string? description = JsonReading.GetString(item, "attributes", "synopsis")
                ?? JsonReading.GetString(item, "attributes", "description");
            return new MetadataCandidate(
                this.Kind,
                id,
                title,
                alternative,
                MetadataText.DescribeFormat(
                    JsonReading.GetString(item, "attributes", "subtype") ?? JsonReading.GetString(item, "attributes", "showType")
                ),
                start?.Year,
                JsonReading.GetInt(item, "attributes", "episodeCount"),
                MetadataText.CleanDescription(description),
                JsonReading.GetString(item, "attributes", "posterImage", "original")
                    ?? JsonReading.GetString(item, "attributes", "posterImage", "large")
                    ?? JsonReading.GetString(item, "attributes", "posterImage", "medium"),
                start,
                PartialDate.FromIso(JsonReading.GetString(item, "attributes", "endDate")),
                null
            );
        }
    }
}
