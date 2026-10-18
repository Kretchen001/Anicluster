using Anicluster.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Anicluster.Views {

    /// <summary>
    /// Dialog zur Auswahl der Tags eines Animes.
    /// </summary>
    public partial class TagSelectionView : UserControl {

        private TagSelectionViewModel? ViewModel => this.DataContext as TagSelectionViewModel;

        public TagSelectionView() {
            this.InitializeComponent();
        }

        private void BtnClearFilter_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.ClearFilter();
        }

        private void BtnFinish_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.Accept();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.Cancel();
        }
    }
}
