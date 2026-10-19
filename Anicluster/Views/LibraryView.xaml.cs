using Anicluster.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Anicluster.Views {

    /// <summary>
    /// Bibliothek mit Karten- und Tabellenansicht. Der Code-Behind übersetzt nur Tabellen-Gesten
    /// (Kopfklick, Doppelklick, Rechtsklick) in Aufrufe am <see cref="LibraryViewModel"/>.
    /// </summary>
    public partial class LibraryView : UserControl {

        private LibraryViewModel? ViewModel => this.DataContext as LibraryViewModel;

        public LibraryView() {
            this.InitializeComponent();
        }

        /// <summary>Ein Klick auf den Spaltenkopf sortiert über das ViewModel (statt über die DataGrid-eigene Sortierung).</summary>
        private void Table_Sorting(object sender, DataGridSortingEventArgs e) {
            e.Handled = true;
            AnimeSortField? field = e.Column.SortMemberPath switch {
                "Name" => AnimeSortField.Name,
                "Tier" => AnimeSortField.Tier,
                "State" => AnimeSortField.State,
                "Favorite" => AnimeSortField.Favorite,
                _ => null
            };
            if (field.HasValue) {
                this.ViewModel?.SortBy(field.Value);
            }
        }

        private void TableRow_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            if (sender is DataGridRow { Item: AnimeListItemViewModel item }) {
                item.RequestEdit();
            }
        }

        private void TableRow_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e) {
            if (sender is DataGridRow row) {
                row.IsSelected = true;
            }
        }

        private void TableMenuItemDetails_Click(object sender, RoutedEventArgs e) {
            (this.Table.SelectedItem as AnimeListItemViewModel)?.RequestEdit();
        }

        private void TableMenuItemDelete_Click(object sender, RoutedEventArgs e) {
            (this.Table.SelectedItem as AnimeListItemViewModel)?.RequestDelete();
        }
    }
}
