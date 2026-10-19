using Anicluster.Theming;
using Anicluster.ViewModels;
using System.Windows;
using System.Windows.Input;

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

        private void BtnNewAnime_Click(object sender, RoutedEventArgs e) {
            this.ViewModel.CreateNewAnime();
        }

        private void BtnQuickAnime_Click(object sender, RoutedEventArgs e) {
            this.ViewModel.CreateQuickAnime();
        }
    }
}
