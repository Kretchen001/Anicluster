using BiboAnime.datatypes;
using System.Diagnostics;
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
            labelNameHeadLine.Content = oneAnime.name;

            setDisplay();
        }

        private void setDisplay() {
            textBoxName.Text = oneAnime.name;
            textBoxOriginalName.Text = oneAnime.originalName;
            if (oneAnime.favorite) {
                checkBoxFavorit.IsChecked = true;
                checkBoxNotFavorit.IsChecked = false;
            }
            else {
                checkBoxFavorit.IsChecked = false;
                checkBoxNotFavorit.IsChecked = true;
            }
            textBoxUrlAnimePlanet.Text = oneAnime.urlAnimePlanet;
        }

        private void clickCallLink(object sender, RoutedEventArgs e) {
            Process.Start(new ProcessStartInfo {
                FileName = oneAnime.urlAnimePlanet,
                UseShellExecute = true
            });
        }

        private void clickCloseButton(object sender, RoutedEventArgs e) {

        }

        private void clickEditEnitity(object sender, RoutedEventArgs e) {

        }

        private void clickRemoveEntity(object sender, RoutedEventArgs e) {

        }

        private void clickChoosenTags(object sender, RoutedEventArgs e) {
            ShowDetailsTags showDetailsTagsWindows = new ShowDetailsTags(oneAnime.tags);
            showDetailsTagsWindows.ShowDialog();
        }
    }
}