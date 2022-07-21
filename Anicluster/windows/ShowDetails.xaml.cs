using BiboAnime.databaseLiteDB;
using BiboAnime.datatypes;
using System;
using System.Collections.Generic;
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

            this.oneAnime = new AnimeData() {
                id = givenAnime.id,
                favorite = givenAnime.favorite,
                name = givenAnime.name,
                originalName = givenAnime.originalName,
                rating = givenAnime.rating,
                staffeln = givenAnime.staffeln,
                status = givenAnime.status,
                tier = givenAnime.tier,
                tags = givenAnime.tags,
                thirdPartyRecommendation = givenAnime.thirdPartyRecommendation,
                urlAnimePlanet = givenAnime.urlAnimePlanet,
                episodesTotal = givenAnime.episodesTotal,
                movies = givenAnime.movies,
                ovh = givenAnime.ovh
            };

            labelNameHeadLine.Content = givenAnime.name;

            this.changedData = new AnimeData() { 
                //id = givenAnime.id,
                //favorite = givenAnime.favorite,
                //name = givenAnime.name,
                //originalName = givenAnime.originalName,
                //rating = givenAnime.rating,
                //staffeln = givenAnime.staffeln,
                //status = givenAnime.status,
                //tier = givenAnime.tier,
                //tags = givenAnime.tags,
                //thirdPartyRecommendation = givenAnime.thirdPartyRecommendation,
                //urlAnimePlanet = givenAnime.urlAnimePlanet,
                //episodesTotal = givenAnime.episodesTotal,
                //movies = givenAnime.movies,
                //ovh = givenAnime.ovh
            };

            setDisplay();
            showTier();
            showRating();
            showStaffeln();
            labelStatus.Content = oneAnime.status.ToString();
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
            textBoxRecommendation.Text = oneAnime.thirdPartyRecommendation;
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

        private void showRating() {

            progressBarStoryRating.Value = oneAnime.rating.story;
            labelStoryRating.Content = oneAnime.rating.story;

            progressBarAnimationRating.Value = oneAnime.rating.animation;
            labelAnimationRating.Content = oneAnime.rating.animation;

            progressBarSpecialEffectsRating.Value = oneAnime.rating.specialEffect;
            labelSpecialEffectsRating.Content = oneAnime.rating.specialEffect;

            progressBarSoundRating.Value = oneAnime.rating.sound;
            labelSoundRating.Content = oneAnime.rating.sound;

            progressBarGermanDubRating.Value = oneAnime.rating.germanDub;
            labelGermanDubRating.Content = oneAnime.rating.germanDub;

            calculateAndPrintGeneralRating();
        }

        private void calculateAndPrintGeneralRating() {
            int ratingGeneral = 0;
            if (oneAnime.rating.germanDub == 0) {
                ratingGeneral = (int)((oneAnime.rating.story * 0.4) + (oneAnime.rating.sound * 0.2)
                    + (oneAnime.rating.animation * 0.3) + (oneAnime.rating.specialEffect * 0.1));
            }
            else {
                ratingGeneral = (int)((oneAnime.rating.story * 0.4) + (oneAnime.rating.sound * 0.1)
                    + (oneAnime.rating.animation * 0.3) + (oneAnime.rating.specialEffect * 0.1)
                    + (oneAnime.rating.germanDub * 0.1));
            }
            labelGeneralRating.Content = ratingGeneral;
            progressBarGeneralRating.Value = ratingGeneral;
        }

        private void showStaffeln() {

            textBlockForStaffelPrint.Text = "";
            textBoxTotalEpisodes.Text = "";

            if (oneAnime.staffeln != null) {
                for (int i = 0; i < oneAnime.staffeln.Count; i += 1) {
                    if (i != (oneAnime.staffeln.Count - 1)) {
                        textBlockForStaffelPrint.Text += "St." + (i + 1) + " : " + oneAnime.staffeln[i].episodes + " | ";
                    }
                    else {
                        textBlockForStaffelPrint.Text += "St." + (i + 1) + " : " + oneAnime.staffeln[i].episodes;
                    }
                }

                textBoxTotalEpisodes.Text = oneAnime.episodesTotal.ToString();
            }
        }

        private void checkIfAllOriginal() {
            buttonAcceptChanges.Visibility = Visibility.Hidden;
            //if (oneAnime == changedData) {
            //    buttonAcceptChanges.Visibility = Visibility.Hidden;
            //}
        }

        private void toggleFavorite(object sender, RoutedEventArgs e) {
            buttonAcceptChanges.Visibility = Visibility.Visible;

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

            if (oneAnime.staffeln != changedData.staffeln) {
                buttonAcceptChanges.Visibility = Visibility.Visible;
            }

            checkIfAllOriginal();

            showStaffeln();
        }

        private void textChangeName(object sender, TextChangedEventArgs e) {
            if (oneAnime.name != textBoxName.Text) {
                buttonAcceptChanges.Visibility = Visibility.Visible;
                if (changedData.name != textBoxName.Text) {
                    changedData.name = textBoxName.Text;
                }
            }

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