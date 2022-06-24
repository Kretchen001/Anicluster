using BiboAnime.databaseLiteDB;
using BiboAnime.datatypes;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace Anicluster.windows.filter {
    /// <summary>
    /// Interaktionslogik für FilterWindow.xaml
    /// </summary>
    public partial class FilterWindow : Window {

        public bool favoriteBool = false;

        private bool ratingGeneralBool = true;
        private bool ratingStoryBool = false;
        private bool ratingAnimationBool = false;
        private bool ratingSpecialEffectsBool = false;
        private bool ratingSoundBool = false;
        private bool ratingGermanDubBool = false;

        private int ratingGeneral = 0;
        private int ratingStory = 0;
        private int ratingAnimation = 0;
        private int ratingSpecialEffects = 0;
        private int ratingSound = 0;
        private int ratingGermanDub = 0;

        private bool isShiftPressed = false;

        private bool animeTierS = false;
        private bool animeTierA = false;
        private bool animeTierB = false;
        private bool animeTierC = false;
        private bool animeTierD = false;

        private databaseController dbController = new databaseController();
        private databaseFilterController dbFilterController = new databaseFilterController();
        private List<AnimeData> animeList = new List<AnimeData>();
        private List<AnimeTag> animeTags = new List<AnimeTag>();

        public FilterWindow() {
            InitializeComponent();

            AddHandler(Keyboard.KeyDownEvent, (KeyEventHandler)HandleKeyDownEvent);
            AddHandler(Keyboard.KeyUpEvent, (KeyEventHandler)HandleKeyUpEvent);

            fillTagView();

            animeList = dbController.getAllAnimes();
            updateFilterTable();
        }

        private void updateFilterTable() {
            dataGridFilterAnimeList.ItemsSource = null;
            dataGridFilterAnimeList.ItemsSource = rowCrafter(animeList);
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

        private void click_ShowDetails(object sender, RoutedEventArgs e) {
            // get the id per clicked Details-Button
            int currentRowIndex = dataGridFilterAnimeList.Items.IndexOf(dataGridFilterAnimeList.CurrentItem); // begin by 0. Give the rowNumber | NOT THE ID
            if ((currentRowIndex != (dataGridFilterAnimeList.Items.Count)) || (currentRowIndex == 0)) {
                AnimeRow selectedAnime = (AnimeRow)dataGridFilterAnimeList.Items[currentRowIndex];

                // give the anime to the ShowDetails-Window
                ShowDetails showDetailsScreen = new ShowDetails(dbController.getAnimeById(selectedAnime.id));
                showDetailsScreen.Show();
            }
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

        private void clickRatingStoryPlus(object sender, RoutedEventArgs e) {
            ratingStoryIncDec(1);
        }

        private void clickRatingStoryMinus(object sender, RoutedEventArgs e) {
            ratingStoryIncDec(-1);
        }

        private void clickRatingAnimationPlus(object sender, RoutedEventArgs e) {
            ratingAnimationIncDec(1);
        }

        private void clickRatingAnimationMinus(object sender, RoutedEventArgs e) {
            ratingAnimationIncDec(-1);
        }

        private void clickRatingSpecialEffectsPlus(object sender, RoutedEventArgs e) {
            ratingSpecialEffectsIncDec(1);
        }

        private void clickRatingSpecialEffectsMinus(object sender, RoutedEventArgs e) {
            ratingSpecialEffectsIncDec(-1);
        }

        private void clickRatingSoundPlus(object sender, RoutedEventArgs e) {
            ratingSoundIncDec(1);
        }

        private void clickRatingSoundMinus(object sender, RoutedEventArgs e) {
            ratingSoundIncDec(-1);
        }

        private void clickRatingGermanDubPlus(object sender, RoutedEventArgs e) {
            ratingGermanDubIncDec(1);
        }

        private void clickRatingGermanDubMinus(object sender, RoutedEventArgs e) {
            ratingGermanDubIncDec(-1);
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

        private void toggleFavorite(object sender, RoutedEventArgs e) {
            favoriteBool = !favoriteBool;

            updateFilterData();
        }

        private void toggleGeneralRating(object sender, RoutedEventArgs e) {
            ratingGeneralBool = !ratingGeneralBool;
            ratingStoryBool = false;
            checkBoxStoryRating.IsChecked = ratingStoryBool;
            ratingSoundBool = false;
            checkBoxSoundRating.IsChecked = ratingSoundBool;
            ratingAnimationBool = false;
            checkBoxAnimationRating.IsChecked = ratingAnimationBool;
            ratingSpecialEffectsBool = false;
            checkBoxSpecialEffectsRating.IsChecked = ratingSpecialEffectsBool;
            ratingGermanDubBool = false;
            checkBoxGermanDub.IsChecked = ratingGermanDubBool;
        }

        private void toggleStoryRating(object sender, RoutedEventArgs e) {
            ratingStoryBool = !ratingStoryBool;
            ratingGeneralBool = false;
            checkBoxGeneralRating.IsChecked = ratingGeneralBool;
        }

        private void toggleSoundRating(object sender, RoutedEventArgs e) {
            ratingSoundBool = !ratingSoundBool;
            ratingGeneralBool = false;
            checkBoxGeneralRating.IsChecked = ratingGeneralBool;
        }

        private void toggleAnimationRating(object sender, RoutedEventArgs e) {
            ratingAnimationBool = !ratingAnimationBool;
            ratingGeneralBool = false;
            checkBoxGeneralRating.IsChecked = ratingGeneralBool;
        }

        private void toggleSpecialEffectsRating(object sender, RoutedEventArgs e) {
            ratingSpecialEffectsBool = !ratingSpecialEffectsBool;
            ratingGeneralBool = false;
            checkBoxGeneralRating.IsChecked = ratingGeneralBool;
        }

        private void toggleGermanDubRating(object sender, RoutedEventArgs e) {
            ratingGermanDubBool = !ratingGermanDubBool;
            ratingGeneralBool = false;
            checkBoxGeneralRating.IsChecked = ratingGeneralBool;
        }

        private List<AnimeRow> rowCrafter(List<AnimeData> oList) {
            List<AnimeRow> rows = new List<AnimeRow>();
            for (int i = 0; i < oList.Count; i += 1) {
                string tagString = "";
                for (int j = 0; j < oList[i].tags.Count; j += 1) {
                    if (j != (oList[i].tags.Count - 1)) {
                        tagString += oList[i].tags[j].tagDesignator + ", ";
                    }
                    else {
                        tagString += oList[i].tags[j].tagDesignator;
                    }
                }
                rows.Add(new AnimeRow {
                    id = oList[i].id,
                    favorite = oList[i].favorite,
                    name = oList[i].name,
                    status = oList[i].status,
                    tags = tagString,
                    generalRating = oList[i].rating.general,
                });
            }
            return rows;
        }

        private void updateFilterData() {
            // favorite
            if (favoriteBool) {
                animeList.Clear();
                animeList = dbFilterController.dbFilterIsFav(true);
                updateFilterTable();
            }
            else {
                animeList.Clear();
                animeList = dbController.getAllAnimes();
                updateFilterTable();
            }

            // tier
            List<AnimeRow> tempList = new List<AnimeRow>();

            if (animeTierS) {
                tempList.AddRange(rowCrafter(dbFilterController.dbFilterTier(AnimeTier.S)));
            }
            if (animeTierA) {
                tempList.AddRange(rowCrafter(dbFilterController.dbFilterTier(AnimeTier.A)));
            }
            if (animeTierB) {
                tempList.AddRange(rowCrafter(dbFilterController.dbFilterTier(AnimeTier.B)));
            }
            if (animeTierC) {
                tempList.AddRange(rowCrafter(dbFilterController.dbFilterTier(AnimeTier.C)));
            }
            if (animeTierD) {
                tempList.AddRange(rowCrafter(dbFilterController.dbFilterTier(AnimeTier.D)));
            }

            for (int i = 0; i < tempList.Count; i += 1) {
                for (int j = 0; j < animeList.Count; j += 1) {
                    if (tempList[i].id == animeList[j].id) {
                        animeList.RemoveAt(j);
                    }
                }
            }

            // Rating

            updateFilterTable();
        }

        private void toggleTierS(object sender, RoutedEventArgs e) {
            animeTierS = !animeTierS;

            updateFilterData();
        }

        private void toggleTierA(object sender, RoutedEventArgs e) {
            animeTierA = !animeTierA;

            updateFilterData();
        }

        private void toggleTierB(object sender, RoutedEventArgs e) {
            animeTierB = !animeTierB;

            updateFilterData();
        }

        private void toggleTierC(object sender, RoutedEventArgs e) {
            animeTierC = !animeTierC;

            updateFilterData();
        }

        private void toggleTierD(object sender, RoutedEventArgs e) {
            animeTierD = !animeTierD;

            updateFilterData();
        }

        private void fillTagView() {
            animeTags = dbController.getAllAnimeTags();
            
            for (int i = 1; i < animeTags.Count; i += 1) {
                treeViewItemTags.Items.Add(new TagUserControll(new TagSelection(animeTags[i])));
            }
        }
    }
}