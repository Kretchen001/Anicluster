using Anicluster.TasteProfile;

namespace Anicluster.ViewModels {

    /// <summary>
    /// Ein Anime der Wunschliste mit seiner Passung zum Geschmacksprofil.
    /// </summary>
    public class TasteSuggestionViewModel {

        /// <summary>Der Anime als Listeneintrag (liefert Name, Tier und Vorschaubild)</summary>
        public AnimeListItemViewModel Item { get; }

        public int Percent { get; }

        public string PercentText => $"{this.Percent} %";

        /// <summary>Warum der Anime passt, z. B. „Passt wegen: Action, Fantasy“</summary>
        public string MatchText { get; }

        public TasteSuggestionViewModel(TasteSuggestion suggestion, AnimeListItemViewModel item) {
            this.Item = item;
            this.Percent = suggestion.Percent;
            this.MatchText = suggestion.MatchingTags.Count == 0
                ? string.Empty
                : "Passt wegen: " + string.Join(", ", suggestion.MatchingTags.Select(x => x.Name));
        }

        /// <summary>Öffnet den Anime zum Anzeigen oder Bearbeiten.</summary>
        public void RequestEdit() {
            this.Item.RequestEdit();
        }
    }
}
