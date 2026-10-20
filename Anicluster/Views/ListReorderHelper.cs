using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Anicluster.Views {

    /// <summary>
    /// Macht die Zeilen eines DataGrids per Drag &amp; Drop umsortierbar. Reine UI-Geste: Das Ziehen startet erst nach
    /// einer kleinen Mausbewegung (ein einfacher Klick markiert weiter die Zeile), die Verschiebung selbst übernimmt der Aufrufer.
    /// </summary>
    internal class ListReorderHelper<T> where T : class {

        private readonly DataGrid _grid;
        private readonly Action<T, int> _move;
        private Point _dragStart;
        private T? _dragItem;

        /// <param name="grid">Das DataGrid, dessen Zeilen sortiert werden sollen</param>
        /// <param name="move">Wird mit dem verschobenen Eintrag und der Zielposition aufgerufen</param>
        public ListReorderHelper(DataGrid grid, Action<T, int> move) {
            this._grid = grid;
            this._move = move;
            grid.AllowDrop = true;
            grid.PreviewMouseLeftButtonDown += this.OnMouseDown;
            grid.PreviewMouseLeftButtonUp += (sender, e) => this._dragItem = null;
            grid.PreviewMouseMove += this.OnMouseMove;
            grid.DragOver += this.OnDragOver;
            grid.Drop += this.OnDrop;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e) {
            this._dragItem = FindAncestor<DataGridRow>(e.OriginalSource as DependencyObject)?.Item as T;
            this._dragStart = e.GetPosition(null);
        }

        private void OnMouseMove(object sender, MouseEventArgs e) {
            if (e.LeftButton != MouseButtonState.Pressed || this._dragItem is null) {
                return;
            }
            Point position = e.GetPosition(null);
            if (Math.Abs(position.X - this._dragStart.X) < SystemParameters.MinimumHorizontalDragDistance
                && Math.Abs(position.Y - this._dragStart.Y) < SystemParameters.MinimumVerticalDragDistance) {
                return;
            }
            T item = this._dragItem;
            this._dragItem = null;
            DragDrop.DoDragDrop(this._grid, new DataObject(typeof(T), item), DragDropEffects.Move);
        }

        private void OnDragOver(object sender, DragEventArgs e) {
            e.Effects = e.Data.GetDataPresent(typeof(T)) ? DragDropEffects.Move : DragDropEffects.None;
            e.Handled = true;
        }

        private void OnDrop(object sender, DragEventArgs e) {
            if (!e.Data.GetDataPresent(typeof(T))) {
                return;
            }
            DataGridRow? target = FindAncestor<DataGridRow>(e.OriginalSource as DependencyObject);
            if (target is null) {
                return;
            }
            int targetIndex = this._grid.Items.IndexOf(target.Item);
            if (targetIndex >= 0) {
                this._move((T)e.Data.GetData(typeof(T)), targetIndex);
            }
        }

        private static TAncestor? FindAncestor<TAncestor>(DependencyObject? current) where TAncestor : DependencyObject {
            while (current is not null) {
                if (current is TAncestor ancestor) {
                    return ancestor;
                }
                // Nicht-visuelle Elemente (z. B. Text-Runs) haben keinen visuellen Parent
                current = current is Visual or System.Windows.Media.Media3D.Visual3D
                    ? VisualTreeHelper.GetParent(current)
                    : LogicalTreeHelper.GetParent(current);
            }
            return null;
        }
    }
}
