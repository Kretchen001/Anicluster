using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Anicluster.Views {

    /// <summary>
    /// Gemeinsame Drag-&amp;-Drop-Hilfen zum Umsortieren der Einträge einer ListView.
    /// Reine UI-Geste, die eigentliche Verschiebung übernimmt das ViewModel.
    /// </summary>
    internal static class ListReorderHelper {

        /// <summary>Startet das Ziehen des angeklickten Eintrags (PreviewMouseLeftButtonDown).</summary>
        public static void StartDrag(MouseButtonEventArgs e) {
            ListViewItem? item = FindAncestor<ListViewItem>(e.OriginalSource as DependencyObject);
            if (item is not null) {
                DragDrop.DoDragDrop(item, item.DataContext, DragDropEffects.Move);
            }
        }

        /// <summary>Erlaubt das Ablegen nur für Einträge vom Typ <typeparamref name="T"/> (DragOver).</summary>
        public static void AllowDrop<T>(DragEventArgs e) {
            e.Effects = e.Data.GetDataPresent(typeof(T)) ? DragDropEffects.Move : DragDropEffects.None;
            e.Handled = true;
        }

        /// <summary>Liefert den abgelegten Eintrag und die Zielposition (Drop).</summary>
        /// <returns><c>false</c>, wenn nicht auf einem Eintrag abgelegt wurde.</returns>
        public static bool TryGetDrop<T>(ListView listView, DragEventArgs e, out T dropped, out int targetIndex) where T : class {
            dropped = null!;
            targetIndex = -1;
            if (!e.Data.GetDataPresent(typeof(T))) {
                return false;
            }
            ListViewItem? target = FindAncestor<ListViewItem>(e.OriginalSource as DependencyObject);
            if (target is null) {
                return false;
            }
            dropped = (T)e.Data.GetData(typeof(T));
            targetIndex = listView.Items.IndexOf(target.DataContext);
            return targetIndex >= 0;
        }

        private static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject {
            while (current is not null) {
                if (current is T ancestor) {
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
