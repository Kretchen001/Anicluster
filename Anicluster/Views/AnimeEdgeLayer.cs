using Anicluster.AnimeNetwork;
using Anicluster.ViewModels;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace Anicluster.Views {

    /// <summary>
    /// Zeichnet den Hintergrund des Anime-Netzwerks: weiche Flächen um die Cluster samt Beschriftung, darüber die Verbindungslinien
    /// (Pfeile bei Vorgänger → Nachfolger, gestrichelt bei Verwandt, Stärke nach Ähnlichkeit). Wird bei jedem Schritt der Simulation
    /// über <see cref="UIElement.InvalidateVisual"/> neu gezeichnet. Die Farben kommen aus den Theme-Ressourcen
    /// (EdgeBrush, AccentBrush, AccentSoftBrush, LineBrush, TextMutedBrush).
    /// </summary>
    public class AnimeEdgeLayer : FrameworkElement {

        /// <summary>Abstand der Cluster-Fläche zu den Mittelpunkten der Knoten</summary>
        private const double HullPadding = 40.0;
        private const double ArrowLength = 11.0;
        private const double ArrowHalfWidth = 5.0;
        private const double CaptionFontSize = 12.0;

        public static readonly DependencyProperty EdgesProperty = DependencyProperty.Register(
            nameof(Edges),
            typeof(IReadOnlyList<AnimeEdgeViewModel>),
            typeof(AnimeEdgeLayer),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender)
        );

        public static readonly DependencyProperty ClustersProperty = DependencyProperty.Register(
            nameof(Clusters),
            typeof(IReadOnlyList<AnimeClusterViewModel>),
            typeof(AnimeEdgeLayer),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnClustersChanged)
        );

        private readonly Dictionary<(AnimeEdgeKind Kind, int Bucket, bool Highlighted), Pen> _pens = [];
        private readonly Dictionary<int, FormattedText> _captions = [];
        private Brush _edgeBrush = Brushes.Gray;
        private Brush _accentBrush = Brushes.Blue;
        private Brush _hullBrush = Brushes.LightGray;
        private Brush _textBrush = Brushes.Gray;
        private Pen? _hullPen;

        public IReadOnlyList<AnimeEdgeViewModel>? Edges {
            get => (IReadOnlyList<AnimeEdgeViewModel>?)this.GetValue(EdgesProperty);
            set => this.SetValue(EdgesProperty, value);
        }

        public IReadOnlyList<AnimeClusterViewModel>? Clusters {
            get => (IReadOnlyList<AnimeClusterViewModel>?)this.GetValue(ClustersProperty);
            set => this.SetValue(ClustersProperty, value);
        }

        /// <summary>Neue Cluster haben neue Beschriftungen, die zwischengespeicherten Texte werden verworfen.</summary>
        private static void OnClustersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is AnimeEdgeLayer layer) {
                layer._captions.Clear();
            }
        }

        protected override void OnRender(DrawingContext drawingContext) {
            base.OnRender(drawingContext);
            this.RefreshBrushes();
            IReadOnlyList<AnimeClusterViewModel>? clusters = this.Clusters;
            if (clusters is not null) {
                this.DrawClusters(drawingContext, clusters);
            }
            IReadOnlyList<AnimeEdgeViewModel>? edges = this.Edges;
            if (edges is null) {
                return;
            }
            foreach (AnimeEdgeViewModel edge in edges) {
                if (!edge.IsVisible) {
                    continue;
                }
                bool isDimmed = edge.IsDimmed && !edge.IsHighlighted;
                if (isDimmed) {
                    drawingContext.PushOpacity(0.15);
                }
                Pen pen = this.GetPen(edge);
                drawingContext.DrawLine(pen, new Point(edge.A.CenterX, edge.A.CenterY), new Point(edge.B.CenterX, edge.B.CenterY));
                if (edge.Kind == AnimeEdgeKind.Successor) {
                    DrawArrow(drawingContext, edge, pen.Brush);
                }
                if (isDimmed) {
                    drawingContext.Pop();
                }
            }
        }

        /// <summary>Holt die aktuellen Theme-Pinsel. Hat sich das Design geändert, werden Stifte und Texte neu erzeugt.</summary>
        private void RefreshBrushes() {
            Brush edgeBrush = this.TryFindResource("EdgeBrush") as Brush ?? Brushes.Gray;
            Brush accentBrush = this.TryFindResource("AccentBrush") as Brush ?? Brushes.Blue;
            Brush hullBrush = this.TryFindResource("AccentSoftBrush") as Brush ?? Brushes.LightGray;
            Brush lineBrush = this.TryFindResource("LineBrush") as Brush ?? Brushes.Gray;
            Brush textBrush = this.TryFindResource("TextMutedBrush") as Brush ?? Brushes.Gray;
            if (ReferenceEquals(edgeBrush, this._edgeBrush) && ReferenceEquals(accentBrush, this._accentBrush)
                && ReferenceEquals(hullBrush, this._hullBrush) && ReferenceEquals(textBrush, this._textBrush)
                && this._hullPen is not null) {
                return;
            }
            this._pens.Clear();
            this._captions.Clear();
            this._edgeBrush = edgeBrush;
            this._accentBrush = accentBrush;
            this._hullBrush = hullBrush;
            this._textBrush = textBrush;
            this._hullPen = new Pen(lineBrush, 1.0);
            if (this._hullPen.CanFreeze) {
                this._hullPen.Freeze();
            }
        }

        #region Cluster
        private void DrawClusters(DrawingContext drawingContext, IReadOnlyList<AnimeClusterViewModel> clusters) {
            foreach (AnimeClusterViewModel cluster in clusters) {
                List<(double X, double Y)> points = [];
                foreach (AnimeNodeViewModel member in cluster.Members) {
                    if (member.IsVisibleInGraph) {
                        points.Add((member.CenterX, member.CenterY));
                    }
                }
                if (points.Count < 2) {
                    continue;
                }
                List<Point> outline = CreateOutline(ConvexHull.Compute(points));
                if (outline.Count < 3) {
                    continue;
                }
                drawingContext.DrawGeometry(this._hullBrush, this._hullPen, CreateSmoothShape(outline));
                if (cluster.Caption.Length > 0) {
                    this.DrawCaption(drawingContext, cluster, points);
                }
            }
        }

        /// <summary>Schiebt die Eckpunkte der Hülle von ihrer Mitte weg, zwei Punkte werden zu einer Kapsel aufgeweitet.</summary>
        private static List<Point> CreateOutline(List<(double X, double Y)> hull) {
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
                double ux = (b.X - a.X) / length * HullPadding;
                double uy = (b.Y - a.Y) / length * HullPadding;
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
                outline.Add(new Point(point.X + dx / distance * HullPadding, point.Y + dy / distance * HullPadding));
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

        private void DrawCaption(DrawingContext drawingContext, AnimeClusterViewModel cluster, List<(double X, double Y)> points) {
            if (!this._captions.TryGetValue(cluster.Id, out FormattedText? text)) {
                text = new FormattedText(
                    cluster.Caption,
                    CultureInfo.CurrentUICulture,
                    FlowDirection.LeftToRight,
                    new Typeface(SystemFonts.MessageFontFamily, FontStyles.Normal, FontWeights.SemiBold, FontStretches.Normal),
                    CaptionFontSize,
                    this._textBrush,
                    VisualTreeHelper.GetDpi(this).PixelsPerDip
                );
                this._captions[cluster.Id] = text;
            }
            double centerX = points.Average(p => p.X);
            double top = Math.Max(2.0, points.Min(p => p.Y) - HullPadding - text.Height - 2.0);
            drawingContext.DrawText(text, new Point(centerX - text.Width / 2.0, top));
        }
        #endregion

        #region Verbindungen
        /// <summary>Die Linienstärke wächst bei Ähnlichkeiten mit der Stärke (in fünf Stufen, die Stifte werden wiederverwendet).</summary>
        private Pen GetPen(AnimeEdgeViewModel edge) {
            int bucket = edge.Kind == AnimeEdgeKind.Similarity ? (int)Math.Round(edge.Weight * 4) : 0;
            (AnimeEdgeKind, int, bool) key = (edge.Kind, bucket, edge.IsHighlighted);
            if (!this._pens.TryGetValue(key, out Pen? pen)) {
                double thickness = edge.Kind switch {
                    AnimeEdgeKind.Successor => 2.4,
                    AnimeEdgeKind.Related => 1.6,
                    _ => 1.0 + bucket * 0.8
                };
                pen = new Pen(edge.IsHighlighted ? this._accentBrush : this._edgeBrush, thickness) {
                    StartLineCap = PenLineCap.Round,
                    EndLineCap = PenLineCap.Round
                };
                if (edge.Kind == AnimeEdgeKind.Related) {
                    pen.DashStyle = DashStyles.Dash;
                }
                if (pen.CanFreeze) {
                    pen.Freeze();
                }
                this._pens[key] = pen;
            }
            return pen;
        }

        /// <summary>Pfeilspitze am Rand des Nachfolgers, in Richtung des Nachfolgers.</summary>
        private static void DrawArrow(DrawingContext drawingContext, AnimeEdgeViewModel edge, Brush brush) {
            double dx = edge.A.CenterX - edge.B.CenterX;
            double dy = edge.A.CenterY - edge.B.CenterY;
            double length = Math.Sqrt(dx * dx + dy * dy);
            if (length < 1.0) {
                return;
            }
            // Einheitsvektor vom Nachfolger zum Vorgänger
            double ux = dx / length;
            double uy = dy / length;
            double halfWidth = edge.B.Width / 2.0 + 3.0;
            double halfHeight = edge.B.Height / 2.0 + 3.0;
            double scaleX = Math.Abs(ux) < 1e-6 ? double.MaxValue : halfWidth / Math.Abs(ux);
            double scaleY = Math.Abs(uy) < 1e-6 ? double.MaxValue : halfHeight / Math.Abs(uy);
            double distance = Math.Min(scaleX, scaleY);
            if (distance + ArrowLength >= length) {
                return; // Die Knoten liegen zu dicht beieinander für einen Pfeil
            }
            Point tip = new Point(edge.B.CenterX + ux * distance, edge.B.CenterY + uy * distance);
            Point baseCenter = new Point(tip.X + ux * ArrowLength, tip.Y + uy * ArrowLength);
            Point left = new Point(baseCenter.X - uy * ArrowHalfWidth, baseCenter.Y + ux * ArrowHalfWidth);
            Point right = new Point(baseCenter.X + uy * ArrowHalfWidth, baseCenter.Y - ux * ArrowHalfWidth);
            StreamGeometry geometry = new StreamGeometry();
            using (StreamGeometryContext context = geometry.Open()) {
                context.BeginFigure(tip, true, true);
                context.LineTo(left, true, false);
                context.LineTo(right, true, false);
            }
            geometry.Freeze();
            drawingContext.DrawGeometry(brush, null, geometry);
        }
        #endregion
    }
}
