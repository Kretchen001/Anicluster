using Anicluster.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Anicluster.Views {

    /// <summary>
    /// Dialog "Metadaten laden". Der Code-Behind leitet nur Klicks und die Eingabetaste an das <see cref="MetadataImportViewModel"/> weiter.
    /// </summary>
    public partial class MetadataImportView : UserControl {

        private MetadataImportViewModel? ViewModel => this.DataContext as MetadataImportViewModel;

        public MetadataImportView() {
            this.InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e) {
            this.TBQuery.Focus();
            this.TBQuery.SelectAll();
            if (this.ViewModel is not null) {
                await this.ViewModel.StartAsync();
            }
        }

        private async void TBQuery_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter && this.ViewModel is { CanSearch: true } viewModel) {
                e.Handled = true;
                await viewModel.SearchAsync();
            }
        }

        private async void BtnSearch_Click(object sender, RoutedEventArgs e) {
            if (this.ViewModel is not null) {
                await this.ViewModel.SearchAsync();
            }
        }

        private void BtnApply_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.Apply();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.Cancel();
        }
    }
}
