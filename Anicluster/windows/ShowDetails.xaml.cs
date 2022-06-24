using BiboAnime.databaseLiteDB;
using BiboAnime.datatypes;
using System.Diagnostics;
using System.Windows;

namespace Anicluster.windows {
    /// <summary>
    /// Interaktionslogik für ShowDetails.xaml
    /// </summary>
    public partial class ShowDetails : Window {

        public AnimeData oneAnime;

        databaseController dbController = new databaseController();

        public ShowDetails(AnimeData oneAnime) {
            InitializeComponent();
            this.oneAnime = oneAnime;
            labelNameHeadLine.Content = oneAnime.name;

            setDisplay();
            showTier();
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
            this.Close();
        }

        private void clickEditEnitity(object sender, RoutedEventArgs e) {

        }

        private void clickRemoveEntity(object sender, RoutedEventArgs e) {
            var yesNo = MessageBox.Show("Möchten Sie " + oneAnime.name + " wirklich löschen?",
                "Löschen",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (yesNo == MessageBoxResult.Yes) {
                bool del = dbController.deleteAnimeFromDB(oneAnime);
                if (del) {
                    MessageBox.Show(oneAnime.name + " gelöscht!",
                        "Gelöscht",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    this.Close();
                }
            }
        }

        private void clickChoosenTags(object sender, RoutedEventArgs e) {
            ShowDetailsTags showDetailsTagsWindows = new ShowDetailsTags(oneAnime.tags);
            showDetailsTagsWindows.ShowDialog();
        }

        private void showTier() {
            int S = 0, A = 0, B = 0, C = 0, D = 0;
            switch (oneAnime.tier) {
                case AnimeTier.S: S = 2; break;
                case AnimeTier.A: A = 2; break;
                case AnimeTier.B: B = 2; break;
                case AnimeTier.C: C = 2; break;
                case AnimeTier.D: D = 2; break;
                default: break;
            }
            labelTierS.BorderThickness = new Thickness(S);
            labelTierA.BorderThickness = new Thickness(A);
            labelTierB.BorderThickness = new Thickness(B);
            labelTierC.BorderThickness = new Thickness(C);
            labelTierD.BorderThickness = new Thickness(D);
        }
    }
}