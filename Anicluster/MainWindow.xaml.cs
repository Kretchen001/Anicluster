using Anicluster.Theming;
using Anicluster.ViewModels;
using Serilog;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;

namespace Anicluster {

    /// <summary>
    /// Hauptfenster. Enthält nur Weiterleitungen der Klicks an das <see cref="MainViewModel"/>.
    /// </summary>
    public partial class MainWindow : Window {

        private MainViewModel ViewModel => (MainViewModel)this.DataContext;

        public MainWindow() {
            this.InitializeComponent();
            WindowThemeHelper.Attach(this);
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.New, this.OnNewAnimeExecuted)); // Strg + N
        }

        private void OnNewAnimeExecuted(object sender, ExecutedRoutedEventArgs e) {
            this.ViewModel.CreateNewAnime();
        }

        /// <summary>Verstecktes Feature: Rechtsklick auf die Versionsnummer öffnet die SQL-Konsole.</summary>
        private void VersionText_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            e.Handled = true;
            this.ViewModel.OpenSqlConsole();
        }

        /// <summary>Öffnet den Link zum Projekt im Standardbrowser.</summary>
        private void ProjectLink_RequestNavigate(object sender, RequestNavigateEventArgs e) {
            e.Handled = true;
            try {
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            }
            catch (Win32Exception ex) {
                Log.Warning(ex, "Der Link {Uri} konnte nicht geöffnet werden.", e.Uri);
            }
        }

        private void BtnNewAnime_Click(object sender, RoutedEventArgs e) {
            this.ViewModel.CreateNewAnime();
        }

        private void BtnQuickAnime_Click(object sender, RoutedEventArgs e) {
            this.ViewModel.CreateQuickAnime();
        }
    }
}
