using Modells.Anime;
using Serilog;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Anicluster.Imaging {

    /// <summary>
    /// Bereitet ein Bild für die Ablage vor: verkleinert es und erzeugt die zugeschnittene Vorschau für die Bilder-Ansicht.
    /// Das Ergebnis besteht aus JPEG-Daten. Transparente Flächen werden weiß hinterlegt, weil JPEG keine Transparenz kennt.
    /// Zustandslos, damit die Aufbereitung ohne Oberfläche testbar bleibt.
    /// </summary>
    public static class ImageProcessor {

        /// <summary>Längste Seite des gespeicherten Bildes in Pixeln (kleinere Bilder werden nicht vergrößert)</summary>
        public const int MaxImageSize = 1024;

        /// <summary>Größe der Vorschau, das Seitenverhältnis 2:3 entspricht einem typischen Cover</summary>
        public const int ThumbnailWidth = 240;
        public const int ThumbnailHeight = 360;

        private const int JpegQuality = 88;

        /// <summary>
        /// Erzeugt aus beliebigen Bilddaten (JPG, PNG, BMP, GIF, TIFF, ...) das Bild in beiden Größen.
        /// </summary>
        /// <returns>Das fertige Bild, <c>null</c> wenn die Daten kein lesbares Bild sind.</returns>
        public static AnimeImage? TryCreate(byte[] source) {
            ArgumentNullException.ThrowIfNull(source);
            try {
                BitmapSource original = Decode(source);
                return new AnimeImage() {
                    Data = EncodeScaled(original),
                    Thumbnail = EncodeThumbnail(original)
                };
            }
            catch (FileFormatException ex) {
                LogFailure(ex);
            }
            catch (NotSupportedException ex) {
                LogFailure(ex);
            }
            catch (InvalidOperationException ex) {
                LogFailure(ex);
            }
            catch (ArgumentException ex) {
                LogFailure(ex);
            }
            return null;
        }

        private static BitmapSource Decode(byte[] source) {
            using (MemoryStream stream = new MemoryStream(source)) {
                // OnLoad liest das Bild vollständig, danach darf der Stream geschlossen werden
                BitmapDecoder decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                return decoder.Frames[0];
            }
        }

        /// <summary>Das ganze Bild, so verkleinert, dass die längste Seite höchstens <see cref="MaxImageSize"/> misst.</summary>
        private static byte[] EncodeScaled(BitmapSource original) {
            double scale = Math.Min(1.0, (double)MaxImageSize / Math.Max(original.PixelWidth, original.PixelHeight));
            int width = Math.Max(1, (int)Math.Round(original.PixelWidth * scale));
            int height = Math.Max(1, (int)Math.Round(original.PixelHeight * scale));
            return RenderJpeg(original, width, height, new Rect(0, 0, width, height));
        }

        /// <summary>Die Vorschau füllt die feste Größe vollständig aus, überstehende Ränder werden mittig abgeschnitten.</summary>
        private static byte[] EncodeThumbnail(BitmapSource original) {
            double scale = Math.Max((double)ThumbnailWidth / original.PixelWidth, (double)ThumbnailHeight / original.PixelHeight);
            double drawnWidth = original.PixelWidth * scale;
            double drawnHeight = original.PixelHeight * scale;
            Rect target = new Rect((ThumbnailWidth - drawnWidth) / 2, (ThumbnailHeight - drawnHeight) / 2, drawnWidth, drawnHeight);
            return RenderJpeg(original, ThumbnailWidth, ThumbnailHeight, target);
        }

        /// <summary>Zeichnet das Bild in den Zielbereich einer weißen Fläche und kodiert das Ergebnis als JPEG.</summary>
        private static byte[] RenderJpeg(BitmapSource original, int width, int height, Rect target) {
            DrawingVisual visual = new DrawingVisual();
            RenderOptions.SetBitmapScalingMode(visual, BitmapScalingMode.HighQuality);
            using (DrawingContext context = visual.RenderOpen()) {
                context.DrawRectangle(Brushes.White, null, new Rect(0, 0, width, height));
                context.DrawImage(original, target);
            }
            RenderTargetBitmap rendered = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            rendered.Render(visual);

            JpegBitmapEncoder encoder = new JpegBitmapEncoder() { QualityLevel = JpegQuality };
            encoder.Frames.Add(BitmapFrame.Create(rendered));
            using (MemoryStream output = new MemoryStream()) {
                encoder.Save(output);
                return output.ToArray();
            }
        }

        private static void LogFailure(Exception ex) {
            Log.Warning(ex, "Die Bilddaten konnten nicht verarbeitet werden.");
        }
    }
}
