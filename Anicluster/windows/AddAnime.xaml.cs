using BiboAnime.databaseLiteDB;
using BiboAnime.datatypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Anicluster.windows {
    /// <summary>
    /// Interaktionslogik für AddAnime.xaml
    /// </summary>
    public partial class AddAnime : Window {

        private int ratingGeneral = 0;
        private int ratingStory = 0;
        private int ratingAnimation = 0;
        private int ratingSpecialEffects = 0;
        private int ratingSound = 0;
        private int ratingGermanDub = 0;
        private List<Staffel> animeStaffeln = new List<Staffel>();
        private int animeTotalEpisodes = 0;
        private int animeMovies = 0;
        private int animeOvh = 0;

        private string animeName = "";
        private string originalName = "";
        private string urlAnimePlanet = "";
        private string animeRecomendation = "";

        private AnimeStatus animeStatus = AnimeStatus.Wunschliste;

        private bool favorite = false;

        private AnimeTier animeTier = AnimeTier.Dummy;

        private List<AnimeTag> animeTagList = new List<AnimeTag>();

        private bool isShiftPressed = false;

        public Action<bool> shouldViewUpdated;

        public AddAnime() {
            InitializeComponent();

            AddHandler(Keyboard.KeyDownEvent, (KeyEventHandler)HandleKeyDownEvent);
            AddHandler(Keyboard.KeyUpEvent, (KeyEventHandler)HandleKeyUpEvent);

            initLabel();
            initCheckBox();
        }

        private void HandleKeyDownEvent(object sender, KeyEventArgs e) {
            if ((e.Key == Key.LeftShift) && (isShiftPressed == false)) {
                isShiftPressed = true;
            }
        }

        private void HandleKeyUpEvent(object sender, KeyEventArgs e) {
            if ((e.Key == Key.LeftShift) && (isShiftPressed == true)) {
                isShiftPressed = false;
            }
        }

        private void initLabel() {
            labelGeneralRating.Content = ratingGeneral.ToString();
            labelStoryRating.Content = ratingStory.ToString();
            labelAnimationRating.Content = ratingAnimation.ToString();
            labelSpecialEffectsRating.Content = ratingSpecialEffects.ToString();
            labelSoundRating.Content = ratingSound.ToString();
            labelGermanDubRating.Content = ratingGermanDub.ToString();
        }

        private void initCheckBox() {
            checkBoxStatus0.Content = AnimeStatus.Wunschliste;
            checkBoxStatus1.Content = AnimeStatus.Angefangen;
            checkBoxStatus2.Content = AnimeStatus.Fertig;
            checkBoxStatus3.Content = AnimeStatus.Unterbrochen;
            checkBoxStatus4.Content = AnimeStatus.Abgebrochen;
        }

        private void mouseWheelSrollStory(object sender, MouseWheelEventArgs e) {
            ratingStoryIncDec(e.Delta);
        }

        private void mouseWheelScrollAnimation(object sender, MouseWheelEventArgs e) {
            ratingAnimationIncDec(e.Delta);
        }

        private void mouseWheelScrollSpecialEffects(object sender, MouseWheelEventArgs e) {
            ratingSpecialEffectsIncDec(e.Delta);
        }

        private void mouseWheelScrollSound(object sender, MouseWheelEventArgs e) {
            ratingSoundIncDec(e.Delta);
        }

        private void mouseWheelScrollGermanDub(object sender, MouseWheelEventArgs e) {
            ratingGermanDubIncDec(e.Delta);
        }

        private void calculateAndPrintGeneralRating() {
            if (ratingGermanDub == 0) {
                ratingGeneral = (int)((ratingStory * 0.4) + (ratingSound * 0.2)
                    + (ratingAnimation * 0.3) + (ratingSpecialEffects * 0.1));
            }
            else {
                ratingGeneral = (int)((ratingStory * 0.4) + (ratingSound * 0.1)
                    + (ratingAnimation * 0.3) + (ratingSpecialEffects * 0.1)
                    + (ratingGermanDub * 0.1));
            }
            labelGeneralRating.Content = ratingGeneral;
            progressBarGeneralRating.Value = ratingGeneral;
        }

        private void checkBoxStatus0_Checked(object sender, RoutedEventArgs e) {
            boxCheckChange(true, false, false, false, false);
            animeStatus = AnimeStatus.Wunschliste;
        }

        private void checkBoxStatus1_Checked(object sender, RoutedEventArgs e) {
            boxCheckChange(false, true, false, false, false);
            animeStatus = AnimeStatus.Angefangen;
        }

        private void checkBoxStatus2_Checked(object sender, RoutedEventArgs e) {
            boxCheckChange(false, false, true, false, false);
            animeStatus = AnimeStatus.Fertig;
        }

        private void checkBoxStatus3_Checked(object sender, RoutedEventArgs e) {
            boxCheckChange(false, false, false, true, false);
            animeStatus = AnimeStatus.Unterbrochen;
        }

        private void checkBoxStatus4_Checked(object sender, RoutedEventArgs e) {
            boxCheckChange(false, false, false, false, true);
            animeStatus = AnimeStatus.Abgebrochen;
        }

        private void boxCheckChange(bool zero, bool one, bool two, bool three, bool four) {
            checkBoxStatus0.IsChecked = zero;
            checkBoxStatus1.IsChecked = one;
            checkBoxStatus2.IsChecked = two;
            checkBoxStatus3.IsChecked = three;
            checkBoxStatus4.IsChecked = four;
        }

        private void checkBoxFavorit_Checked(object sender, RoutedEventArgs e) {
            if (favorite == false) {
                checkBoxFavorit.IsChecked = true;
                checkBoxNotFavorit.IsChecked = false;
                favorite = true;
            }
            else {
                checkBoxFavorit.IsChecked = false;
                checkBoxNotFavorit.IsChecked = true;
                favorite = false;
            }
        }
        private void checkBoxFavorit_NotChecked(object sender, RoutedEventArgs e) {
            if (favorite == true) {
                checkBoxFavorit.IsChecked = false;
                checkBoxNotFavorit.IsChecked = true;
                favorite = false;
            }
            else {
                checkBoxFavorit.IsChecked = true;
                checkBoxNotFavorit.IsChecked = false;
                favorite = true;
            }
        }

        private void textChangeName(object sender, TextChangedEventArgs e) {
            animeName = textBoxName.Text;
        }

        private void textChangeOriginalName(object sender, TextChangedEventArgs e) {
            originalName = textBoxOriginalName.Text;
        }

        private void textChangeUrlTextBox(object sender, TextChangedEventArgs e) {
            urlAnimePlanet = textBoxUrlAnimePlanet.Text;
        }

        private void textChangeRecommendation(object sender, RoutedEventArgs e) {
            animeRecomendation = textBoxRecommendation.Text;
        }

        private void click_ConfirmButton(object sender, RoutedEventArgs e) {
            databaseController dbController = new databaseController();

            AnimeData? temp = null;
            // the obligated data
            if (animeName != "" && ratingStory != 0 && ratingSound != 0 && ratingAnimation != 0 && ratingSpecialEffects != 0 && animeTagList.Count != 0) {
                temp = new AnimeData {
                    id = Guid.NewGuid(),
                    name = animeName,
                    favorite = favorite,
                    status = animeStatus,
                    rating = new Rating(ratingStory, ratingSound, ratingAnimation, ratingSpecialEffects, ratingGermanDub),
                    tags = animeTagList,
                };
            }
            else {
                MessageBox.Show("Es fehlen obligatorische Daten", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return; // -> end of function
            }

            // optional data added
            if (originalName != "") {
                temp.originalName = originalName;
            }
            if (animeRecomendation != "") {
                temp.thirdPartyRecommendation = animeRecomendation;
            }
            if (urlAnimePlanet != "") {
                temp.urlAnimePlanet = urlAnimePlanet;
            }
            if (animeStaffeln.Count > 0) {
                temp.staffeln = animeStaffeln;
            }
            if (animeTotalEpisodes != 0) {
                temp.episodesTotal = animeTotalEpisodes;
            }
            if (animeOvh != 0) {
                temp.ovh = animeOvh;
            }
            if (animeMovies != 0) {
                temp.movies = animeMovies;
            }
            if (animeTier != AnimeTier.Dummy) {
                temp.tier = animeTier;
            }
            else {
                temp.tier = AnimeTier.Dummy;
            }

            if (!dbController.checkIfAnimeExist(temp.name)) {
                dbController.addAnimeToDB(temp);
                shouldViewUpdated(true);
                this.Close();
            }
            else {
                MessageBox.Show("Name schon vorhanden!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void click_LinkAutoGenerate(object sender, RoutedEventArgs e) {
            if (animeName != "") {
                string autoLink = "https://www.anime-planet.com/anime/";
                autoLink += animeNameConvert();

                textBoxUrlAnimePlanet.Text = autoLink;
                //urlAnimePlanet = autoLink;
            }
            else {
                MessageBox.Show("Kein Name, der Konvertiert werden kann!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string animeNameConvert() {
            return animeName.ToLower().Replace(' ', '-');
        }

        private void click_CallLink(object sender, RoutedEventArgs e) {
            if (urlAnimePlanet.Length >= 6) {
                if (urlAnimePlanet[0..5] == "https:") {
                    Process.Start(new ProcessStartInfo {
                        FileName = urlAnimePlanet,
                        UseShellExecute = true
                    });
                }
            }
        }

        private void click_RatingStoryPlus(object sender, RoutedEventArgs e) {
            ratingStoryIncDec(1);
        }

        private void click_RatingStoryMinus(object sender, RoutedEventArgs e) {
            ratingStoryIncDec(-1);
        }

        private void ratingStoryIncDec(int e) {
            if ((e > 0) && (ratingStory <= 100)) {
                if (ratingStory != 100) {
                    if (isShiftPressed && ratingStory < 90) {
                        ratingStory += 10;
                    }
                    else if (isShiftPressed && ratingStory >= 90) {
                        ratingStory = 100;
                    }
                    else {
                        ratingStory += 1;
                    }
                }
            }
            else {
                if (ratingStory > 0) {
                    if (isShiftPressed && ratingStory > 10) {
                        ratingStory -= 10;
                    }
                    else if (isShiftPressed && ratingStory <= 10) {
                        ratingStory = 0;
                    }
                    else {
                        ratingStory -= 1;
                    }
                }
            }
            labelStoryRating.Content = ratingStory.ToString();
            progressBarStoryRating.Value = ratingStory;
            // generalRating recalculate
            calculateAndPrintGeneralRating();
        }

        private void ratingAnimationIncDec(int e) {
            if ((e > 0) && (ratingAnimation <= 100)) {
                if (ratingAnimation != 100) {
                    if (isShiftPressed && ratingAnimation < 90) {
                        ratingAnimation += 10;
                    }
                    else if (isShiftPressed && ratingAnimation >= 90) {
                        ratingAnimation = 100;
                    }
                    else {
                        ratingAnimation += 1;
                    }
                }
            }
            else {
                if (ratingAnimation > 0) {
                    if (isShiftPressed && ratingAnimation > 10) {
                        ratingAnimation -= 10;
                    }
                    else if (isShiftPressed && ratingAnimation <= 10) {
                        ratingAnimation = 0;
                    }
                    else {
                        ratingAnimation -= 1;
                    }
                }
            }
            labelAnimationRating.Content = ratingAnimation.ToString();
            progressBarAnimationRating.Value = ratingAnimation;
            // generalRating recalculate
            calculateAndPrintGeneralRating();
        }

        private void ratingSpecialEffectsIncDec(int e) {
            if ((e > 0) && (ratingSpecialEffects <= 100)) {
                if (ratingSpecialEffects != 100) {
                    if (isShiftPressed && ratingSpecialEffects < 90) {
                        ratingSpecialEffects += 10;
                    }
                    else if (isShiftPressed && ratingSpecialEffects >= 90) {
                        ratingSpecialEffects = 100;
                    }
                    else {
                        ratingSpecialEffects += 1;
                    }
                }
            }
            else {
                if (ratingSpecialEffects > 0) {
                    if (isShiftPressed && ratingSpecialEffects > 10) {
                        ratingSpecialEffects -= 10;
                    }
                    else if (isShiftPressed && ratingSpecialEffects <= 10) {
                        ratingSpecialEffects = 0;
                    }
                    else {
                        ratingSpecialEffects -= 1;
                    }
                }
            }
            labelSpecialEffectsRating.Content = ratingSpecialEffects.ToString();
            progressBarSpecialEffectsRating.Value = ratingSpecialEffects;
            // generalRating recalculate
            calculateAndPrintGeneralRating();
        }

        private void ratingSoundIncDec(int e) {
            if ((e > 0) && (ratingSound <= 100)) {
                if (ratingSound != 100) {
                    if (isShiftPressed && ratingSound < 90) {
                        ratingSound += 10;
                    }
                    else if (isShiftPressed && ratingSound >= 90) {
                        ratingSound = 100;
                    }
                    else {
                        ratingSound += 1;
                    }
                }
            }
            else {
                if (ratingSound > 0) {
                    if (isShiftPressed && ratingSound > 10) {
                        ratingSound -= 10;
                    }
                    else if (isShiftPressed && ratingSound <= 10) {
                        ratingSound = 0;
                    }
                    else {
                        ratingSound -= 1;
                    }
                }
            }
            labelSoundRating.Content = ratingSound.ToString();
            progressBarSoundRating.Value = ratingSound;
            // generalRating recalculate
            calculateAndPrintGeneralRating();
        }

        private void ratingGermanDubIncDec(int e) {
            if ((e > 0) && (ratingGermanDub <= 100)) {
                if (ratingGermanDub != 100) {
                    if (isShiftPressed && ratingGermanDub < 90) {
                        ratingGermanDub += 10;
                    }
                    else if (isShiftPressed && ratingGermanDub >= 90) {
                        ratingGermanDub = 100;
                    }
                    else {
                        ratingGermanDub += 1;
                    }
                }
            }
            else {
                if (ratingGermanDub > 0) {
                    if (isShiftPressed && ratingGermanDub > 10) {
                        ratingGermanDub -= 10;
                    }
                    else if (isShiftPressed && ratingGermanDub <= 10) {
                        ratingGermanDub = 0;
                    }
                    else {
                        ratingGermanDub -= 1;
                    }
                }
            }
            labelGermanDubRating.Content = ratingGermanDub.ToString();
            progressBarGermanDubRating.Value = ratingGermanDub;
            // generalRating recalculate
            calculateAndPrintGeneralRating();
        }

        private void click_RatingAnimationPlus(object sender, RoutedEventArgs e) {
            ratingAnimationIncDec(1);
        }

        private void click_RatingAnimationMinus(object sender, RoutedEventArgs e) {
            ratingAnimationIncDec(-1);
        }

        private void click_RatingSpecialEffectsPlus(object sender, RoutedEventArgs e) {
            ratingSpecialEffectsIncDec(1);
        }

        private void click_RatingSpecialEffectsMinus(object sender, RoutedEventArgs e) {
            ratingSpecialEffectsIncDec(-1);
        }

        private void click_RatingSoundPlus(object sender, RoutedEventArgs e) {
            ratingSoundIncDec(1);
        }

        private void click_RatingSoundMinus(object sender, RoutedEventArgs e) {
            ratingSoundIncDec(-1);
        }

        private void click_RatingGermanDubPlus(object sender, RoutedEventArgs e) {
            ratingGermanDubIncDec(1);
        }

        private void click_RatingGermanDubMinus(object sender, RoutedEventArgs e) {
            ratingGermanDubIncDec(-1);
        }

        private void click_RatingGermanDubMinus(object sender, MouseButtonEventArgs e) {
            ratingGermanDubIncDec(-1);
        }

        private void click_MoviesPlus(object sender, RoutedEventArgs e) {
            animeMovies += 1;
            textBoxMovies.Text = animeMovies.ToString();
        }

        private void click_MoviesMinus(object sender, RoutedEventArgs e) {
            if (animeMovies != 0) {
                animeMovies -= 1;
                textBoxMovies.Text = animeMovies.ToString();
            }
        }

        private void click_OvhsPlus(object sender, RoutedEventArgs e) {
            animeOvh += 1;
            textBoxOvhs.Text = animeOvh.ToString();
        }

        private void click_OvhsMinus(object sender, RoutedEventArgs e) {
            if (animeOvh != 0) {
                animeOvh -= 1;
                textBoxOvhs.Text = animeOvh.ToString();
            }
        }

        private void clickTierS(object sender, MouseButtonEventArgs e) {
            if (animeTier == AnimeTier.S) {
                labelTierS.BorderThickness = new Thickness(0);
                animeTier = AnimeTier.Dummy;
            }
            else {
                tierBorderThicknessChange(S: 2, A: 0, B: 0, C: 0, D: 0);
                animeTier = AnimeTier.S;
            }
        }

        private void clickTierA(object sender, MouseButtonEventArgs e) {
            if (animeTier == AnimeTier.A) {
                labelTierA.BorderThickness = new Thickness(0);
                animeTier = AnimeTier.Dummy;
            }
            else {
                tierBorderThicknessChange(S: 0, A: 2, B: 0, C: 0, D: 0);
                animeTier = AnimeTier.A;
            }
        }

        private void clickTierB(object sender, MouseButtonEventArgs e) {
            if (animeTier == AnimeTier.B) {
                labelTierB.BorderThickness = new Thickness(0);
                animeTier = AnimeTier.Dummy;
            }
            else {
                tierBorderThicknessChange(S: 0, A: 0, B: 2, C: 0, D: 0);
                animeTier = AnimeTier.B;
            }
        }

        private void clickTierC(object sender, MouseButtonEventArgs e) {
            if (animeTier == AnimeTier.C) {
                labelTierC.BorderThickness = new Thickness(0);
                animeTier = AnimeTier.Dummy;
            }
            else {
                tierBorderThicknessChange(S: 0, A: 0, B: 0, C: 2, D: 0);
                animeTier = AnimeTier.C;
            }
        }

        private void clickTierD(object sender, MouseButtonEventArgs e) {
            if (animeTier == AnimeTier.D) {
                labelTierD.BorderThickness = new Thickness(0);
                animeTier = AnimeTier.Dummy;
            }
            else {
                tierBorderThicknessChange(S: 0, A: 0, B: 0, C: 0, D: 2);
                animeTier = AnimeTier.D;
            }
        }

        private void tierBorderThicknessChange(int S, int A, int B, int C, int D) {
            labelTierS.BorderThickness = new Thickness(S);
            labelTierA.BorderThickness = new Thickness(A);
            labelTierB.BorderThickness = new Thickness(B);
            labelTierC.BorderThickness = new Thickness(C);
            labelTierD.BorderThickness = new Thickness(D);
        }

        private void clickChooseTags(object sender, RoutedEventArgs e) {
            TagList tagListWindow = new TagList(animeTagList);
            tagListWindow.ShowDialog();
            animeTagList = tagListWindow.selectedTagList;
        }

        private void clickStaffelManagement(object sender, RoutedEventArgs e) {
            StaffelManager staffelManagerWindow = new StaffelManager(animeStaffeln);
            staffelManagerWindow.ShowDialog();

            animeStaffeln = staffelManagerWindow.managerStaffeln;

            // show the Manager-Solution
            textBlockForStaffelPrint.Text = "";
            for (int i = 0; i < animeStaffeln.Count; i += 1) {
                if (i != (animeStaffeln.Count - 1)) {
                    textBlockForStaffelPrint.Text += "St." + (animeStaffeln[i].counter) + " : " + animeStaffeln[i].episodes + " | ";
                }
                else {
                    textBlockForStaffelPrint.Text += "St." + (animeStaffeln[i].counter) + " : " + animeStaffeln[i].episodes;
                }
            }

            // calculate and print total Episodes
            int z = 0;
            for (int i = 0; i < animeStaffeln.Count; i += 1) {
                z += animeStaffeln[i].episodes;
            }
            animeTotalEpisodes = z;
            textBoxTotalEpisodes.Text = z.ToString();
        }
    }
}