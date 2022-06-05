using BiboAnime.datatypes;
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
        private int animeStaffeln = 0;
        private int animeTotalEpisodes = 0;

        private string animeName = "";
        private string originalName = "";
        private string urlAnimePlanet = "";

        private AnimeStatus animeStatus = AnimeStatus.Wunschliste;

        private bool favorite = false;

        public AddAnime() {
            InitializeComponent();

            initLabel();
            initCheckBox();
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
                    + (ratingAnimation * 0.3) + (ratingSpecialEffects* 0.1));
            }
            else {
                ratingGeneral = (int)((ratingStory * 0.4) + (ratingSound * 0.1) 
                    + (ratingAnimation * 0.3) + (ratingSpecialEffects * 0.1) 
                    + (ratingGermanDub* 0.1));
            }
            labelGeneralRating.Content = ratingGeneral;
            progressBarGeneralRating.Value = ratingGeneral;
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

        private void checkBoxStatus0_Checked(object sender, RoutedEventArgs e) {
            checkBoxStatus0.IsChecked = true;
            checkBoxStatus1.IsChecked = false;
            checkBoxStatus2.IsChecked = false;
            checkBoxStatus3.IsChecked = false;
            checkBoxStatus4.IsChecked = false;
            animeStatus = AnimeStatus.Wunschliste;
        }

        private void checkBoxStatus1_Checked(object sender, RoutedEventArgs e) {
            checkBoxStatus0.IsChecked = false;
            checkBoxStatus1.IsChecked = true;
            checkBoxStatus2.IsChecked = false;
            checkBoxStatus3.IsChecked = false;
            checkBoxStatus4.IsChecked = false;
            animeStatus = AnimeStatus.Angefangen;
        }

        private void checkBoxStatus2_Checked(object sender, RoutedEventArgs e) {
            checkBoxStatus0.IsChecked = false;
            checkBoxStatus1.IsChecked = false;
            checkBoxStatus2.IsChecked = true;
            checkBoxStatus3.IsChecked = false;
            checkBoxStatus4.IsChecked = false;
            animeStatus = AnimeStatus.Fertig;
        }

        private void checkBoxStatus3_Checked(object sender, RoutedEventArgs e) {
            checkBoxStatus0.IsChecked = false;
            checkBoxStatus1.IsChecked = false;
            checkBoxStatus2.IsChecked = false;
            checkBoxStatus3.IsChecked = true;
            checkBoxStatus4.IsChecked = false;
            animeStatus = AnimeStatus.Unterbrochen;
        }

        private void checkBoxStatus4_Checked(object sender, RoutedEventArgs e) {
            checkBoxStatus0.IsChecked = false;
            checkBoxStatus1.IsChecked = false;
            checkBoxStatus2.IsChecked = false;
            checkBoxStatus3.IsChecked = false;
            checkBoxStatus4.IsChecked = true;
            animeStatus = AnimeStatus.Abgebrochen;
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

        private void click_ConfirmButton(object sender, RoutedEventArgs e) {
            // the obligated data
            if (animeName != "" && originalName != "" && ratingStory != 0 && ratingSound != 0 && ratingAnimation != 0 && ratingSpecialEffects != 0) {
                AnimeData temp = new AnimeData {
                    id = data.listOfAnimes.Count,
                    name = animeName,
                    originalName = originalName,
                    favorite = favorite,
                    status = animeStatus,
                    rating = new Rating(ratingStory, ratingSound, ratingAnimation, ratingSpecialEffects, ratingGermanDub)
                };
            }
            else {
                MessageBox.Show("Es fehlen obligatorische Daten", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            return animeName.ToLower().Replace(' ','-');
        }

        private void click_CallLink(object sender, RoutedEventArgs e) {
            Process.Start(new ProcessStartInfo {
                FileName = urlAnimePlanet,
                UseShellExecute = true
            });
        }

        private void click_StaffelnPlus(object sender, RoutedEventArgs e) {
            animeStaffeln += 1;
            textBoxStaffeln.Text = animeStaffeln.ToString();
        }

        private void click_TotalEpisodesPlus(object sender, RoutedEventArgs e) {
            animeTotalEpisodes += 1;
            textBoxTotalEpisodes.Text = animeTotalEpisodes.ToString();
        }

        private void click_StaffelnMinus(object sender, RoutedEventArgs e) {
            animeStaffeln -= 1;
            textBoxStaffeln.Text = animeStaffeln.ToString();
        }

        private void click_TotalEpisodesMinus(object sender, RoutedEventArgs e) {
            animeTotalEpisodes -= 1;
        }

        private void textChange_Staffeln(object sender, TextChangedEventArgs e) {
            animeStaffeln = int.Parse(textBoxStaffeln.Text);
        }

        private void textChange_TotalEpisodes(object sender, TextChangedEventArgs e) {
            animeTotalEpisodes = int.Parse(textBoxTotalEpisodes.Text);
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
                    ratingStory += 1;
                }
            }
            else {
                if (ratingStory >= 0) {
                    if (ratingStory != 0) {
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
                    ratingAnimation += 1;
                }
            }
            else {
                if (ratingAnimation >= 0) {
                    if (ratingAnimation != 0) {
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
                    ratingSpecialEffects += 1;
                }
            }
            else {
                if (ratingSpecialEffects >= 0) {
                    if (ratingSpecialEffects != 0) {
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
                    ratingSound += 1;
                }
            }
            else {
                if (ratingSound >= 0) {
                    if (ratingSound != 0) {
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
                    ratingGermanDub += 1;
                }
            }
            else {
                if (ratingGermanDub >= 0) {
                    if (ratingGermanDub != 0) {
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
    }
}