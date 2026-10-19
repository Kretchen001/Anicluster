using Anicluster.Theming;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Anicluster.Converters {

    /// <summary>Wandelt eine Farbe in einen Pinsel um, z. B. für Farbfelder in den Einstellungen.</summary>
    public class ColorToBrushConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value is Color color ? ColorHelper.CreateBrush(color) : Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
