using Anicluster.TagNetwork;
using Modells.Anime;

namespace Anicluster.ViewModels {

    /// <summary>
    /// Ein Anime als Knoten im Anime-Netzwerk: Name, Tier, Status, Cluster, Position und Zustand (ausgewählt, hervorgehoben, abgeblendet).
    /// </summary>
    public class AnimeNodeViewModel : PropertyBaseChange {

        /// <summary>Schriftgröße der Namen im Netzwerk</summary>
        public const double NodeFontSize = 13.0;

        private const int MaxNameLength = 26;

        private bool _isSelected;
        private bool _isHighlighted;
        private bool _isDimmed;
        private bool _isVisibleInGraph;

        public int Id { get; }

        public string Name { get; }

        /// <summary>Der Name, bei Bedarf gekürzt (der volle Name steht im Tooltip)</summary>
        public string DisplayName { get; }

        public bool IsFavorite { get; }

        public Tier Tier { get; }

        public State State { get; }

        /// <summary>Kurztext des Tiers ("–", wenn keins gesetzt ist)</summary>
        public string TierText => this.Tier == Tier.NotDefined ? "–" : this.Tier.ToString();

        /// <summary>Nummer des Clusters (0 = größter), -1 ohne Verbindung. Wird bei jeder Neuberechnung gesetzt.</summary>
        public int ClusterId { get; set; } = -1;

        /// <summary>Hat der Anime in der aktuellen Netzwerkart mindestens eine Verbindung?</summary>
        public bool IsLinked { get; set; }

        /// <summary>Der zugehörige Knoten der Simulation</summary>
        public ForceNode Node { get; }

        /// <summary>Ist der Anime im Netzwerk sichtbar? (Daten vorhanden, verbunden oder "ohne Verbindung" eingeschaltet, Statusfilter)</summary>
        public bool IsVisibleInGraph {
            get => this._isVisibleInGraph;
            set => this.SetProperty(ref this._isVisibleInGraph, value);
        }

        /// <summary>Der gewählte Anime</summary>
        public bool IsSelected {
            get => this._isSelected;
            set => this.SetProperty(ref this._isSelected, value);
        }

        /// <summary>Unter der Maus, gewählt oder Nachbar davon, oder Treffer der Suche</summary>
        public bool IsHighlighted {
            get => this._isHighlighted;
            set => this.SetProperty(ref this._isHighlighted, value);
        }

        /// <summary>Gehört gerade nicht zum Fokus und wird abgeblendet.</summary>
        public bool IsDimmed {
            get => this._isDimmed;
            set => this.SetProperty(ref this._isDimmed, value);
        }

        public double Width => this.Node.Width;

        public double Height => this.Node.Height;

        public double CenterX => this.Node.X;

        public double CenterY => this.Node.Y;

        /// <summary>Linke obere Ecke für die Darstellung auf der Zeichenfläche</summary>
        public double Left => this.Node.X - this.Node.Width / 2.0;

        public double Top => this.Node.Y - this.Node.Height / 2.0;

        public AnimeNodeViewModel(AnimeListEntry entry) {
            this.Id = entry.Id;
            this.Name = entry.Name;
            this.DisplayName = entry.Name.Length > MaxNameLength ? entry.Name[..(MaxNameLength - 1)].TrimEnd() + "…" : entry.Name;
            this.IsFavorite = entry.IsFavorite;
            this.Tier = entry.Tier;
            this.State = entry.State;
            // Die Größe des Knotens wird aus der Textlänge abgeschätzt (Tier-Punkt, Rand und Stern eingerechnet),
            // sie bestimmt die Kollision in der Simulation
            this.Node = new ForceNode {
                Id = entry.Id,
                Width = this.DisplayName.Length * NodeFontSize * 0.58 + 37 + (entry.IsFavorite ? 19 : 0),
                Height = NodeFontSize * 1.35 + 12
            };
        }

        /// <summary>Meldet der Darstellung, dass sich die Position geändert hat.</summary>
        public void NotifyPositionChanged() {
            this.OnPropertyChanged(nameof(this.Left));
            this.OnPropertyChanged(nameof(this.Top));
        }
    }
}
