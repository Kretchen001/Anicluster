using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Anicluster.Converters {

    public class EmptyToVisibilityConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            string? text = value as string;
            // Wenn leer oder null → Visible, sonst → Collapsed
            return string.IsNullOrEmpty(text) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}