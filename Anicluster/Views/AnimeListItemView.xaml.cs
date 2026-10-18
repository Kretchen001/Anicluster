using Anicluster.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Anicluster.Views {

    /// <summary>
    /// Ein Anime in der Liste des Hauptfensters.
    /// </summary>
    public partial class AnimeListItemView : UserControl {

        public AnimeListItemView() {
            this.InitializeComponent();
        }

        private void MenuItemDetailsAndEdit_Click(object sender, RoutedEventArgs e) {
            if (this.DataContext is AnimeListItemViewModel item) {
                item.RequestEdit();
            }
        }

        private void MenuItemDelete_Click(object sender, RoutedEventArgs e) {
            if (this.DataContext is AnimeListItemViewModel item) {
                item.RequestDelete();
            }
        }
    }
}
