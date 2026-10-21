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
        private const string Include = "&include=animeProductions.producer%2Cstaff.person";

        private readonly MetadataHttp _http;

        public MetadataProviderKind Kind => MetadataProviderKind.Kitsu;

        public KitsuProvider(MetadataHttp http) {
            this._http = http;
        }

        public async Task<MetadataResult<IReadOnlyList<MetadataCandidate>>> SearchAsync(string query, CancellationToken cancellationToken) {
            // Die eckigen Klammern der JSON:API-Parameter werden mitkodiert
            string limit = ResultLimit.ToString(CultureInfo.InvariantCulture);
            string url = $"{BaseUrl}?filter%5Btext%5D={Uri.EscapeDataString(query)}&page%5Blimit%5D={limit}{Include}";
            MetadataResult<string> response = await this.GetAsync(url, cancellationToken);
            if (response.Value is null) {
                return response.AsFailure<IReadOnlyList<MetadataCandidate>>();
            }
            try {
                using (JsonDocument document = JsonDocument.Parse(response.Value)) {
                    List<MetadataCandidate> candidates = [];
                    JsonElement? data = JsonReading.Get(document.RootElement, "data");
                    Dictionary<string, JsonElement> included = IndexIncluded(document.RootElement);
                    if (data is { ValueKind: JsonValueKind.Array } list) {
                        foreach (JsonElement item in list.EnumerateArray()) {
                            MetadataCandidate? candidate = this.Parse(item, included);
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
            string url = $"{BaseUrl}/{id.ToString(CultureInfo.InvariantCulture)}?{Include.TrimStart('&')}";
            MetadataResult<string> response = await this.GetAsync(url, cancellationToken);
            if (response.Value is null) {
                return response.AsFailure<MetadataCandidate>();
            }
            try {
                using (JsonDocument document = JsonDocument.Parse(response.Value)) {
                    JsonElement? data = JsonReading.Get(document.RootElement, "data");
                    Dictionary<string, JsonElement> included = IndexIncluded(document.RootElement);
                    MetadataCandidate? candidate = data is { ValueKind: JsonValueKind.Object } item ? this.Parse(item, included) : null;
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

        private MetadataCandidate? Parse(JsonElement item, Dictionary<string, JsonElement> included) {
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
                null,
                ReadProductions(item, included, "studio", "producer"),
                ReadProductions(item, included, "licensor", "serialization"),
                ReadAuthors(item, included),
                japanese
            );
        }

        /// <summary>Alle mitgelieferten Einträge (<c>included</c>) nach Typ und Id, damit Verweise schnell aufgelöst werden.</summary>
        private static Dictionary<string, JsonElement> IndexIncluded(JsonElement root) {
            Dictionary<string, JsonElement> index = new Dictionary<string, JsonElement>();
            JsonElement? included = JsonReading.Get(root, "included");
            if (included is { ValueKind: JsonValueKind.Array } list) {
                foreach (JsonElement entry in list.EnumerateArray()) {
                    string? type = JsonReading.GetString(entry, "type");
                    string? id = JsonReading.GetString(entry, "id");
                    if (type is not null && id is not null) {
                        index[type + ":" + id] = entry;
                    }
                }
            }
            return index;
        }

        /// <summary>Verweise (<c>type</c> und <c>id</c>) einer Beziehung des Eintrags, z. B. <c>animeProductions</c>.</summary>
        private static List<JsonElement> ResolveRelation(
            JsonElement owner, string relation, Dictionary<string, JsonElement> included
        ) {
            List<JsonElement> result = [];
            JsonElement? refs = JsonReading.Get(owner, "relationships", relation, "data");
            if (refs is { ValueKind: JsonValueKind.Array } list) {
                foreach (JsonElement reference in list.EnumerateArray()) {
                    string? type = JsonReading.GetString(reference, "type");
                    string? id = JsonReading.GetString(reference, "id");
                    if (type is not null && id is not null && included.TryGetValue(type + ":" + id, out JsonElement entry)) {
                        result.Add(entry);
                    }
                }
            }
            return result;
        }

        private static string? ResolveName(JsonElement owner, string relation, Dictionary<string, JsonElement> included) {
            string? type = JsonReading.GetString(owner, "relationships", relation, "data", "type");
            string? id = JsonReading.GetString(owner, "relationships", relation, "data", "id");
            if (type is not null && id is not null && included.TryGetValue(type + ":" + id, out JsonElement target)) {
                return JsonReading.GetString(target, "attributes", "name");
            }
            return null;
        }

        /// <summary>Firmen mit einer der Rollen (z. B. studio, producer, licensor) aus den Produktionen des Animes.</summary>
        private static string? ReadProductions(JsonElement item, Dictionary<string, JsonElement> included, params string[] roles) {
            List<string> names = [];
            foreach (JsonElement production in ResolveRelation(item, "animeProductions", included)) {
                string? role = JsonReading.GetString(production, "attributes", "role");
                string? name = ResolveName(production, "producer", included);
                if (role is not null && name is not null && roles.Contains(role, StringComparer.OrdinalIgnoreCase)) {
                    names.Add(name);
                }
            }
            return JsonReading.JoinNames(names);
        }

        /// <summary>Mitarbeiter mit der Rolle „Original Creator“ oder „Original Story“ gelten als Autor.</summary>
        private static string? ReadAuthors(JsonElement item, Dictionary<string, JsonElement> included) {
            List<string> names = [];
            foreach (JsonElement member in ResolveRelation(item, "staff", included)) {
                string? role = JsonReading.GetString(member, "attributes", "role");
                string? name = ResolveName(member, "person", included);
                if (role is not null && name is not null && role.Contains("Original", StringComparison.OrdinalIgnoreCase)) {
                    names.Add(name);
                }
            }
            return JsonReading.JoinNames(names);
        }
    }
}
