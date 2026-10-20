using Anicluster.Theming;
using System.Windows.Media;

namespace Anicluster.ViewModels {

    /// <summary>
    /// Bearbeitung einer Farbe in den Einstellungen: Hex-Eingabe oder Schieberegler für Rot, Grün und Blau.
    /// </summary>
    public class ColorEditorViewModel : PropertyBaseChange {

        private Color _color;

        public string Title { get; }

        public string Description { get; }

        /// <summary>Wird ausgelöst, wenn sich die Farbe geändert hat.</summary>
        public event EventHandler? ColorChanged;

        public Color Color {
            get => this._color;
            set {
                if (this._color == value) {
                    return;
                }
                this._color = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.Red));
                this.OnPropertyChanged(nameof(this.Green));
                this.OnPropertyChanged(nameof(this.Blue));
                this.OnPropertyChanged(nameof(this.Hex));
                this.ColorChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public double Red {
            get => this._color.R;
            set => this.Color = Color.FromRgb(ToByte(value), this._color.G, this._color.B);
        }

        public double Green {
            get => this._color.G;
            set => this.Color = Color.FromRgb(this._color.R, ToByte(value), this._color.B);
        }

        public double Blue {
            get => this._color.B;
            set => this.Color = Color.FromRgb(this._color.R, this._color.G, ToByte(value));
        }

        /// <summary>Farbe als "#RRGGBB". Ungültige Eingaben werden ignoriert, bis sie vollständig sind.</summary>
        public string Hex {
            get => ColorHelper.ToHex(this._color);
            set {
                if (ColorHelper.TryParseHex(value, out Color parsed)) {
                    this.Color = parsed;
                }
            }
        }

        public ColorEditorViewModel(string title, string description, Color color) {
            this.Title = title;
            this.Description = description;
            this._color = color;
        }

        private static byte ToByte(double value) {
            return (byte)Math.Clamp(Math.Round(value), 0, 255);
        }
    }
}
