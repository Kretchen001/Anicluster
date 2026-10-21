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

        // Grundfelder, die immer abgefragt werden
        private const string BasicFields = @"id idMal title { romaji english native } format seasonYear episodes description(asHtml: false)
            coverImage { extraLarge large } startDate { year month day } endDate { year month day }";

        // Mit Studios und Mitarbeitern: schwerere Abfrage, bei der AniList manchmal mit einem Serverfehler antwortet
        private const string MediaFields = BasicFields
            + @" studios { edges { isMain node { name isAnimationStudio } } } staff(perPage: 25) { edges { role node { name { full } } } }";

        private readonly MetadataHttp _http;

        public MetadataProviderKind Kind => MetadataProviderKind.AniList;

        public AniListProvider(MetadataHttp http) {
            this._http = http;
        }

        public async Task<MetadataResult<IReadOnlyList<MetadataCandidate>>> SearchAsync(string query, CancellationToken cancellationToken) {
            string Build(string fields) {
                return "query ($search: String) { Page(page: 1, perPage: " + ResultLimit.ToString(CultureInfo.InvariantCulture)
                    + ") { media(search: $search, type: ANIME) { " + fields + " } } }";
            }
            MetadataResult<string> response = await this.PostWithFallbackAsync(Build, new { search = query }, cancellationToken);
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
            string Build(string fields) {
                return "query ($id: Int) { Media(id: $id, type: ANIME) { " + fields + " } }";
            }
            MetadataResult<string> response = await this.PostWithFallbackAsync(Build, new { id }, cancellationToken);
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

        /// <summary>Fragt mit allen Feldern an und bei einem Serverfehler noch einmal nur mit den Grundfeldern.</summary>
        private async Task<MetadataResult<string>> PostWithFallbackAsync(
            Func<string, string> build,
            object variables,
            CancellationToken cancellationToken
        ) {
            MetadataResult<string> response = await this.PostAsync(build(MediaFields), variables, cancellationToken);
            if (response.Status == MetadataStatus.ServiceError || response.Status == MetadataStatus.Timeout) {
                Log.Warning("AniList antwortet auf die Abfrage mit Studios und Mitarbeitern nicht, es folgt die Abfrage mit den Grundfeldern.");
                response = await this.PostAsync(build(BasicFields), variables, cancellationToken);
            }
            return response;
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
                JsonReading.GetInt(item, "idMal"),
                ReadStudios(item),
                null,
                ReadAuthors(item),
                native
            );
        }

        /// <summary>Studios zuerst, danach die übrigen beteiligten Firmen (Produzenten).</summary>
        private static string? ReadStudios(JsonElement item) {
            List<(bool IsStudio, string Name)> entries = [];
            JsonElement? edges = JsonReading.Get(item, "studios", "edges");
            if (edges is { ValueKind: JsonValueKind.Array } list) {
                foreach (JsonElement edge in list.EnumerateArray()) {
                    string? name = JsonReading.GetString(edge, "node", "name");
                    if (name is not null) {
                        bool isStudio = JsonReading.Get(edge, "node", "isAnimationStudio") is { ValueKind: JsonValueKind.True };
                        entries.Add((isStudio, name));
                    }
                }
            }
            return JsonReading.JoinNames(entries.OrderByDescending(x => x.IsStudio).Select(x => x.Name));
        }

        /// <summary>Mitarbeiter mit der Rolle „Original Creator“ oder „Original Story“ gelten als Autor.</summary>
        private static string? ReadAuthors(JsonElement item) {
            List<string> names = [];
            JsonElement? edges = JsonReading.Get(item, "staff", "edges");
            if (edges is { ValueKind: JsonValueKind.Array } list) {
                foreach (JsonElement edge in list.EnumerateArray()) {
                    string? role = JsonReading.GetString(edge, "role");
                    string? name = JsonReading.GetString(edge, "node", "name", "full");
                    if (role is not null && name is not null && role.Contains("Original", StringComparison.OrdinalIgnoreCase)) {
                        names.Add(name);
                    }
                }
            }
            return JsonReading.JoinNames(names);
        }
    }
}
