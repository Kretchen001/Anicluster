using Anicluster.AnimeNetwork;

namespace Anicluster.ViewModels {

    /// <summary>
    /// Verbindung zwischen zwei Animes im Netzwerk. Wird von der Zeichenfläche bei jedem Neuzeichnen ausgelesen.
    /// Bei <see cref="AnimeEdgeKind.Successor"/> ist <see cref="A"/> der Vorgänger und <see cref="B"/> der Nachfolger.
    /// </summary>
    public class AnimeEdgeViewModel {

        public required AnimeNodeViewModel A { get; init; }

        public required AnimeNodeViewModel B { get; init; }

        /// <summary>Stärke von 0 bis 1 (Ähnlichkeit, bei Beziehungen die Gewichtung der Art)</summary>
        public double Weight { get; init; }

        public AnimeEdgeKind Kind { get; init; }

        public bool IsHighlighted { get; set; }

        /// <summary>Nur sichtbar, wenn beide Animes im Netzwerk sichtbar sind.</summary>
        public bool IsVisible => this.A.IsVisibleInGraph && this.B.IsVisibleInGraph;

        public bool IsDimmed => this.A.IsDimmed || this.B.IsDimmed;
    }
}
