namespace Anicluster.ViewModels {

    /// <summary>
    /// Verbindung zwischen zwei Tags im Netzwerk. Wird von der Zeichenfläche bei jedem Neuzeichnen ausgelesen.
    /// </summary>
    public class TagEdgeViewModel {

        public required TagNodeViewModel A { get; init; }

        public required TagNodeViewModel B { get; init; }

        /// <summary>Anzahl gemeinsamer Animes</summary>
        public int Count { get; init; }

        /// <summary>Gewicht von 0 bis 1 im Verhältnis zur stärksten Verbindung (bestimmt die Linienstärke)</summary>
        public double Weight { get; init; }

        public bool IsHighlighted { get; set; }

        /// <summary>Nur sichtbar, wenn beide Tags im Netzwerk sichtbar sind.</summary>
        public bool IsVisible => this.A.IsVisibleInGraph && this.B.IsVisibleInGraph;

        public bool IsDimmed => this.A.IsDimmed || this.B.IsDimmed;
    }
}
