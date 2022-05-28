using BiboAnime.datatypes;
using System.Windows;

namespace Anicluster.windows {
    /// <summary>
    /// Interaktionslogik für ShowDetails.xaml
    /// </summary>
    public partial class ShowDetails : Window {

        public AnimeData oneAnime;

        public ShowDetails(AnimeData oneAnime) {
            InitializeComponent();
            this.oneAnime = oneAnime;
        }

        private void click_EditEnitity(object sender, RoutedEventArgs e) {

        }

        private void click_RemoveEntity(object sender, RoutedEventArgs e) {

        }
    }
}
