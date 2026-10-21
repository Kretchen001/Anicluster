using System.Globalization;
using System.Text.Json;

namespace Anicluster.Metadata {

    /// <summary>Liest verschachtelte JSON-Werte, ohne bei fehlenden oder anders typisierten Feldern Ausnahmen zu werfen.</summary>
    internal static class JsonReading {

        /// <summary>Das Element am Pfad, <c>null</c> wenn es fehlt oder JSON-<c>null</c> ist.</summary>
        public static JsonElement? Get(JsonElement element, params string[] path) {
            JsonElement current = element;
            foreach (string name in path) {
                if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(name, out JsonElement next)) {
                    return null;
                }
                current = next;
            }
            return current.ValueKind == JsonValueKind.Null ? null : current;
        }

        /// <summary>Text am Pfad, ein leerer Text ergibt <c>null</c>.</summary>
        public static string? GetString(JsonElement element, params string[] path) {
            JsonElement? value = Get(element, path);
            if (value is { ValueKind: JsonValueKind.String } text) {
                string? result = text.GetString();
                return string.IsNullOrWhiteSpace(result) ? null : result.Trim();
            }
            return null;
        }

        /// <summary>Ganze Zahl am Pfad (auch als Text geliefert), sonst <c>null</c>.</summary>
        public static int? GetInt(JsonElement element, params string[] path) {
            JsonElement? value = Get(element, path);
            if (value is { ValueKind: JsonValueKind.Number } number && number.TryGetInt32(out int fromNumber)) {
                return fromNumber;
            }
            if (value is { ValueKind: JsonValueKind.String } text
                && int.TryParse(text.GetString(), NumberStyles.None, CultureInfo.InvariantCulture, out int fromText)) {
                return fromText;
            }
            return null;
        }

        /// <summary>Datum aus einem Objekt mit year, month und day (jeweils auch <c>null</c>), <c>null</c> ohne Jahr.</summary>
        public static PartialDate? GetDateParts(JsonElement element, params string[] path) {
            JsonElement? parts = Get(element, path);
            if (parts is not { ValueKind: JsonValueKind.Object } value) {
                return null;
            }
            int? year = GetInt(value, "year");
            return year is null ? null : new PartialDate(year, GetInt(value, "month"), GetInt(value, "day"));
        }
    }
}
