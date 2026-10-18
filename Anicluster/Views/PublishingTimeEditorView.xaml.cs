using Anicluster.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Anicluster.Views {

    /// <summary>
    /// Dialog zum Bearbeiten des Veröffentlichungszeitraums.
    /// </summary>
    public partial class PublishingTimeEditorView : UserControl {

        private PublishingTimeEditorViewModel? ViewModel => this.DataContext as PublishingTimeEditorViewModel;

        public PublishingTimeEditorView() {
            this.InitializeComponent();
        }

        private void BtnAddInterruption_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.AddInterruption();
        }

        private void BtnAccept_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.Accept();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.Cancel();
        }
    }
}
