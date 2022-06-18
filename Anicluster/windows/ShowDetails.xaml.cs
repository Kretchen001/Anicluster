using BiboAnime.datatypes;
using System.Windows;
using System.Windows.Media;

namespace Anicluster.windows {
    /// <summary>
    /// Interaktionslogik für ShowDetails.xaml
    /// </summary>
    public partial class ShowDetails : Window {

        public AnimeData oneAnime;

        private string a { get; set; } = "ahfsd";

        public ShowDetails(AnimeData oneAnime) {
            InitializeComponent();
            this.oneAnime = oneAnime;
            labelNameHeadLine.Content = oneAnime.name;
        }

        private void clickCallLink(object sender, RoutedEventArgs e) {

        }

        private void clickCloseButton(object sender, RoutedEventArgs e) {

        }

        private void clickEditEnitity(object sender, RoutedEventArgs e) {

        }

        private void clickRemoveEntity(object sender, RoutedEventArgs e) {

        }

        private void clickChoosenTags(object sender, RoutedEventArgs e) {

        }
    }
}