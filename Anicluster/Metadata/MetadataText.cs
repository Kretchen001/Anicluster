using System.Net;
using System.Text.RegularExpressions;

namespace Anicluster.Metadata {

    /// <summary>Bereinigt Texte der Online-Quellen für die Anzeige und Speicherung.</summary>
    internal static class MetadataText {

        private static readonly Regex LineBreakPattern = new Regex(@"<\s*(br\s*/?|/p)\s*>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex TagPattern = new Regex(@"<[^>]+>", RegexOptions.Compiled);
        private static readonly Regex BlankLinesPattern = new Regex(@"\n{3,}", RegexOptions.Compiled);
        private static readonly Regex MalMarkerPattern = new Regex(
            @"\s*\[Written by MAL Rewrite\]\s*$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        private static readonly Dictionary<string, string> Formats = new Dictionary<string, string> {
            ["TV"] = "TV-Serie",
            ["TV_SHORT"] = "TV (kurz)",
            ["TV_SPECIAL"] = "TV-Special",
            ["MOVIE"] = "Film",
            ["SPECIAL"] = "Special",
            ["OVA"] = "OVA",
            ["ONA"] = "ONA",
            ["MUSIC"] = "Musik",
            ["CM"] = "Werbung",
            ["PV"] = "Trailer"
        };

        /// <summary>
        /// Macht aus dem Text einer Quelle reinen Text: Zeilenumbrüche aus HTML bleiben erhalten, andere Tags verschwinden,
        /// HTML-Zeichen werden aufgelöst. <c>null</c> bei leerem Text.
        /// </summary>
        public static string? CleanDescription(string? source) {
            if (string.IsNullOrWhiteSpace(source)) {
                return null;
            }
            string text = LineBreakPattern.Replace(source, "\n");
            text = TagPattern.Replace(text, string.Empty);
            text = WebUtility.HtmlDecode(text);
            text = text.Replace("\r\n", "\n").Replace('\r', '\n');
            text = MalMarkerPattern.Replace(text, string.Empty);
            text = BlankLinesPattern.Replace(text, "\n\n").Trim();
            return text.Length == 0 ? null : text.Replace("\n", "\r\n");
        }

        /// <summary>Deutsche Bezeichnung der Art eines Eintrags, unbekannte Werte bleiben wie geliefert.</summary>
        public static string? DescribeFormat(string? format) {
            if (string.IsNullOrWhiteSpace(format)) {
                return null;
            }
            string key = format.Trim().ToUpperInvariant().Replace(' ', '_');
            return Formats.TryGetValue(key, out string? description) ? description : format.Trim();
        }
    }
}
