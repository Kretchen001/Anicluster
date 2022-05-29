using BiboAnime.datatypes;
using System.Windows;
using System.Windows.Media;

namespace Anicluster.windows {
    /// <summary>
    /// Interaktionslogik für ShowDetails.xaml
    /// </summary>
    public partial class ShowDetails : Window {

        public AnimeData oneAnime;
        private bool isFavorite;

        public ShowDetails(AnimeData oneAnime) {
            InitializeComponent();
            this.oneAnime = oneAnime;

            labelName.Content = oneAnime.name;
            labelOriginalName.Content = oneAnime.originalName;
            labelRatingGeneral.Content = oneAnime.rating.general;
            labelFavorite.Content = oneAnime.favorite.ToString();

            isFavorite = oneAnime.favorite;
            if (isFavorite) {
                labelFavorite.Foreground = new SolidColorBrush(Colors.DarkGreen);
                labelFavorite.Content = "Favorisiert";
            }
            else {
                labelFavorite.Foreground = new SolidColorBrush(Colors.DarkRed);
                labelFavorite.Content = "Kein Favorit";
            }
        }

        private void click_EditEnitity(object sender, RoutedEventArgs e) {

        }

        private void click_RemoveEntity(object sender, RoutedEventArgs e) {

        }

        private void click_ShowRatingDetails(object sender, RoutedEventArgs e) {

        }

        private void click_DoubleClickChangeFavorite(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            if (isFavorite) {
                isFavorite = false;
                labelFavorite.Foreground = new SolidColorBrush(Colors.DarkRed);
                labelFavorite.Content = "Kein Favorit";
            }
            else {
                isFavorite = true;
                labelFavorite.Foreground = new SolidColorBrush(Colors.DarkGreen);
                labelFavorite.Content = "Favorisiert";
            }
        }
    }
}
