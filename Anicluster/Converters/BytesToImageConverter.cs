using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Anicluster.Converters {

    /// <summary>
    /// Macht aus gespeicherten Bilddaten (<c>byte[]</c>) ein anzeigbares Bild. Ohne oder mit unlesbaren Daten ergibt sich <c>null</c>,
    /// die View zeigt dann ihren Platzhalter.
    /// </summary>
    public class BytesToImageConverter : IValueConverter {

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is not byte[] { Length: > 0 } bytes) {
                return null;
            }
            try {
                BitmapImage image = new BitmapImage();
                using (MemoryStream stream = new MemoryStream(bytes)) {
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad; // lädt vollständig, der Stream wird danach nicht mehr gebraucht
                    image.StreamSource = stream;
                    image.EndInit();
                }
                image.Freeze();
                return image;
            }
            catch (NotSupportedException) {
                return null;
            }
            catch (FileFormatException) {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
