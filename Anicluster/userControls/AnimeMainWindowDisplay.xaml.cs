using Modells.Anime;
using Modells.Anime.SoundRating;
using Modells.ViewModel;
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
            InitializeComponent();
            this.Anime = anime;
            if (Anime.IsFavorite) {
                StarPath.Fill = Brushes.Gold;
            }
            TranslationOfState = TranslationOfStateDeu[anime.State];
            this.DataContext = this;
        }
    }
}
