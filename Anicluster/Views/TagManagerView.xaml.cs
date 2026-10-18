using Anicluster.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Anicluster.Views {

    /// <summary>
    /// Dialog zum Verwalten aller Tags.
    /// </summary>
    public partial class TagManagerView : UserControl {

        private TagManagerViewModel? ViewModel => this.DataContext as TagManagerViewModel;

        public TagManagerView() {
            this.InitializeComponent();
        }

        private void MenuItemAddTag_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.OpenAddTagPopup();
        }

        private void MenuItemDeleteSelectedTag_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.DeleteSelectedTag();
        }

        private void MenuItemTagSearch_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.ToggleSearch();
        }

        private void BtnClearSearch_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.ClearSearch();
        }

        private void BtnPopUp_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.AddTag();
        }
    }
}
