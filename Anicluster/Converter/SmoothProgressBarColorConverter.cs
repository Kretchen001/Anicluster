using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Anicluster.Converters {
    public class SmoothProgressBarColorConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null) {
                return Brushes.Red;
            }

            double val = 0;
            try {
                val = System.Convert.ToDouble(value);
            }
            catch {
                return Brushes.Red;
            }

            // Clamp zwischen 0 und 100
            val = Math.Max(0, Math.Min(100, val));

            // Start- und Endfarbe: Rot → Orange → Gelb → YellowGreen → Grün → Lime
            // Wir interpolieren linear zwischen den Farben
            Color c;

            if (val <= 20) {
                c = InterpolateColor(Colors.Red, Colors.Orange, val / 20.0);
            }
            else if (val <= 40) {
                c = InterpolateColor(Colors.Orange, Colors.Yellow, (val - 20) / 20.0);
            }
            else if (val <= 60) {
                c = InterpolateColor(Colors.Yellow, Colors.YellowGreen, (val - 40) / 20.0);
            }
            else if (val <= 80) {
                c = InterpolateColor(Colors.YellowGreen, Colors.LimeGreen, (val - 60) / 20.0);
            }
            else {
                c = InterpolateColor(Colors.LimeGreen, Colors.Lime, (val - 80) / 20.0);
            }

            return new SolidColorBrush(c);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        private Color InterpolateColor(Color from, Color to, double fraction) {
            byte a = (byte)(from.A + (to.A - from.A) * fraction);
            byte r = (byte)(from.R + (to.R - from.R) * fraction);
            byte g = (byte)(from.G + (to.G - from.G) * fraction);
            byte b = (byte)(from.B + (to.B - from.B) * fraction);
            return Color.FromArgb(a, r, g, b);
        }
    }
}