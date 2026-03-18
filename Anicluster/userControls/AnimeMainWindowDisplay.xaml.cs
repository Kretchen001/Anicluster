using Anicluster.windows;
using Modells.Anime;
using Modells.Anime.SoundRating;
using Modells.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Anicluster.userControls {

    /// <summary>
    /// Interaktionslogik für AnimeMainWindowDisplay.xaml
    /// </summary>
    public partial class AnimeMainWindowDisplay : UserControl {

        public AnimeViewModel Anime { get; set; }
        private static Dictionary<State, string> TranslationOfStateDeu = new Dictionary<State, string>() {
            { State.Wishlist, "Wunschliste" },
            { State.Started, "Begonnen" },
            { State.Finished, "Beendet" },
            { State.Interrupted, "Unterbrochen" },
            { State.Canceled, "Abgebrochen" },
        };
        public string TranslationOfState { get; set; }

        public AnimeMainWindowDisplay(AnimeViewModel anime) {
            this.InitializeComponent();
            this.Anime = anime;
            if (this.Anime.IsFavorite) {
                this.StarPath.Fill = Brushes.Gold;
            }
            this.TranslationOfState = TranslationOfStateDeu[anime.State];
            this.DataContext = this;
        }
        private void MenuItemDetailsAndEdit_Click(object sender, RoutedEventArgs e) {
            // Neues UC erzeugen
            EditAnime editAnimeUC = new EditAnime(this.Anime.Id);

            // MainWindow holen
            if (Window.GetWindow(this) is MainWindow mainWindow) {
                mainWindow.ShowEditAnime(editAnimeUC);
            }
        }
    }
}
