using System.Globalization;
using System.Windows.Media;

namespace Anicluster.Theming {

    /// <summary>
    /// Hilfsmethoden für Farben: Hex-Umwandlung, Mischen und Helligkeit.
    /// </summary>
    public static class ColorHelper {

        /// <summary>Wandelt "#RRGGBB" (das "#" ist optional) in eine Farbe um.</summary>
        public static bool TryParseHex(string? hex, out Color color) {
            color = default;
            if (string.IsNullOrWhiteSpace(hex)) {
                return false;
            }
            string text = hex.Trim().TrimStart('#');
            if (text.Length != 6 || !int.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int value)) {
                return false;
            }
            color = Color.FromRgb((byte)(value >> 16), (byte)(value >> 8), (byte)value);
            return true;
        }

        /// <summary>Wie <see cref="TryParseHex"/>, bei ungültiger Eingabe wird <paramref name="fallback"/> geliefert.</summary>
        public static Color FromHex(string? hex, Color fallback) {
            return TryParseHex(hex, out Color color) ? color : fallback;
        }

        public static string ToHex(Color color) {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        /// <summary>Mischt zwei Farben. <paramref name="amount"/> 0 = nur <paramref name="from"/>, 1 = nur <paramref name="to"/>.</summary>
        public static Color Blend(Color from, Color to, double amount) {
            amount = Math.Clamp(amount, 0, 1);
            return Color.FromRgb(
                (byte)Math.Round(from.R + (to.R - from.R) * amount),
                (byte)Math.Round(from.G + (to.G - from.G) * amount),
                (byte)Math.Round(from.B + (to.B - from.B) * amount)
            );
        }

        /// <summary>Wahrgenommene Helligkeit von 0 (schwarz) bis 1 (weiß).</summary>
        public static double Luminance(Color color) {
            return (0.2126 * color.R + 0.7152 * color.G + 0.0722 * color.B) / 255.0;
        }

        /// <summary>Dunkle oder weiße Schriftfarbe, je nachdem, was auf dem Hintergrund besser lesbar ist.</summary>
        public static Color ReadableOn(Color background) {
            return Luminance(background) > 0.6 ? Color.FromRgb(0x11, 0x18, 0x27) : Colors.White;
        }

        public static Color WithAlpha(Color color, byte alpha) {
            return Color.FromArgb(alpha, color.R, color.G, color.B);
        }

        /// <summary>Erzeugt einen eingefrorenen (unveränderlichen) Pinsel.</summary>
        public static SolidColorBrush CreateBrush(Color color) {
            SolidColorBrush brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }
    }
}
