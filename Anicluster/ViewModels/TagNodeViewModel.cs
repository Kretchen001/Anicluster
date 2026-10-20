using Anicluster.TagNetwork;

namespace Anicluster.ViewModels {

    /// <summary>
    /// Ein Tag als Knoten im Netzwerk: Häufigkeit, Schriftgröße, Position und Zustand (an/aus, ausgewählt, hervorgehoben).
    /// </summary>
    public class TagNodeViewModel : PropertyBaseChange {

        private bool _isEnabled = true;
        private bool _isSelected;
        private bool _isHighlighted;
        private bool _isDimmed;
        private bool _isVisibleInGraph = true;
        private bool _isBelowMinimum;

        public int Id { get; }

        public string Designation { get; }

        /// <summary>Anzahl der Animes mit diesem Tag</summary>
        public int Count { get; }

        /// <summary>Schriftgröße, wächst mit der Häufigkeit</summary>
        public double FontSize { get; }

        /// <summary>Der zugehörige Knoten der Simulation</summary>
        public ForceNode Node { get; }

        /// <summary>Wird ausgelöst, wenn der Tag über <see cref="IsEnabled"/> ein- oder ausgeschaltet wurde.</summary>
        public event EventHandler? EnabledChanged;

        /// <summary>Vom Benutzer ein- oder ausgeschaltet (Checkbox oder Kontextmenü)</summary>
        public bool IsEnabled {
            get => this._isEnabled;
            set {
                if (this.SetProperty(ref this._isEnabled, value)) {
                    this.EnabledChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>Ist der Tag im Netzwerk sichtbar? (eingeschaltet und mindestens so häufig wie gefordert)</summary>
        public bool IsVisibleInGraph {
            get => this._isVisibleInGraph;
            set => this.SetProperty(ref this._isVisibleInGraph, value);
        }

        /// <summary>Ist der Tag seltener als die geforderte Mindesthäufigkeit? (Anzeige in der Liste)</summary>
        public bool IsBelowMinimum {
            get => this._isBelowMinimum;
            set => this.SetProperty(ref this._isBelowMinimum, value);
        }

        /// <summary>Wirkt als Filter für die Animes.</summary>
        public bool IsSelected {
            get => this._isSelected;
            set => this.SetProperty(ref this._isSelected, value);
        }

        /// <summary>Unter der Maus oder Nachbar des Tags unter der Maus</summary>
        public bool IsHighlighted {
            get => this._isHighlighted;
            set => this.SetProperty(ref this._isHighlighted, value);
        }

        /// <summary>Gehört gerade nicht zum Fokus (Hover oder Auswahl) und wird abgeblendet.</summary>
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

        public TagNodeViewModel(int id, string designation, int count, double fontSize) {
            this.Id = id;
            this.Designation = designation;
            this.Count = count;
            this.FontSize = fontSize;
            // Die Größe des Textfelds wird aus der Schriftgröße abgeschätzt, sie bestimmt die Kollision in der Simulation
            this.Node = new ForceNode {
                Id = id,
                Width = designation.Length * fontSize * 0.58 + 28,
                Height = fontSize * 1.35 + 12
            };
        }

        /// <summary>Meldet der Darstellung, dass sich die Position geändert hat.</summary>
        public void NotifyPositionChanged() {
            this.OnPropertyChanged(nameof(this.Left));
            this.OnPropertyChanged(nameof(this.Top));
            this.OnPropertyChanged(nameof(this.CenterX));
            this.OnPropertyChanged(nameof(this.CenterY));
        }
    }
}
