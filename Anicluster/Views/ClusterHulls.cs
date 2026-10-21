using Anicluster.AnimeNetwork;
using System.Windows;
using System.Windows.Media;

namespace Anicluster.Views {

    /// <summary>
    /// Gemeinsame Hilfen für die weichen Cluster-Flächen im Anime- und im Tag-Netzwerk: die abgerundete Form um eine Punktmenge
    /// und die Farbe je Cluster (aus dem Akzent des Designs abgeleitet).
    /// </summary>
    internal static class ClusterHulls {

        /// <summary>Drehung des Farbtons von Cluster zu Cluster (Goldener Winkel, Nachbarn unterscheiden sich deutlich)</summary>
        private const double HueStep = 137.5;

        /// <summary>
        /// Abgerundete Fläche um die Punkte, <paramref name="padding"/> Pixel von ihrer äußeren Hülle entfernt.
        /// <c>null</c>, wenn die Punkte keine Fläche ergeben (zum Beispiel alle auf einer Stelle).
        /// </summary>
        public static StreamGeometry? CreateShape(IReadOnlyList<(double X, double Y)> points, double padding) {
            List<Point> outline = CreateOutline(ConvexHull.Compute(points), padding);
            return outline.Count < 3 ? null : CreateSmoothShape(outline);
        }

        /// <summary>
        /// Fläche und Rand eines Clusters. Der größte Cluster (Nummer 0) hat die Akzentfarbe des Designs, jeder weitere ihren um einen festen
        /// Winkel gedrehten Farbton (Sättigung und Helligkeit bleiben beim Akzent). Ist der Akzent kein einfarbiger Pinsel,
        /// gelten die übergebene Standardfläche und der Standardrand.
        /// </summary>
        public static (Brush Fill, Pen Outline) CreateStyle(Brush accent, int clusterId, Brush defaultFill, Pen defaultOutline) {
            if (accent is not SolidColorBrush solid) {
                return (defaultFill, defaultOutline);
            }
            Color color = ShiftHue(solid.Color, clusterId * HueStep);
            SolidColorBrush fill = new SolidColorBrush(Color.FromArgb(0x30, color.R, color.G, color.B));
            SolidColorBrush line = new SolidColorBrush(Color.FromArgb(0x99, color.R, color.G, color.B));
            Pen pen = new Pen(line, 1.2);
            fill.Freeze();
            pen.Freeze();
            return (fill, pen);
        }

        /// <summary>Schiebt die Eckpunkte der Hülle von ihrer Mitte weg, zwei Punkte werden zu einer Kapsel aufgeweitet.</summary>
        private static List<Point> CreateOutline(List<(double X, double Y)> hull, double padding) {
            List<Point> outline = [];
            if (hull.Count < 2) {
                return outline;
            }
            if (hull.Count == 2) {
                (double X, double Y) a = hull[0];
                (double X, double Y) b = hull[1];
                double length = Math.Sqrt((b.X - a.X) * (b.X - a.X) + (b.Y - a.Y) * (b.Y - a.Y));
                if (length < 1e-6) {
                    return outline;
                }
                double ux = (b.X - a.X) / length * padding;
                double uy = (b.Y - a.Y) / length * padding;
                outline.Add(new Point(a.X - ux - uy, a.Y - uy + ux));
                outline.Add(new Point(b.X + ux - uy, b.Y + uy + ux));
                outline.Add(new Point(b.X + ux + uy, b.Y + uy - ux));
                outline.Add(new Point(a.X - ux + uy, a.Y - uy - ux));
                return outline;
            }
            double centerX = hull.Average(p => p.X);
            double centerY = hull.Average(p => p.Y);
            foreach ((double X, double Y) point in hull) {
                double dx = point.X - centerX;
                double dy = point.Y - centerY;
                double distance = Math.Sqrt(dx * dx + dy * dy);
                if (distance < 1e-6) {
                    dx = 1.0;
                    dy = 0.0;
                    distance = 1.0;
                }
                outline.Add(new Point(point.X + dx / distance * padding, point.Y + dy / distance * padding));
            }
            return outline;
        }

        /// <summary>Rundet das Vieleck ab: Die Eckpunkte werden zu Kontrollpunkten, die Kurve läuft durch die Mitten der Seiten.</summary>
        private static StreamGeometry CreateSmoothShape(List<Point> outline) {
            StreamGeometry geometry = new StreamGeometry();
            using (StreamGeometryContext context = geometry.Open()) {
                int count = outline.Count;
                context.BeginFigure(Middle(outline[count - 1], outline[0]), true, true);
                for (int index = 0; index < count; index += 1) {
                    Point corner = outline[index];
                    Point next = outline[(index + 1) % count];
                    context.QuadraticBezierTo(corner, Middle(corner, next), true, false);
                }
            }
            geometry.Freeze();
            return geometry;
        }

        private static Point Middle(Point first, Point second) {
            return new Point((first.X + second.X) / 2.0, (first.Y + second.Y) / 2.0);
        }

        /// <summary>Dreht den Farbton um die angegebenen Grad. Sättigung mindestens 0,5 und Helligkeit zwischen 0,55 und 0,95, damit die Farbe sichtbar bleibt.</summary>
        private static Color ShiftHue(Color color, double degrees) {
            double red = color.R / 255.0;
            double green = color.G / 255.0;
            double blue = color.B / 255.0;
            double max = Math.Max(red, Math.Max(green, blue));
            double min = Math.Min(red, Math.Min(green, blue));
            double delta = max - min;
            double hue = 0.0;
            if (delta > 1e-9) {
                if (max == red) {
                    hue = 60.0 * (((green - blue) / delta) % 6.0);
                }
                else if (max == green) {
                    hue = 60.0 * ((blue - red) / delta + 2.0);
                }
                else {
                    hue = 60.0 * ((red - green) / delta + 4.0);
                }
            }
            double saturation = Math.Max(max <= 0.0 ? 0.0 : delta / max, 0.5);
            double value = Math.Clamp(max, 0.55, 0.95);
            hue = ((hue + degrees) % 360.0 + 360.0) % 360.0;

            double chroma = value * saturation;
            double secondary = chroma * (1.0 - Math.Abs(hue / 60.0 % 2.0 - 1.0));
            double offset = value - chroma;
            (double R, double G, double B) rgb = (int)(hue / 60.0) switch {
                0 => (chroma, secondary, 0.0),
                1 => (secondary, chroma, 0.0),
                2 => (0.0, chroma, secondary),
                3 => (0.0, secondary, chroma),
                4 => (secondary, 0.0, chroma),
                _ => (chroma, 0.0, secondary)
            };
            return Color.FromRgb(
                (byte)Math.Round((rgb.R + offset) * 255.0),
                (byte)Math.Round((rgb.G + offset) * 255.0),
                (byte)Math.Round((rgb.B + offset) * 255.0)
            );
        }
    }
}
