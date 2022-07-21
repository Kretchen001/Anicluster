using BiboAnime.databaseLiteDB;
using BiboAnime.datatypes;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Anicluster.windows {

    /// <summary>
    /// Interaktionslogik für ShowDetails.xaml
    /// </summary>
    public partial class ShowDetails : Window {

        public AnimeData oneAnime = new AnimeData();
        private AnimeData changedData = new AnimeData();

        public Action<bool> shouldViewUpdated;

        databaseController dbController = new databaseController();

        public ShowDetails(AnimeData givenAnime) {
            InitializeComponent();

            oneAnime = givenAnime.DeepClone();
            changedData = givenAnime.DeepClone();

            setDisplay();
            showTier();
            showRating();
            showStaffeln();
            labelStatus.Content = oneAnime.status.ToString();
            labelNameHeadLine.Content = givenAnime.name;
        }

        private void setDisplay() {
            textBoxName.Text = changedData.name;
            textBoxOriginalName.Text = changedData.originalName;
            if (changedData.favorite) {
                checkBoxFavorit.IsChecked = true;
                checkBoxNotFavorit.IsChecked = false;
            }
            else {
                checkBoxFavorit.IsChecked = false;
                checkBoxNotFavorit.IsChecked = true;
            }
            textBoxUrlAnimePlanet.Text = changedData.urlAnimePlanet;
            textBoxRecommendation.Text = changedData.thirdPartyRecommendation;
        }

        private void showTier() {
            int S = 0, A = 0, B = 0, C = 0, D = 0;
            switch (changedData.tier) {
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

        private void showRating() {

            progressBarStoryRating.Value = changedData.rating.story;
            labelStoryRating.Content = changedData.rating.story;

            progressBarAnimationRating.Value = changedData.rating.animation;
            labelAnimationRating.Content = changedData.rating.animation;

            progressBarSpecialEffectsRating.Value = changedData.rating.specialEffect;
            labelSpecialEffectsRating.Content = changedData.rating.specialEffect;

            progressBarSoundRating.Value = changedData.rating.sound;
            labelSoundRating.Content = changedData.rating.sound;

            progressBarGermanDubRating.Value = changedData.rating.germanDub;
            labelGermanDubRating.Content = changedData.rating.germanDub;

            calculateAndPrintGeneralRating();
        }

        private void calculateAndPrintGeneralRating() {
            int ratingGeneral = 0;
            if (changedData.rating.germanDub == 0) {
                ratingGeneral = (int)((changedData.rating.story * 0.4) + (changedData.rating.sound * 0.2)
                    + (changedData.rating.animation * 0.3) + (changedData.rating.specialEffect * 0.1));
            }
            else {
                ratingGeneral = (int)((changedData.rating.story * 0.4) + (changedData.rating.sound * 0.1)
                    + (changedData.rating.animation * 0.3) + (changedData.rating.specialEffect * 0.1)
                    + (changedData.rating.germanDub * 0.1));
            }
            labelGeneralRating.Content = ratingGeneral;
            progressBarGeneralRating.Value = ratingGeneral;
        }

        private void showStaffeln() {

            textBlockForStaffelPrint.Text = "";
            textBoxTotalEpisodes.Text = "";

            if (changedData.staffeln != null) {
                for (int i = 0; i < changedData.staffeln.Count; i += 1) {
                    if (i != (changedData.staffeln.Count - 1)) {
                        textBlockForStaffelPrint.Text += "St." + (i + 1) + " : " + changedData.staffeln[i].episodes + " | ";
                    }
                    else {
                        textBlockForStaffelPrint.Text += "St." + (i + 1) + " : " + changedData.staffeln[i].episodes;
                    }
                }

                textBoxTotalEpisodes.Text = changedData.episodesTotal.ToString();
            }
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
                    shouldViewUpdated(true);
                    this.Close();
                }
            }
        }

        private void clickChoosenTags(object sender, RoutedEventArgs e) {
            ShowDetailsTags showDetailsTagsWindows = new ShowDetailsTags(changedData.tags);
            showDetailsTagsWindows.ShowDialog();
        }

        private void checkIfAllOriginal() {
            if (oneAnime == changedData) {
                buttonAcceptChanges.Visibility = Visibility.Hidden;
            }
            else {
                buttonAcceptChanges.Visibility = Visibility.Visible;
            }
        }

        private void toggleFavorite(object sender, RoutedEventArgs e) {
            changedData.favorite = !changedData.favorite;

            if (changedData.favorite) {
                checkBoxNotFavorit.IsChecked = false;
                checkBoxFavorit.IsChecked = true;
            }
            else {
                checkBoxNotFavorit.IsChecked = true;
                checkBoxFavorit.IsChecked = false;
            }

            checkIfAllOriginal();
        }

        private void changeStaffeln(object sender, RoutedEventArgs e) {

            StaffelManager staffelManagerWindow = new StaffelManager(changedData.staffeln); // changed, so any multi-Changes will be recognised
            staffelManagerWindow.ShowDialog();

            changedData.staffeln = staffelManagerWindow.getManagerStaffeln();

            checkIfAllOriginal();

            showStaffeln();
        }

        private void textChangeName(object sender, TextChangedEventArgs e) {
            changedData.name = textBoxName.Text;

            checkIfAllOriginal();
        }

        private void textChangeOriginalName(object sender, TextChangedEventArgs e) {
            if (oneAnime.originalName != textBoxOriginalName.Text) {
                buttonAcceptChanges.Visibility = Visibility.Visible;
                if (changedData.originalName != textBoxOriginalName.Text) {
                    changedData.originalName = textBoxOriginalName.Text;
                }
            }

            checkIfAllOriginal();
        }

        private void textChangeUrl(object sender, TextChangedEventArgs e) {
            if (oneAnime.urlAnimePlanet != textBoxUrlAnimePlanet.Text) {
                buttonAcceptChanges.Visibility = Visibility.Visible;
                if (changedData.urlAnimePlanet != textBoxUrlAnimePlanet.Text) {
                    changedData.urlAnimePlanet = textBoxUrlAnimePlanet.Text;
                }
            }

            checkIfAllOriginal();
        }

        private void textChangeRecomendation(object sender, TextChangedEventArgs e) {
            if (oneAnime.thirdPartyRecommendation != textBoxRecommendation.Text) {
                buttonAcceptChanges.Visibility = Visibility.Visible;
                if (changedData.thirdPartyRecommendation != textBoxRecommendation.Text) {
                    changedData.thirdPartyRecommendation = textBoxRecommendation.Text;
                }
            }

            checkIfAllOriginal();
        }
    }
}