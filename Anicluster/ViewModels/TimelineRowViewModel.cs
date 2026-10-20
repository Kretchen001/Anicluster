using Anicluster.Timeline;
using Modells.Anime;

namespace Anicluster.ViewModels {

    /// <summary>
    /// Eine Zeile des Zeitstrahls: ein Anime mit seinen Balken und Pausen.
    /// </summary>
    public class TimelineRowViewModel {

        public int Id { get; }

        public string Name { get; }

        public bool IsFavorite { get; }

        public Tier Tier { get; }

        public IReadOnlyList<TimelineBarViewModel> Bars { get; }

        public IReadOnlyList<TimelineGapLayout> Gaps { get; }

        /// <summary>Wird ausgelöst, wenn der Anime zum Anzeigen/Bearbeiten geöffnet werden soll.</summary>
        public event EventHandler? EditRequested;

        public TimelineRowViewModel(TimelineRowLayout layout, bool colorByTier) {
            this.Id = layout.Entry.Id;
            this.Name = layout.Entry.Name;
            this.IsFavorite = layout.Entry.IsFavorite;
            this.Tier = layout.Entry.Tier;
            this.Bars = layout.Bars.Select(x => new TimelineBarViewModel(x, layout.Entry, colorByTier)).ToList();
            this.Gaps = layout.Gaps;
        }

        public void RequestEdit() {
            this.EditRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
