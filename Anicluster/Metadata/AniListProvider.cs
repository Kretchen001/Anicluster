using Serilog;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace Anicluster.Metadata {

    /// <summary>
    /// AniList (GraphQL, https://graphql.anilist.co). Lesen geht ohne Konto. Pro Anfrage kommen Titel, Art, Episoden,
    /// Beschreibung, Cover und Start/Ende der Veröffentlichung mit.
    /// </summary>
    internal sealed class AniListProvider : IMetadataProvider {

        private const string Endpoint = "https://graphql.anilist.co";
        private const int ResultLimit = 8;

        private const string MediaFields = @"id idMal title { romaji english native } format seasonYear episodes description(asHtml: false)
            coverImage { extraLarge large } startDate { year month day } endDate { year month day }";

        private readonly MetadataHttp _http;

        public MetadataProviderKind Kind => MetadataProviderKind.AniList;

        public AniListProvider(MetadataHttp http) {
            this._http = http;
        }

        public async Task<MetadataResult<IReadOnlyList<MetadataCandidate>>> SearchAsync(string query, CancellationToken cancellationToken) {
            string graphQl = "query ($search: String) { Page(page: 1, perPage: " + ResultLimit.ToString(CultureInfo.InvariantCulture)
                + ") { media(search: $search, type: ANIME) { " + MediaFields + " } } }";
            MetadataResult<string> response = await this.PostAsync(graphQl, new { search = query }, cancellationToken);
            if (response.Value is null) {
                return response.AsFailure<IReadOnlyList<MetadataCandidate>>();
            }
            try {
                using (JsonDocument document = JsonDocument.Parse(response.Value)) {
                    List<MetadataCandidate> candidates = [];
                    JsonElement? media = JsonReading.Get(document.RootElement, "data", "Page", "media");
                    if (media is { ValueKind: JsonValueKind.Array } list) {
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
                Log.Error(ex, "Die Antwort von AniList konnte nicht gelesen werden.");
                return MetadataResult<IReadOnlyList<MetadataCandidate>>.Failure(MetadataStatus.ServiceError, detail: "Unerwartete Antwort");
            }
        }

        public async Task<MetadataResult<MetadataCandidate>> GetByIdAsync(string externalId, CancellationToken cancellationToken) {
            if (!int.TryParse(externalId, NumberStyles.None, CultureInfo.InvariantCulture, out int id)) {
                return MetadataResult<MetadataCandidate>.Failure(MetadataStatus.NotFound);
            }
            string graphQl = "query ($id: Int) { Media(id: $id, type: ANIME) { " + MediaFields + " } }";
            MetadataResult<string> response = await this.PostAsync(graphQl, new { id }, cancellationToken);
            if (response.Value is null) {
                return response.AsFailure<MetadataCandidate>();
            }
            try {
                using (JsonDocument document = JsonDocument.Parse(response.Value)) {
                    JsonElement? media = JsonReading.Get(document.RootElement, "data", "Media");
                    MetadataCandidate? candidate = media is { ValueKind: JsonValueKind.Object } item ? this.Parse(item) : null;
                    return candidate is null
                        ? MetadataResult<MetadataCandidate>.Failure(MetadataStatus.NotFound)
                        : MetadataResult<MetadataCandidate>.Success(candidate);
                }
            }
            catch (JsonException ex) {
                Log.Error(ex, "Die Antwort von AniList konnte nicht gelesen werden.");
                return MetadataResult<MetadataCandidate>.Failure(MetadataStatus.ServiceError, detail: "Unerwartete Antwort");
            }
        }

        private Task<MetadataResult<string>> PostAsync(string graphQl, object variables, CancellationToken cancellationToken) {
            string body = JsonSerializer.Serialize(new { query = graphQl, variables });
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, Endpoint) {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };
            request.Headers.Accept.ParseAdd("application/json");
            return this._http.SendForTextAsync(request, cancellationToken);
        }

        private MetadataCandidate? Parse(JsonElement item) {
            int? id = JsonReading.GetInt(item, "id");
            if (id is null) {
                return null;
            }
            string? romaji = JsonReading.GetString(item, "title", "romaji");
            string? english = JsonReading.GetString(item, "title", "english");
            string? native = JsonReading.GetString(item, "title", "native");
            string title = romaji ?? english ?? native ?? "(ohne Titel)";
            string? alternative = new[] { english, native }
                .FirstOrDefault(x => x is not null && !string.Equals(x, title, StringComparison.OrdinalIgnoreCase));
            PartialDate? start = JsonReading.GetDateParts(item, "startDate");
            return new MetadataCandidate(
                this.Kind,
                id.Value.ToString(CultureInfo.InvariantCulture),
                title,
                alternative,
                MetadataText.DescribeFormat(JsonReading.GetString(item, "format")),
                JsonReading.GetInt(item, "seasonYear") ?? start?.Year,
                JsonReading.GetInt(item, "episodes"),
                MetadataText.CleanDescription(JsonReading.GetString(item, "description")),
                JsonReading.GetString(item, "coverImage", "extraLarge") ?? JsonReading.GetString(item, "coverImage", "large"),
                start,
                JsonReading.GetDateParts(item, "endDate"),
                JsonReading.GetInt(item, "idMal")
            );
        }
    }
}
