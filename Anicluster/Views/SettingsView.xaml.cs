using Anicluster.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Anicluster.Views {

    /// <summary>
    /// Einstellungsseite (Design und eigene Farben).
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
    }
}
