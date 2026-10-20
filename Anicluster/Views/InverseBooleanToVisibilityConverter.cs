using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Anicluster.Views {

    /// <summary>Gegenstück zum BooleanToVisibilityConverter: sichtbar, wenn der Wert <c>false</c> ist.</summary>
    public class InverseBooleanToVisibilityConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value is true ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
