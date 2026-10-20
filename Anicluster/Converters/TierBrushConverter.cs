using Modells.Anime;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Anicluster.Converters {

    /// <summary>
    /// Liefert die Farbe eines Tiers aus den Ressourcen (Schlüssel "Tier{Tier}Brush", siehe TierColors.xaml).
    /// Mit dem ConverterParameter "Text" wird stattdessen eine darauf gut lesbare Schriftfarbe geliefert.
    /// </summary>
    public class TierBrushConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            Brush brush = Brushes.Gray;
            if (value is Tier tier && Application.Current.TryFindResource($"Tier{tier}Brush") is Brush found) {
                brush = found;
            }
            if (parameter is "Text" && brush is SolidColorBrush solid) {
                return new SolidColorBrush(Anicluster.Theming.ColorHelper.ReadableOn(solid.Color));
            }
            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
