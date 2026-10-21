using Anicluster.Timeline;
using Modells.Anime;

namespace Anicluster.ViewModels {

    /// <summary>
    /// Ein Balken auf dem Zeitstrahl. Die Farbe richtet sich wahlweise nach dem Tier oder dem Status des Animes.
    /// </summary>
    public class TimelineBarViewModel {

        public double Left { get; }

        public double Width { get; }

        public string Tooltip { get; }

        public Tier Tier { get; }

        public State State { get; }

        /// <summary><c>true</c> = Farbe nach Tier, <c>false</c> = Farbe nach Status</summary>
        public bool ColorByTier { get; }

        public TimelineBarViewModel(TimelineBarLayout layout, TimelineEntry entry, bool colorByTier) {
            this.Left = layout.Left;
            this.Width = layout.Width;
            this.Tooltip = layout.Tooltip;
            this.Tier = entry.Tier;
            this.State = entry.State;
            this.ColorByTier = colorByTier;
        }
    }
}
