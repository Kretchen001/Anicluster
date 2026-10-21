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
        private const double CaptionFontSize = 13.0;

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
        private readonly Dictionary<int, (Brush Fill, Pen Outline)> _hullStyles = [];
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
            this._hullStyles.Clear();
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
                StreamGeometry? shape = ClusterHulls.CreateShape(points, HullPadding);
                if (shape is null) {
                    continue;
                }
                (Brush fill, Pen outlinePen) = this.GetHullStyle(cluster.Id);
                drawingContext.DrawGeometry(fill, outlinePen, shape);
                if (cluster.Caption.Length > 0) {
                    this.DrawCaption(drawingContext, cluster, points);
                }
            }
        }

        /// <summary>Fläche und Rand eines Clusters, je Cluster einmal erzeugt (die Farben stammen aus dem Akzent des Designs).</summary>
        private (Brush Fill, Pen Outline) GetHullStyle(int clusterId) {
            if (!this._hullStyles.TryGetValue(clusterId, out (Brush Fill, Pen Outline) style)) {
                style = ClusterHulls.CreateStyle(this._accentBrush, clusterId, this._hullBrush, this._hullPen ?? new Pen(this._edgeBrush, 1.0));
                this._hullStyles[clusterId] = style;
            }
            return style;
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
