using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Anicluster.Views {

    /// <summary>
    /// Ordnet gleich große Einträge zeilenweise nebeneinander an (wie ein WrapPanel) und erzeugt nur die Einträge,
    /// die gerade im sichtbaren Bereich liegen. Dadurch bleibt auch eine Liste mit sehr vielen Kacheln flüssig.
    /// Die Größe eines Eintrags wird am ersten Eintrag gemessen, alle Einträge müssen gleich groß sein.
    /// Gescrollt wird senkrecht, der umgebende <see cref="ScrollViewer"/> braucht <c>CanContentScroll="True"</c>.
    /// </summary>
    public class VirtualizingWrapPanel : VirtualizingPanel, IScrollInfo {

        private Size _itemSize = new Size(0, 0);
        private Size _extent = new Size(0, 0);
        private Size _viewport = new Size(0, 0);
        private Point _offset = new Point(0, 0);
        private int _columns = 1;

        #region IScrollInfo
        public ScrollViewer? ScrollOwner { get; set; }

        public bool CanVerticallyScroll { get; set; }

        public bool CanHorizontallyScroll { get; set; }

        public double ExtentWidth => this._extent.Width;

        public double ExtentHeight => this._extent.Height;

        public double ViewportWidth => this._viewport.Width;

        public double ViewportHeight => this._viewport.Height;

        public double HorizontalOffset => this._offset.X;

        public double VerticalOffset => this._offset.Y;

        /// <summary>Schrittweite für Zeilen- und Mausradbewegung</summary>
        private double LineStep => this._itemSize.Height > 0 ? this._itemSize.Height / 4 : 40;

        public void LineUp() {
            this.SetVerticalOffset(this.VerticalOffset - this.LineStep);
        }

        public void LineDown() {
            this.SetVerticalOffset(this.VerticalOffset + this.LineStep);
        }

        public void PageUp() {
            this.SetVerticalOffset(this.VerticalOffset - this.ViewportHeight);
        }

        public void PageDown() {
            this.SetVerticalOffset(this.VerticalOffset + this.ViewportHeight);
        }

        public void MouseWheelUp() {
            this.SetVerticalOffset(this.VerticalOffset - this.LineStep * 2);
        }

        public void MouseWheelDown() {
            this.SetVerticalOffset(this.VerticalOffset + this.LineStep * 2);
        }

        public void LineLeft() { }

        public void LineRight() { }

        public void PageLeft() { }

        public void PageRight() { }

        public void MouseWheelLeft() { }

        public void MouseWheelRight() { }

        public void SetHorizontalOffset(double offset) { }

        public void SetVerticalOffset(double offset) {
            double maximum = Math.Max(0, this._extent.Height - this._viewport.Height);
            double clamped = Math.Max(0, Math.Min(offset, maximum));
            if (Math.Abs(clamped - this._offset.Y) < 0.01) {
                return;
            }
            this._offset.Y = clamped;
            this.ScrollOwner?.InvalidateScrollInfo();
            this.InvalidateMeasure();
        }

        public Rect MakeVisible(Visual visual, Rect rectangle) {
            return rectangle;
        }
        #endregion

        protected override Size MeasureOverride(Size availableSize) {
            _ = this.InternalChildren; // stellt sicher, dass der Generator der Einträge bereitsteht
            IItemContainerGenerator generator = this.ItemContainerGenerator;
            ItemsControl? owner = ItemsControl.GetItemsOwner(this);
            int count = owner is null ? 0 : owner.Items.Count;
            if (count == 0) {
                this.UpdateScrollInfo(new Size(0, 0), new Size(0, 0));
                return new Size(0, 0);
            }
            if (this._itemSize.Width <= 0 || this._itemSize.Height <= 0) {
                this.Realize(generator, 0, 0); // der erste Eintrag liefert die Größe aller Einträge
                if (this._itemSize.Width <= 0 || this._itemSize.Height <= 0) {
                    return new Size(0, 0);
                }
            }
            double width = double.IsInfinity(availableSize.Width) ? this._itemSize.Width : availableSize.Width;
            this._columns = Math.Max(1, (int)Math.Floor(width / this._itemSize.Width));
            int rows = (count + this._columns - 1) / this._columns;
            double extentHeight = rows * this._itemSize.Height;
            double viewportHeight = double.IsInfinity(availableSize.Height) ? extentHeight : availableSize.Height;
            this.UpdateScrollInfo(new Size(width, extentHeight), new Size(width, viewportHeight));
            this._offset.Y = Math.Max(0, Math.Min(this._offset.Y, Math.Max(0, extentHeight - viewportHeight)));

            int firstRow = (int)Math.Floor(this._offset.Y / this._itemSize.Height);
            int lastRow = (int)Math.Ceiling((this._offset.Y + viewportHeight) / this._itemSize.Height) - 1;
            int firstIndex = Math.Max(0, firstRow * this._columns);
            int lastIndex = Math.Min(count - 1, (lastRow + 1) * this._columns - 1);
            this.Realize(generator, firstIndex, lastIndex);
            this.CleanUp(generator, firstIndex, lastIndex);
            return new Size(width, viewportHeight);
        }

        protected override Size ArrangeOverride(Size finalSize) {
            IItemContainerGenerator generator = this.ItemContainerGenerator;
            for (int i = 0; i < this.InternalChildren.Count; i++) {
                int itemIndex = generator.IndexFromGeneratorPosition(new GeneratorPosition(i, 0));
                int row = itemIndex / this._columns;
                int column = itemIndex % this._columns;
                this.InternalChildren[i].Arrange(new Rect(
                    column * this._itemSize.Width,
                    row * this._itemSize.Height - this._offset.Y,
                    this._itemSize.Width,
                    this._itemSize.Height
                ));
            }
            return finalSize;
        }

        protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args) {
            switch (args.Action) {
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                    this.RemoveInternalChildRange(args.Position.Index, args.ItemUICount);
                    break;
            }
            this.InvalidateMeasure();
        }

        /// <summary>Erzeugt die Einträge von <paramref name="firstIndex"/> bis <paramref name="lastIndex"/>, soweit sie noch fehlen.</summary>
        private void Realize(IItemContainerGenerator generator, int firstIndex, int lastIndex) {
            GeneratorPosition start = generator.GeneratorPositionFromIndex(firstIndex);
            int childIndex = start.Offset == 0 ? start.Index : start.Index + 1;
            using (generator.StartAt(start, GeneratorDirection.Forward, true)) {
                for (int index = firstIndex; index <= lastIndex; index++, childIndex++) {
                    UIElement? child = generator.GenerateNext(out bool isNewlyRealized) as UIElement;
                    if (child is null) {
                        break;
                    }
                    if (isNewlyRealized) {
                        if (childIndex >= this.InternalChildren.Count) {
                            this.AddInternalChild(child);
                        }
                        else {
                            this.InsertInternalChild(childIndex, child);
                        }
                        generator.PrepareItemContainer(child);
                    }
                    bool sizeKnown = this._itemSize.Width > 0 && this._itemSize.Height > 0;
                    child.Measure(sizeKnown ? this._itemSize : new Size(double.PositiveInfinity, double.PositiveInfinity));
                    if (!sizeKnown) {
                        this._itemSize = child.DesiredSize;
                    }
                }
            }
        }

        /// <summary>Gibt die Einträge frei, die außerhalb des sichtbaren Bereichs liegen.</summary>
        private void CleanUp(IItemContainerGenerator generator, int firstIndex, int lastIndex) {
            for (int i = this.InternalChildren.Count - 1; i >= 0; i--) {
                GeneratorPosition position = new GeneratorPosition(i, 0);
                int itemIndex = generator.IndexFromGeneratorPosition(position);
                if (itemIndex < firstIndex || itemIndex > lastIndex) {
                    generator.Remove(position, 1);
                    this.RemoveInternalChildRange(i, 1);
                }
            }
        }

        private void UpdateScrollInfo(Size extent, Size viewport) {
            bool changed = extent != this._extent || viewport != this._viewport;
            this._extent = extent;
            this._viewport = viewport;
            if (changed) {
                this.ScrollOwner?.InvalidateScrollInfo();
            }
        }
    }
}
