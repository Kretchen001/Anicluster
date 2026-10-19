using Anicluster.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Anicluster.Views {

    /// <summary>
    /// Seite zum Verwalten aller Tags.
    /// </summary>
    public partial class TagManagerView : UserControl {

        private TagManagerViewModel? ViewModel => this.DataContext as TagManagerViewModel;

        public TagManagerView() {
            this.InitializeComponent();
        }

        private void BtnAddTag_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.AddTag();
        }

        private void NewTagBox_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                this.ViewModel?.AddTag();
            }
        }

        private void BtnDeleteTag_Click(object sender, RoutedEventArgs e) {
            if (sender is FrameworkElement { DataContext: Modells.Anime.Tag tag }) {
                this.ViewModel?.DeleteTag(tag);
            }
        }
    }
}
