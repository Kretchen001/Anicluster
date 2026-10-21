using Anicluster.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Anicluster.Views {

    /// <summary>
    /// Einstellungsseite (Design, eigene Farben und Datensicherung).
    /// </summary>
    public partial class SettingsView : UserControl {

        private SettingsViewModel? ViewModel => this.DataContext as SettingsViewModel;

        public SettingsView() {
            this.InitializeComponent();
        }

        private void BtnUseLight_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.UseLightAsBase();
        }

        private void BtnUseDark_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.UseDarkAsBase();
        }

        private async void BtnExport_Click(object sender, RoutedEventArgs e) {
            if (this.ViewModel is not null) {
                await this.ViewModel.ExportAsync();
            }
        }

        private async void BtnImport_Click(object sender, RoutedEventArgs e) {
            if (this.ViewModel is not null) {
                await this.ViewModel.ImportAsync();
            }
        }
    }
}
