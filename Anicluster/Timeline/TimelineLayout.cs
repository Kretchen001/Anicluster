using Modells.Anime;
using System.Globalization;

namespace Anicluster.Timeline {

    /// <summary>Ein Balken in einer Zeile: Position und Breite in Pixeln plus Text für den Tooltip.</summary>
    public record TimelineBarLayout(double Left, double Width, string Tooltip);

    /// <summary>Eine Pause innerhalb eines Balkens.</summary>
    public record TimelineGapLayout(double Left, double Width);

    /// <summary>Eine Zeile des Zeitstrahls: ein Anime mit seinen Balken und Pausen.</summary>
    public record TimelineRowLayout(TimelineEntry Entry, IReadOnlyList<TimelineBarLayout> Bars, IReadOnlyList<TimelineGapLayout> Gaps);

    /// <summary>Eine Jahresmarke der Zeitachse.</summary>
    public record TimelineTick(double Left, string Label);

    /// <summary>Das fertige Layout des Zeitstrahls.</summary>
    public record TimelineLayoutResult(
        IReadOnlyList<TimelineRowLayout> Rows,
        IReadOnlyList<TimelineTick> Ticks,
        double Width,
        double TodayLeft,
        int WithoutPeriodCount);

    /// <summary>
    /// Berechnet aus den Veröffentlichungszeiträumen die Pixelpositionen des Zeitstrahls. Reine Berechnung ohne WPF:
    /// die Zeitachse beginnt am 1. Januar des frühesten Jahres, ein Jahr ist <c>pixelsPerYear</c> Pixel breit.
    /// </summary>
    public static class TimelineLayout {

        /// <summary>Kleinste Breite eines Balkens, damit auch kurze Zeiträume und offene Enden sichtbar bleiben</summary>
        private const double MinBarWidth = 8.0;

        /// <summary>Platz rechts neben dem letzten Jahr</summary>
        private const double RightPadding = 40.0;

        private const double DaysPerYear = 365.25;

        /// <param name="entries">Alle Animes</param>
        /// <param name="pixelsPerYear">Breite eines Jahres in Pixeln (Zoom)</param>
        /// <param name="today">Heutiges Datum (Heute-Linie, spätestes Ende der Achse)</param>
        /// <param name="search">Teil des Namens, nach dem die Zeilen gefiltert werden (leer = alle)</param>
        public static TimelineLayoutResult Calculate(
            IReadOnlyList<TimelineEntry> entries, double pixelsPerYear, DateTime today, string search) {
            List<(TimelineEntry Entry, List<(string Label, TimelineSpan Span)> Parts)> dated = [];
            foreach (TimelineEntry entry in entries) {
                List<(string Label, TimelineSpan Span)> parts = GetParts(entry);
                if (parts.Count > 0) {
                    dated.Add((entry, parts));
                }
            }
            int withoutPeriod = entries.Count - dated.Count;
            if (dated.Count == 0) {
                return new TimelineLayoutResult([], [], 0, 0, withoutPeriod);
            }

            // Die Achse richtet sich nach allen Animes mit Zeitraum, damit sie beim Suchen nicht springt.
            DateTime first = dated.Min(x => x.Parts.Min(p => p.Span.Start));
            DateTime last = dated.Max(x => x.Parts.Max(p => p.Span.End ?? p.Span.Start));
            if (last < today) {
                last = today;
            }
            DateTime origin = new DateTime(first.Year, 1, 1);
            double width = ToX(new DateTime(last.Year + 1, 1, 1), origin, pixelsPerYear) + RightPadding;

            string filter = search.Trim();
            List<TimelineRowLayout> rows = dated
                .Where(x => filter.Length == 0 || x.Entry.Name.Contains(filter, StringComparison.CurrentCultureIgnoreCase))
                .OrderBy(x => x.Parts.Min(p => p.Span.Start))
                .ThenBy(x => x.Entry.Name, StringComparer.CurrentCultureIgnoreCase)
                .Select(x => CreateRow(x.Entry, x.Parts, origin, pixelsPerYear))
                .ToList();
            return new TimelineLayoutResult(rows, CreateTicks(origin, last.Year, pixelsPerYear), width, ToX(today, origin, pixelsPerYear), withoutPeriod);
        }

        /// <summary>Die Teile eines Animes, die als Balken gezeichnet werden: seine Staffeln, sonst der Gesamtzeitraum.</summary>
        private static List<(string Label, TimelineSpan Span)> GetParts(TimelineEntry entry) {
            List<(string Label, TimelineSpan Span)> parts = [];
            foreach (TimelineSeason season in entry.Seasons) {
                parts.Add(($"Staffel {season.Number}", season.Span));
            }
            if (parts.Count == 0 && entry.Run is not null) {
                parts.Add((string.Empty, entry.Run));
            }
            return parts;
        }

        private static TimelineRowLayout CreateRow(
            TimelineEntry entry, List<(string Label, TimelineSpan Span)> parts, DateTime origin, double pixelsPerYear) {
            List<TimelineBarLayout> bars = [];
            List<TimelineGapLayout> gaps = [];
            foreach ((string label, TimelineSpan span) in parts) {
                double left = ToX(span.Start, origin, pixelsPerYear);
                double right = span.End.HasValue ? ToX(span.End.Value, origin, pixelsPerYear) : left;
                double barWidth = Math.Max(right - left, MinBarWidth);
                bars.Add(new TimelineBarLayout(left, barWidth, CreateTooltip(entry.Name, label, span)));
                foreach (TimelineInterruption interruption in entry.Interruptions) {
                    DateTime gapStart = interruption.Start < span.Start ? span.Start : interruption.Start;
                    DateTime gapEnd = span.End.HasValue && interruption.End > span.End.Value ? span.End.Value : interruption.End;
                    if (gapEnd > gapStart) {
                        double gapLeft = ToX(gapStart, origin, pixelsPerYear);
                        double gapWidth = ToX(gapEnd, origin, pixelsPerYear) - gapLeft;
                        gaps.Add(new TimelineGapLayout(gapLeft, Math.Max(gapWidth, 2.0)));
                    }
                }
            }
            return new TimelineRowLayout(entry, bars, gaps);
        }

        private static string CreateTooltip(string name, string label, TimelineSpan span) {
            string start = span.Start.ToString("d", CultureInfo.CurrentCulture);
            string period = span.End.HasValue
                ? $"{start} – {span.End.Value.ToString("d", CultureInfo.CurrentCulture)}"
                : $"{start} – Ende unbekannt";
            return label.Length == 0 ? $"{name}\n{period}" : $"{name}\n{label}: {period}";
        }

        /// <summary>Jahresmarken; bei starkem Verkleinern werden nur noch alle 2 oder 5 Jahre beschriftet.</summary>
        private static List<TimelineTick> CreateTicks(DateTime origin, int lastYear, double pixelsPerYear) {
            int step = pixelsPerYear >= 70 ? 1 : pixelsPerYear >= 35 ? 2 : 5;
            List<TimelineTick> ticks = [];
            for (int year = origin.Year; year <= lastYear + 1; year++) {
                if ((year - origin.Year) % step == 0) {
                    double left = ToX(new DateTime(year, 1, 1), origin, pixelsPerYear);
                    ticks.Add(new TimelineTick(left, year.ToString(CultureInfo.InvariantCulture)));
                }
            }
            return ticks;
        }

        private static double ToX(DateTime date, DateTime origin, double pixelsPerYear) {
            return (date - origin).TotalDays / DaysPerYear * pixelsPerYear;
        }
    }
}
