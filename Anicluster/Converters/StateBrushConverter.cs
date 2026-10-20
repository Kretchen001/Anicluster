using Modells.Anime;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Anicluster.Converters {

    /// <summary>
    /// Liefert die Farbe eines Status aus den Ressourcen (Schlüssel "State{State}Brush", siehe TierColors.xaml).
    /// </summary>
    public class StateBrushConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is State state && Application.Current.TryFindResource($"State{state}Brush") is Brush found) {
                return found;
            }
            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
