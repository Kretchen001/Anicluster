using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Modells.Anime;

namespace Anicluster.Converters {

    /// <summary>
    /// Vergleicht das aktuelle Tier mit dem übergebenen Tier.
    /// Wird verwendet, um den aktuell ausgewählten Tier-Button hervorzuheben.
    /// </summary>
    public class TierSelectedConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is Tier currentTier &&
                parameter is Tier buttonTier) {
                return currentTier == buttonTier
                    ? Brushes.Black
                    : Brushes.Transparent;
            }

            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
