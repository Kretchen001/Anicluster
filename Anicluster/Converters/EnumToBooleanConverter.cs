using System.Globalization;
using System.Windows.Data;

namespace Anicluster.Converters {

    /// <summary>
    /// Verbindet einen Enum-Wert mit einem RadioButton: Der RadioButton ist markiert,
    /// wenn der Wert dem ConverterParameter entspricht. Beim Markieren wird der Parameter als neuer Wert gesetzt.
    /// </summary>
    public class EnumToBooleanConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return value is true ? parameter : Binding.DoNothing;
        }
    }
}
