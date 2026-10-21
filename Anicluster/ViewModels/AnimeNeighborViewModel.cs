using Modells.Anime;

namespace Anicluster.ViewModels {

    /// <summary>
    /// Ein mit dem gewählten Anime verbundener Anime in der Liste der Seitenleiste.
    /// <see cref="DetailText"/> nennt die Art der Verbindung ("Nachfolger") oder die Ähnlichkeit ("87 %").
    /// </summary>
    public class AnimeNeighborViewModel {

        public AnimeNodeViewModel Node { get; }

        public string DetailText { get; }

        /// <summary>Stärke der Verbindung von 0 bis 1 (zum Sortieren)</summary>
        public double Weight { get; }

        public string Name => this.Node.Name;

        public Tier Tier => this.Node.Tier;

        public string TierText => this.Node.TierText;

        public AnimeNeighborViewModel(AnimeNodeViewModel node, string detailText, double weight) {
            this.Node = node;
            this.DetailText = detailText;
            this.Weight = weight;
        }
    }
}
