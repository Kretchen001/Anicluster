using System.Globalization;

namespace Anicluster.Metadata {

    /// <summary>
    /// Ein Datum, bei dem Monat oder Tag fehlen können (Online-Quellen kennen oft nur das Jahr).
    /// Nur ein vollständiges Datum kann in ein <see cref="DateTime"/> umgewandelt werden.
    /// </summary>
    public record PartialDate(int? Year, int? Month, int? Day) {

        /// <summary>Sind Jahr, Monat und Tag vorhanden und ergeben ein gültiges Datum?</summary>
        public bool IsComplete => this.ToDateTime() is not null;

        /// <summary>Das Datum, <c>null</c> wenn es unvollständig oder ungültig ist.</summary>
        public DateTime? ToDateTime() {
            if (this.Year is not int year || this.Month is not int month || this.Day is not int day) {
                return null;
            }
            if (year < 1 || year > 9999 || month < 1 || month > 12 || day < 1 || day > DateTime.DaysInMonth(year, month)) {
                return null;
            }
            return new DateTime(year, month, day);
        }

        /// <summary>Deutsche Darstellung, so genau wie bekannt: "07.04.2013", "04.2013" oder "2013".</summary>
        public string ToDisplayText() {
            if (this.Year is not int year) {
                return string.Empty;
            }
            if (this.Month is not int month) {
                return year.ToString("D4", CultureInfo.InvariantCulture);
            }
            if (this.Day is not int day) {
                return $"{month:D2}.{year:D4}";
            }
            return $"{day:D2}.{month:D2}.{year:D4}";
        }

        /// <summary>Liest "2013", "2013-04", "2013-04-07" oder ein ISO-Datum mit Uhrzeit. <c>null</c> ohne lesbares Jahr.</summary>
        public static PartialDate? FromIso(string? text) {
            if (string.IsNullOrWhiteSpace(text)) {
                return null;
            }
            string[] parts = text.Trim().Split('T')[0].Split('-');
            int? year = ParsePart(parts, 0);
            return year is null ? null : new PartialDate(year, ParsePart(parts, 1), ParsePart(parts, 2));
        }

        private static int? ParsePart(string[] parts, int index) {
            return index < parts.Length && int.TryParse(parts[index], NumberStyles.None, CultureInfo.InvariantCulture, out int value)
                ? value
                : null;
        }
    }
}
