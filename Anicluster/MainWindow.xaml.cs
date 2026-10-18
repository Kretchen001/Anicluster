using Anicluster.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Anicluster {

    /// <summary>
    /// Hauptfenster. Enthält nur Weiterleitungen der Menü-Klicks an das <see cref="MainViewModel"/>.
    /// </summary>
    public partial class MainWindow : Window {

        private MainViewModel ViewModel => (MainViewModel)this.DataContext;

        public MainWindow() {
            this.InitializeComponent();
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.New, this.OnNewAnimeExecuted)); // Strg + N
        }

        private void OnNewAnimeExecuted(object sender, ExecutedRoutedEventArgs e) {
            this.ViewModel.CreateNewAnime();
        }

        private void MenuItemNewAnime_Click(object sender, RoutedEventArgs e) {
            this.ViewModel.CreateNewAnime();
        }

        private void MenuItemNewAnimeSmall_Click(object sender, RoutedEventArgs e) {
            this.ViewModel.CreateQuickAnime();
        }

        private void MenuItemTags_Click(object sender, RoutedEventArgs e) {
            this.ViewModel.OpenTagManager();
        }

        /// <summary>Gemeinsamer Handler aller Sortier-Einträge, das Kriterium steht im Tag des Menüeintrags.</summary>
        private void MenuItemSort_Click(object sender, RoutedEventArgs e) {
            if (sender is MenuItem { Tag: AnimeSortField field }) {
                this.ViewModel.SortAnimes(field);
            }
        }
    }
}
