using Anicluster.ViewModels;
using System.Windows;
using System.Windows.Media;

namespace Anicluster.Views {

    /// <summary>
    /// Zeichnet den Hintergrund des Tag-Netzwerks: weiche Flächen um zusammengehörende Tag-Gruppen (je Gruppe eine eigene Farbe),
    /// darüber die Verbindungslinien zwischen den Tags. Wird bei jedem Schritt der Simulation
    /// über <see cref="UIElement.InvalidateVisual"/> neu gezeichnet. Das ist deutlich schneller als eine Linie pro Verbindung als eigenes Element.
    /// Die Farben kommen aus den Theme-Ressourcen (EdgeBrush, AccentBrush, AccentSoftBrush, LineBrush).
    /// </summary>
    public class TagEdgeLayer : FrameworkElement {

        /// <summary>Abstand der Gruppen-Fläche zu den Rändern der Textfelder</summary>
        private const double HullPadding = 18.0;

        public static readonly DependencyProperty EdgesProperty = DependencyProperty.Register(
            nameof(Edges),
            typeof(IReadOnlyList<TagEdgeViewModel>),
            typeof(TagEdgeLayer),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender)
        );

        public static readonly DependencyProperty ClustersProperty = DependencyProperty.Register(
            nameof(Clusters),
            typeof(IReadOnlyList<TagClusterViewModel>),
            typeof(TagEdgeLayer),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender)
        );

        private readonly Dictionary<(int Bucket, bool Highlighted), Pen> _pens = [];
        private readonly Dictionary<int, (Brush Fill, Pen Outline)> _hullStyles = [];
        private Brush? _edgeBrush;
        private Brush? _accentBrush;
        private Brush? _hullBrush;
        private Pen? _hullPen;

        public IReadOnlyList<TagEdgeViewModel>? Edges {
            get => (IReadOnlyList<TagEdgeViewModel>?)this.GetValue(EdgesProperty);
            set => this.SetValue(EdgesProperty, value);
        }

        public IReadOnlyList<TagClusterViewModel>? Clusters {
            get => (IReadOnlyList<TagClusterViewModel>?)this.GetValue(ClustersProperty);
            set => this.SetValue(ClustersProperty, value);
        }

        protected override void OnRender(DrawingContext drawingContext) {
            base.OnRender(drawingContext);
            this.RefreshBrushes();
            IReadOnlyList<TagClusterViewModel>? clusters = this.Clusters;
            if (clusters is not null) {
                this.DrawClusters(drawingContext, clusters);
            }
            IReadOnlyList<TagEdgeViewModel>? edges = this.Edges;
            if (edges is null) {
                return;
            }
            foreach (TagEdgeViewModel edge in edges) {
                if (!edge.IsVisible) {
                    continue;
                }
                bool isDimmed = edge.IsDimmed && !edge.IsHighlighted;
                if (isDimmed) {
                    drawingContext.PushOpacity(0.15);
                }
                drawingContext.DrawLine(
                    this.GetPen(edge),
                    new Point(edge.A.CenterX, edge.A.CenterY),
                    new Point(edge.B.CenterX, edge.B.CenterY)
                );
                if (isDimmed) {
                    drawingContext.Pop();
                }
            }
        }

        /// <summary>Holt die aktuellen Theme-Pinsel. Hat sich das Design geändert, werden Stifte und Flächen neu erzeugt.</summary>
        private void RefreshBrushes() {
            Brush edgeBrush = this.TryFindResource("EdgeBrush") as Brush ?? Brushes.Gray;
            Brush accentBrush = this.TryFindResource("AccentBrush") as Brush ?? Brushes.Blue;
            Brush hullBrush = this.TryFindResource("AccentSoftBrush") as Brush ?? Brushes.LightGray;
            Brush lineBrush = this.TryFindResource("LineBrush") as Brush ?? Brushes.Gray;
            if (ReferenceEquals(edgeBrush, this._edgeBrush) && ReferenceEquals(accentBrush, this._accentBrush)
                && ReferenceEquals(hullBrush, this._hullBrush) && this._hullPen is not null) {
                return;
            }
            this._pens.Clear();
            this._hullStyles.Clear();
            this._edgeBrush = edgeBrush;
            this._accentBrush = accentBrush;
            this._hullBrush = hullBrush;
            this._hullPen = new Pen(lineBrush, 1.0);
            if (this._hullPen.CanFreeze) {
                this._hullPen.Freeze();
            }
        }

        #region Gruppen
        private void DrawClusters(DrawingContext drawingContext, IReadOnlyList<TagClusterViewModel> clusters) {
            foreach (TagClusterViewModel cluster in clusters) {
                // Die Fläche umschließt die ganzen Textfelder, nicht nur ihre Mittelpunkte
                List<(double X, double Y)> points = [];
                int visibleCount = 0;
                foreach (TagNodeViewModel member in cluster.Members) {
                    if (!member.IsVisibleInGraph) {
                        continue;
                    }
                    visibleCount += 1;
                    double halfWidth = member.Width / 2.0;
                    double halfHeight = member.Height / 2.0;
                    points.Add((member.CenterX - halfWidth, member.CenterY - halfHeight));
                    points.Add((member.CenterX + halfWidth, member.CenterY - halfHeight));
                    points.Add((member.CenterX + halfWidth, member.CenterY + halfHeight));
                    points.Add((member.CenterX - halfWidth, member.CenterY + halfHeight));
                }
                if (visibleCount < 2) {
                    continue;
                }
                StreamGeometry? shape = ClusterHulls.CreateShape(points, HullPadding);
                if (shape is null) {
                    continue;
                }
                (Brush fill, Pen outline) = this.GetHullStyle(cluster.Id);
                drawingContext.DrawGeometry(fill, outline, shape);
            }
        }

        /// <summary>Fläche und Rand einer Gruppe, je Gruppe einmal erzeugt (die Farben stammen aus dem Akzent des Designs).</summary>
        private (Brush Fill, Pen Outline) GetHullStyle(int clusterId) {
            if (!this._hullStyles.TryGetValue(clusterId, out (Brush Fill, Pen Outline) style)) {
                style = ClusterHulls.CreateStyle(
                    this._accentBrush ?? Brushes.Blue,
                    clusterId,
                    this._hullBrush ?? Brushes.LightGray,
                    this._hullPen ?? new Pen(Brushes.Gray, 1.0)
                );
                this._hullStyles[clusterId] = style;
            }
            return style;
        }
        #endregion

        #region Verbindungen
        /// <summary>Die Linienstärke wächst mit der Anzahl gemeinsamer Animes (in fünf Stufen, die Stifte werden wiederverwendet).</summary>
        private Pen GetPen(TagEdgeViewModel edge) {
            int bucket = (int)Math.Round(edge.Weight * 4);
            (int, bool) key = (bucket, edge.IsHighlighted);
            if (!this._pens.TryGetValue(key, out Pen? pen)) {
                pen = new Pen(edge.IsHighlighted ? this._accentBrush : this._edgeBrush, 1.2 + bucket * 1.0) {
                    StartLineCap = PenLineCap.Round,
                    EndLineCap = PenLineCap.Round
                };
                if (pen.CanFreeze) {
                    pen.Freeze();
                }
                this._pens[key] = pen;
            }
            return pen;
        }
        #endregion
    }
}
