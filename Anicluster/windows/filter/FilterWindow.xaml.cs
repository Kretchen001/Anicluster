using BiboAnime.databaseLiteDB;
using BiboAnime.datatypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Anicluster.windows.filter {
    /// <summary>
    /// Interaktionslogik für FilterWindow.xaml
    /// </summary>
    public partial class FilterWindow : Window {

        public bool favoriteBool = false;
        public bool noFavoriteBool = false;

        private bool ratingGeneralBool = false;
        private bool ratingStoryBool = false;
        private bool ratingAnimationBool = false;
        private bool ratingSpecialEffectsBool = false;
        private bool ratingSoundBool = false;
        private bool ratingGermanDubBool = false;

        private bool existGermanDub = false;

        private bool isShiftPressed = false;

        private bool animeTierS = false;
        private bool animeTierA = false;
        private bool animeTierB = false;
        private bool animeTierC = false;
        private bool animeTierD = false;
        private bool animeTierDummy = false;

        private bool maxEpisodeBool = false;
        private bool minEpisodeBool = false;

        private bool withoutGermanDub = false;

        private int ratingGeneral = 0;
        private int ratingStory = 0;
        private int ratingAnimation = 0;
        private int ratingSpecialEffects = 0;
        private int ratingSound = 0;
        private int ratingGermanDub = 0;

        private int minEpisodes = 0;
        private int maxEpisodes = 0;

        private databaseController dbController = new databaseController();
        private databaseFilterController dbFilterController = new databaseFilterController();
        private List<AnimeData> animeList = new List<AnimeData>();
        private List<AnimeTag> animeTags = new List<AnimeTag>();
        private List<TagUserControll> tagUserControllList = new List<TagUserControll>();

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
            updateFilterData();
        }

        private void toggleStoryRating(object sender, RoutedEventArgs e) {
            ratingStoryBool = !ratingStoryBool;
            ratingGeneralBool = false;
            checkBoxGeneralRating.IsChecked = ratingGeneralBool;
            updateFilterData();
        }

        private void toggleSoundRating(object sender, RoutedEventArgs e) {
            ratingSoundBool = !ratingSoundBool;
            ratingGeneralBool = false;
            checkBoxGeneralRating.IsChecked = ratingGeneralBool;
            updateFilterData();
        }

        private void toggleAnimationRating(object sender, RoutedEventArgs e) {
            ratingAnimationBool = !ratingAnimationBool;
            ratingGeneralBool = false;
            checkBoxGeneralRating.IsChecked = ratingGeneralBool;
            updateFilterData();
        }

        private void toggleSpecialEffectsRating(object sender, RoutedEventArgs e) {
            ratingSpecialEffectsBool = !ratingSpecialEffectsBool;
            ratingGeneralBool = false;
            checkBoxGeneralRating.IsChecked = ratingGeneralBool;
            updateFilterData();
        }

        private void toggleGermanDubRating(object sender, RoutedEventArgs e) {
            ratingGermanDubBool = !ratingGermanDubBool;
            ratingGeneralBool = false;
            checkBoxGeneralRating.IsChecked = ratingGeneralBool;
            updateFilterData();
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

        public void updateFilterData() {
            // favorite
            List<AnimeData> tempListFavorite = new List<AnimeData>();
            if (favoriteBool) {
                tempListFavorite = dbFilterController.dbFilterIsFav(true);
            }
            else if (noFavoriteBool) {
                tempListFavorite = dbFilterController.dbFilterIsFav(false);
            }

            // tier
            List<AnimeData> tempListTier = new List<AnimeData>();
            if (animeTierS) {
                tempListTier.AddRange(dbFilterController.dbFilterTier(AnimeTier.S));
            }
            if (animeTierA) {
                tempListTier.AddRange(dbFilterController.dbFilterTier(AnimeTier.A));
            }
            if (animeTierB) {
                tempListTier.AddRange(dbFilterController.dbFilterTier(AnimeTier.B));
            }
            if (animeTierC) {
                tempListTier.AddRange(dbFilterController.dbFilterTier(AnimeTier.C));
            }
            if (animeTierD) {
                tempListTier.AddRange(dbFilterController.dbFilterTier(AnimeTier.D));
            }
            if (animeTierDummy) {
                tempListTier.AddRange(dbFilterController.dbFilterTier(AnimeTier.Dummy));
            }

            // check if anime has german dub
            List<AnimeData> tempListDubCheck = new List<AnimeData>();
            if (existGermanDub) {
                tempListDubCheck = dbFilterController.dbFilterRatingCheckGermanDub(true);
            }
            if (withoutGermanDub) {
                tempListDubCheck = dbFilterController.dbFilterRatingCheckGermanDub(false);
            }

            // Rating
            List<AnimeData> tempListRating = new List<AnimeData>();

            if (ratingGeneralBool) {
                tempListRating.AddRange(dbFilterController.dbFilterRatingMinABCDE(minGeneral: ratingGeneral));
            }
            if (ratingStoryBool) {
                tempListRating.AddRange(dbFilterController.dbFilterRatingMinABCDE(minStory: ratingStory));
            }
            if (ratingAnimationBool) {
                tempListRating.AddRange(dbFilterController.dbFilterRatingMinABCDE(minAnimation: ratingAnimation));
            }
            if (ratingSoundBool) {
                tempListRating.AddRange(dbFilterController.dbFilterRatingMinABCDE(minSound: ratingSound));
            }
            if (ratingSpecialEffectsBool) {
                tempListRating.AddRange(dbFilterController.dbFilterRatingMinABCDE(minSpecialEffect: ratingSpecialEffects));
            }
            if (ratingGermanDubBool) {
                tempListRating.AddRange(dbFilterController.dbFilterRatingMinABCDE(minGeneral: ratingGermanDub));
            }

            // Tag-Selection
            List<AnimeTag> tagListInclusive = new List<AnimeTag>();
            List<AnimeTag> tagListExclusive = new List<AnimeTag>();
            for (int i = 0; i < animeTags.Count; i += 1) {
                if (tagUserControllList[i].tagSelection.inclusive) {
                    tagListInclusive.Add(new AnimeTag() {
                        id = tagUserControllList[i].tagSelection.id,
                        tagDesignator = tagUserControllList[i].tagSelection.tagDesignator
                    });
                }
                if (tagUserControllList[i].tagSelection.exclusive) {
                    tagListExclusive.Add(new AnimeTag() {
                        id = tagUserControllList[i].tagSelection.id,
                        tagDesignator = tagUserControllList[i].tagSelection.tagDesignator
                    });
                }
            }

            List<AnimeData> listOfTagSelection = new List<AnimeData>();
            listOfTagSelection.AddRange(dbFilterController.dbFilterByTag(tagListInclusive));
            listOfTagSelection.AddRange(dbFilterController.dbFilterWithoutTag(tagListExclusive));

            // Min Max Episodes
            List<AnimeData> minMaxList = new List<AnimeData>();

            if (maxEpisodeBool && !minEpisodeBool) {
                minMaxList = dbFilterController.dbFilterMinMaxEpisodesTotal(true, maxEpisodes);
            }
            if (minEpisodeBool && !maxEpisodeBool) {
                minMaxList = dbFilterController.dbFilterMinMaxEpisodesTotal(false, minEpisodes);
            }
            if (maxEpisodeBool && minEpisodeBool) {
                if (!(maxEpisodes <= minEpisodes)) {
                    List<AnimeData> maxList = dbFilterController.dbFilterMinMaxEpisodesTotal(true, maxEpisodes);
                    List<AnimeData> minList = dbFilterController.dbFilterMinMaxEpisodesTotal(false, minEpisodes);
                    minMaxList = minList.Where(x => maxList.Contains(x)).ToList();
                }
            }

            // merge Lists
            animeList.Clear();
            if (tempListFavorite.Count != 0) {
                animeList = tempListFavorite;
            }
            if ((tempListTier.Count != 0) && (animeList.Count == 0)) {
                animeList.AddRange(tempListTier);
            }
            else if (tempListTier.Count != 0) {
                animeList = tempListTier.Where(x => animeList.Contains(x)).ToList();
            }
            if ((tempListRating.Count != 0) && (animeList.Count == 0)) {
                animeList.AddRange(tempListRating);
            }
            else if (tempListRating.Count != 0) {
                animeList = tempListRating.Where(x => animeList.Contains(x)).ToList();
            }
            if ((listOfTagSelection.Count != 0) && (animeList.Count == 0)) {
                animeList.AddRange(listOfTagSelection);
            }
            else if (listOfTagSelection.Count != 0) {
                animeList = listOfTagSelection.Where(x => animeList.Contains(x)).ToList();
            }
            if ((minMaxList.Count != 0) && (animeList.Count == 0)) {
                animeList.AddRange(minMaxList);
            }
            else if (minMaxList.Count != 0) {
                animeList = minMaxList.Where(x => animeList.Contains(x)).ToList();
            }

            // remove all duplicated Animes in the List
            animeList = animeList.Distinct(new ItemEqualityComparer()).ToList();

            if (animeList.Count == 0) {
                animeList = dbController.getAllAnimes();
            }

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

        private void toggleTierDummy(object sender, RoutedEventArgs e) {
            animeTierDummy = !animeTierDummy;

            updateFilterData();
        }

        private void fillTagView() {
            animeTags = dbController.getAllAnimeTags();

            for (int i = 0; i < animeTags.Count; i += 1) {
                TagUserControll tempControll = new TagUserControll(new TagSelection(animeTags[i]));
                tagUserControllList.Add(tempControll);
                treeViewItemTags.Items.Add(tempControll);
            }
        }

        private void loadingRowFilterDataGrid(object sender, DataGridRowEventArgs e) {
            try {
                AnimeRow tempRow = (AnimeRow)e.Row.DataContext;
                switch (dbController.getAnimeById(tempRow.id).tier) {
                    case AnimeTier.S: e.Row.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF00D4D4")); break;
                    case AnimeTier.A: e.Row.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF3FFF2F")); break;
                    case AnimeTier.B: e.Row.Background = new SolidColorBrush(Colors.Yellow); break;
                    case AnimeTier.C: e.Row.Background = new SolidColorBrush(Colors.Orange); break;
                    case AnimeTier.D: e.Row.Background = new SolidColorBrush(Colors.Red); break;
                    case AnimeTier.Dummy: e.Row.Background = new SolidColorBrush(Colors.Silver); break;
                    default: break;
                }
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message);
                //Log?
            }
        }

        private void clickFavoriteYes(object sender, RoutedEventArgs e) {
            checkBoxFavoriteNo.IsChecked = false;
            favoriteBool = !favoriteBool;
            if (favoriteBool) {
                noFavoriteBool = false;
            }
            checkBoxFavoriteYes.IsChecked = favoriteBool;

            updateFilterData();
        }

        private void clickFavoriteNo(object sender, RoutedEventArgs e) {
            checkBoxFavoriteYes.IsChecked = false;
            noFavoriteBool = !noFavoriteBool;
            if (noFavoriteBool) {
                favoriteBool = false;
            }
            checkBoxFavoriteNo.IsChecked = noFavoriteBool;

            updateFilterData();
        }

        private void toggleWithoutDub(object sender, RoutedEventArgs e) {
            withoutGermanDub = !withoutGermanDub;
            if (existGermanDub) {
                existGermanDub = false;
                checkBoxWithDub.IsChecked = false;
            }
            if (ratingGermanDubBool) {
                ratingGermanDubBool = false;
                checkBoxGermanDub.IsChecked = false;
            }
            updateFilterData();
        }

        private void toggleWithDub(object sender, RoutedEventArgs e) {
            existGermanDub = !existGermanDub;
            if (withoutGermanDub) {
                withoutGermanDub = false;
                checkBoxWithoutDub.IsChecked = false;
            }
            updateFilterData();
        }

        private void resetButtonClick(object sender, RoutedEventArgs e) {
            animeList.Clear();
            animeList = dbController.getAllAnimes();
            updateFilterTable();

            // properties
            favoriteBool = false;
            noFavoriteBool = false;

            ratingGeneralBool = false;
            ratingStoryBool = false;
            ratingAnimationBool = false;
            ratingSpecialEffectsBool = false;
            ratingSoundBool = false;
            ratingGermanDubBool = false;

            existGermanDub = false;
            withoutGermanDub = false;

            ratingGeneral = 0;
            ratingStory = 0;
            ratingAnimation = 0;
            ratingSpecialEffects = 0;
            ratingSound = 0;
            ratingGermanDub = 0;

            isShiftPressed = false;

            animeTierS = false;
            animeTierA = false;
            animeTierB = false;
            animeTierC = false;
            animeTierD = false;
            animeTierDummy = false;

            // overlay
            checkBoxFavoriteYes.IsChecked = false;
            checkBoxFavoriteNo.IsChecked = false;
            checkBoxTierS.IsChecked = false;
            checkBoxTierA.IsChecked = false;
            checkBoxTierB.IsChecked = false;
            checkBoxTierC.IsChecked = false;
            checkBoxTierD.IsChecked = false;
            checkBoxTierDummy.IsChecked = false;
            checkBoxGeneralRating.IsChecked = false;
            progressBarGeneralRating.Value = 0;
            labelGeneralRating.Content = 0;
            checkBoxStoryRating.IsChecked = false;
            progressBarStoryRating.Value = 0; 
            labelStoryRating.Content = 0;
            checkBoxSoundRating.IsChecked = false;
            progressBarSoundRating.Value = 0;
            labelSoundRating.Content = 0;
            checkBoxAnimationRating.IsChecked = false;
            progressBarAnimationRating.Value = 0;
            labelAnimationRating.Content = 0;
            checkBoxSpecialEffectsRating.IsChecked = false;
            progressBarSpecialEffectsRating.Value = 0;
            labelSpecialEffectsRating.Content = 0;
            checkBoxGermanDub.IsChecked = false;
            progressBarGermanDubRating.Value = 0;
            labelGermanDubRating.Content = 0;
            checkBoxWithDub.IsChecked = false;
            checkBoxWithoutDub.IsChecked = false;
            // expanded
            treeViewItemTags.IsExpanded = false;
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e) {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void clickMaxEpisodes(object sender, RoutedEventArgs e) {
            maxEpisodeBool = !maxEpisodeBool;

            updateFilterData();
        }

        private void clickMinEpisodes(object sender, RoutedEventArgs e) {
            minEpisodeBool = !minEpisodeBool;

            updateFilterData();
        }

        private void textChangeEpisodes(object sender, TextChangedEventArgs e) {
            if (textBoxEpisodesMax.Text == "") {
                textBoxEpisodesMax.Text = "0";
            }
            if (textBoxEpisodesMin.Text == "") {
                textBoxEpisodesMin.Text = "0";
            }
            maxEpisodes = int.Parse(textBoxEpisodesMax.Text);
            minEpisodes = int.Parse(textBoxEpisodesMin.Text);

            updateFilterData();
        }
    }
}