using Anicluster.ViewModels;
using System.Windows;
using System.Windows.Media;

namespace Anicluster.Views {

    /// <summary>
    /// Zeichnet die Verbindungslinien zwischen den Tags des Netzwerks. Wird bei jedem Schritt der Simulation
    /// über <see cref="UIElement.InvalidateVisual"/> neu gezeichnet. Das ist deutlich schneller als eine Linie pro Verbindung als eigenes Element.
    /// Die Farben kommen aus den Theme-Ressourcen (EdgeBrush, AccentBrush).
    /// </summary>
    public class TagEdgeLayer : FrameworkElement {

        public static readonly DependencyProperty EdgesProperty = DependencyProperty.Register(
            nameof(Edges),
            typeof(IReadOnlyList<TagEdgeViewModel>),
            typeof(TagEdgeLayer),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender)
        );

        private readonly Dictionary<(int Bucket, bool Highlighted), Pen> _pens = [];
        private Brush? _edgeBrush;
        private Brush? _accentBrush;

        public IReadOnlyList<TagEdgeViewModel>? Edges {
            get => (IReadOnlyList<TagEdgeViewModel>?)this.GetValue(EdgesProperty);
            set => this.SetValue(EdgesProperty, value);
        }

        protected override void OnRender(DrawingContext drawingContext) {
            base.OnRender(drawingContext);
            IReadOnlyList<TagEdgeViewModel>? edges = this.Edges;
            if (edges is null) {
                return;
            }
            this.RefreshBrushes();
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

        /// <summary>Holt die aktuellen Theme-Pinsel. Hat sich das Design geändert, werden die Stifte neu erzeugt.</summary>
        private void RefreshBrushes() {
            Brush edgeBrush = this.TryFindResource("EdgeBrush") as Brush ?? Brushes.Gray;
            Brush accentBrush = this.TryFindResource("AccentBrush") as Brush ?? Brushes.Blue;
            if (!ReferenceEquals(edgeBrush, this._edgeBrush) || !ReferenceEquals(accentBrush, this._accentBrush)) {
                this._pens.Clear();
                this._edgeBrush = edgeBrush;
                this._accentBrush = accentBrush;
            }
        }

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
    }
}
