using Anicluster.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Anicluster.Views {

    /// <summary>
    /// Ein Anime als Karte in der Bibliothek.
    /// </summary>
    public partial class AnimeListItemView : UserControl {

        private AnimeListItemViewModel? ViewModel => this.DataContext as AnimeListItemViewModel;

        public AnimeListItemView() {
            this.InitializeComponent();
        }

        private void Card_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (e.ClickCount == 2) {
                this.ViewModel?.RequestEdit();
            }
        }

        private void MenuItemDetailsAndEdit_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.RequestEdit();
        }

        private void MenuItemDelete_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.RequestDelete();
        }
    }
}
