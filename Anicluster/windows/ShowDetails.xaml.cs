using BiboAnime.databaseLiteDB;
using BiboAnime.datatypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Anicluster.windows {

    /// <summary>
    /// Interaktionslogik für ShowDetails.xaml
    /// </summary>
    public partial class ShowDetails : Window {

        public AnimeData oneAnime = new AnimeData();
        private AnimeData changedData = new AnimeData();

        public Action<bool> shouldViewUpdated;

        DatabaseController dbController = new DatabaseController();

        private bool isShiftPressed = false;

        public const int MaxInt32Value = 2147483647;

        public ShowDetails(AnimeData givenAnime) {
            InitializeComponent();

            CenterWindowOnScreen();
            
            AddHandler(Keyboard.KeyDownEvent, (KeyEventHandler)HandleKeyDownEvent);
            AddHandler(Keyboard.KeyUpEvent, (KeyEventHandler)HandleKeyUpEvent);

            oneAnime = givenAnime.DeepClone();
            changedData = givenAnime.DeepClone();

            setDisplay();
            showTier();
            showRating();
            showStaffeln();
            showTagsInDataGrid();
            labelStatus.Content = oneAnime.Status.ToString();
            labelNameHeadLine.Content = givenAnime.Name;

            checkIfAllOriginal();
        }

        private void CenterWindowOnScreen() {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            double windowWidth = this.Width;
            double windowHeight = this.Height;
            if ((windowHeight > screenHeight) || (windowWidth > screenWidth)) {
                WindowState = WindowState.Maximized;
            }
            else {
                this.Left = (screenWidth / 2) - (windowWidth / 2);
                this.Top = (screenHeight / 2) - (windowHeight / 2);
            }
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

        private void setDisplay() {
            textBoxName.Text = changedData.Name;
            textBoxOriginalName.Text = changedData.OriginalName;
            if (changedData.Favorite) {
                checkBoxFavorit.IsChecked = true;
                checkBoxNotFavorit.IsChecked = false;
            }
            else {
                checkBoxFavorit.IsChecked = false;
                checkBoxNotFavorit.IsChecked = true;
            }
            textBoxUrlAnimePlanet.Text = changedData.UrlAnimePlanet;
            textBoxRecommendation.Text = changedData.ThirdPartyRecommendation;
        }

        private void showTier() {
            int S = 0, A = 0, B = 0, C = 0, D = 0;
            switch (changedData.Tier) {
                case AnimeTier.S: S = 2; break;
                case AnimeTier.A: A = 2; break;
                case AnimeTier.B: B = 2; break;
                case AnimeTier.C: C = 2; break;
                case AnimeTier.D: D = 2; break;
                case AnimeTier.Dummy: {
                        S = 0;
                        A = 0; 
                        B = 0;
                        C = 0;
                        D = 0;
                        break;
                    }
                default: break;
            }
            labelTierS.BorderThickness = new Thickness(S);
            labelTierA.BorderThickness = new Thickness(A);
            labelTierB.BorderThickness = new Thickness(B);
            labelTierC.BorderThickness = new Thickness(C);
            labelTierD.BorderThickness = new Thickness(D);
        }

        private void showRating() {

            progressBarStoryRating.Value = changedData.Rating.Story;
            labelStoryRating.Content = changedData.Rating.Story;

            progressBarAnimationRating.Value = changedData.Rating.Animation;
            labelAnimationRating.Content = changedData.Rating.Animation;

            progressBarSpecialEffectsRating.Value = changedData.Rating.SpecialEffect;
            labelSpecialEffectsRating.Content = changedData.Rating.SpecialEffect;

            progressBarSoundRating.Value = changedData.Rating.Sound;
            labelSoundRating.Content = changedData.Rating.Sound;

            progressBarGermanDubRating.Value = changedData.Rating.GermanDub;
            labelGermanDubRating.Content = changedData.Rating.GermanDub;

            calculateAndPrintGeneralRating();
        }

        private void calculateAndPrintGeneralRating() {
            int ratingGeneral = 0;
            if (changedData.Rating.GermanDub == 0) {
                ratingGeneral = (int)((changedData.Rating.Story * 0.4) + (changedData.Rating.Sound * 0.2)
                    + (changedData.Rating.Animation * 0.3) + (changedData.Rating.SpecialEffect * 0.1));
            }
            else {
                ratingGeneral = (int)((changedData.Rating.Story * 0.4) + (changedData.Rating.Sound * 0.1)
                    + (changedData.Rating.Animation * 0.3) + (changedData.Rating.SpecialEffect * 0.1)
                    + (changedData.Rating.GermanDub * 0.1));
            }
            labelGeneralRating.Content = ratingGeneral;
            progressBarGeneralRating.Value = ratingGeneral;

            checkIfAllOriginal();
        }

        private void showStaffeln() {

            textBlockForStaffelPrint.Text = "";
            textBoxTotalEpisodes.Text = "";

            if (changedData.Staffeln != null) {
                int x = 0;
                for (int i = 0; i < changedData.Staffeln.Count; i += 1) {
                    x += changedData.Staffeln[i].Episodes;
                    if (i != (changedData.Staffeln.Count - 1)) {
                        textBlockForStaffelPrint.Text += "St." + (i + 1) + " : " + changedData.Staffeln[i].Episodes + " | ";
                    }
                    else {
                        textBlockForStaffelPrint.Text += "St." + (i + 1) + " : " + changedData.Staffeln[i].Episodes;
                    }
                }

                changedData.EpisodesTotal = x;
                textBoxTotalEpisodes.Text = changedData.EpisodesTotal.ToString();
            }

            textBoxMovies.Text = changedData.Movies.ToString();
            textBoxOvhs.Text = changedData.Ovh.ToString();
        }

        private void showTagsInDataGrid() {

            List<string> tagNames = new List<string>();
            for (int i = 0; i < changedData.Tags.Count; i += 1) {
                tagNames.Add(changedData.Tags[i].TagDesignator);
            }
            tagNames.Sort();

            dataGridTagsShowDetails.ItemsSource = null;
            dataGridTagsShowDetails.ItemsSource = tagNames;
        }

        private void clickCallLink(object sender, RoutedEventArgs e) {
            Process.Start(new ProcessStartInfo {
                FileName = oneAnime.UrlAnimePlanet,
                UseShellExecute = true
            });
        }

        private void clickCloseButton(object sender, RoutedEventArgs e) {
            if (buttonAcceptChanges.Visibility == Visibility.Visible) {
                if (MessageBox.Show("Es wurden Änderungen vorgenommen.\n" +
                    "Beenden um sie zu verwerfen?",
                    "Änderungen verwerfen?",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning) == MessageBoxResult.Yes) {
                    this.Close();
                }
            }
            else {
                this.Close();
            }
        }

        private void clickAcceptChanges(object sender, RoutedEventArgs e) {
            DatabaseController.updateAnime(changedData);
            shouldViewUpdated(true);
            buttonAcceptChanges.Visibility = Visibility.Hidden;
            labelStatus.Content = changedData.Status.ToString();
        }

        private void clickRemoveEntity(object sender, RoutedEventArgs e) {
            var yesNo = MessageBox.Show("Möchten Sie " + oneAnime.Name + " wirklich löschen?",
                "Löschen",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (yesNo == MessageBoxResult.Yes) {
                bool del = DatabaseController.DeleteAnimeFromDB(oneAnime);
                if (del) {
                    MessageBox.Show(oneAnime.Name + " gelöscht!",
                        "Gelöscht",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    shouldViewUpdated(true);
                    this.Close();
                }
            }
        }

        private void clickChoosenTags(object sender, RoutedEventArgs e) {

            TagList tagListWindow = new TagList(changedData.Tags);
            tagListWindow.ShowDialog();

            changedData.Tags = tagListWindow.selectedTagList;

            checkIfAllOriginal();

            showTagsInDataGrid();
        }

        private void checkIfAllOriginal() {
            if (oneAnime.Name != changedData.Name) {
                buttonAcceptChanges.Visibility = Visibility.Visible;
                return;
            }
            if (oneAnime.OriginalName != changedData.OriginalName) {
                if ((oneAnime.OriginalName is null) && (changedData.OriginalName != "")) {
                    buttonAcceptChanges.Visibility = Visibility.Visible;
                    return;
                }
                if (oneAnime.OriginalName is not null) {
                    buttonAcceptChanges.Visibility = Visibility.Visible;
                    return;
                }
            }
            if (oneAnime.Favorite != changedData.Favorite) {
                buttonAcceptChanges.Visibility = Visibility.Visible;
                return;
            }
            if (!oneAnime.Rating.Equals(oneAnime.Rating, changedData.Rating)) {
                buttonAcceptChanges.Visibility = Visibility.Visible;
                return;
            }
            AnimeTag tempTag = new AnimeTag();
            if (!tempTag.EqualsList(oneAnime.Tags, changedData.Tags)) {
                buttonAcceptChanges.Visibility = Visibility.Visible;
                return;
            }
            if (oneAnime.UrlAnimePlanet != changedData.UrlAnimePlanet) {
                if ((oneAnime.UrlAnimePlanet is null) && (changedData.UrlAnimePlanet != "")) {
                    buttonAcceptChanges.Visibility = Visibility.Visible;
                    return;
                }
            }
            if (oneAnime.Status != changedData.Status) {
                buttonAcceptChanges.Visibility = Visibility.Visible;
                return;
            }
            Staffel tempStaffel = new Staffel();
            if (!tempStaffel.EqualsList(oneAnime.Staffeln, changedData.Staffeln)) {
                buttonAcceptChanges.Visibility = Visibility.Visible;
                return;
            }
            // Not USE because of calculating-order
            //if (oneAnime.episodesTotal != changedData.episodesTotal) {
            //    buttonAcceptChanges.Visibility = Visibility.Visible;
            //    return;
            //}
            if (oneAnime.Movies != changedData.Movies) {
                buttonAcceptChanges.Visibility = Visibility.Visible;
                return;
            }
            if (oneAnime.Ovh != changedData.Ovh) {
                buttonAcceptChanges.Visibility = Visibility.Visible;
                return;
            }
            if (oneAnime.ThirdPartyRecommendation != changedData.ThirdPartyRecommendation) {
                buttonAcceptChanges.Visibility = Visibility.Visible;
                return;
            }
            if (oneAnime.Tier != changedData.Tier) {
                buttonAcceptChanges.Visibility = Visibility.Visible;
                return;
            }
            if (oneAnime.Movies != changedData.Movies) {
                buttonAcceptChanges.Visibility = Visibility.Visible;
                return;
            }
            if (oneAnime.Ovh != changedData.Ovh) {
                buttonAcceptChanges.Visibility = Visibility.Visible;
                return;
            }

            buttonAcceptChanges.Visibility = Visibility.Hidden;
        }

        private void toggleFavorite(object sender, RoutedEventArgs e) {
            changedData.Favorite = !changedData.Favorite;

            if (changedData.Favorite) {
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

            StaffelManager staffelManagerWindow = new StaffelManager(changedData.Staffeln); // changed, so any multi-Changes will be recognised
            staffelManagerWindow.ShowDialog();

            changedData.Staffeln = staffelManagerWindow.getManagerStaffeln();

            checkIfAllOriginal();

            showStaffeln();
        }

        private void textChangeName(object sender, TextChangedEventArgs e) {
            changedData.Name = textBoxName.Text;

            checkIfAllOriginal();
        }

        private void textChangeOriginalName(object sender, TextChangedEventArgs e) {
            changedData.OriginalName = textBoxOriginalName.Text;

            checkIfAllOriginal();
        }

        private void textChangeUrl(object sender, TextChangedEventArgs e) {
            changedData.UrlAnimePlanet = textBoxUrlAnimePlanet.Text;

            checkIfAllOriginal();
        }

        private void textChangeRecomendation(object sender, TextChangedEventArgs e) {
            changedData.ThirdPartyRecommendation = textBoxRecommendation.Text;

            checkIfAllOriginal();
        }

        private void textChangedOvh(object sender, TextChangedEventArgs e) {
            if (textBoxOvhs.Text.Length != 0) {
                try {
                    changedData.Ovh = int.Parse(textBoxOvhs.Text);
                }
                catch (Exception ex) {
                    textBoxOvhs.Text = textBoxOvhs.Text.Replace(" ", "");
                    Console.WriteLine(ex.ToString());
                }
            }
            checkIfAllOriginal();
        }

        private void textChangedMovies(object sender, TextChangedEventArgs e) {
            if (textBoxMovies.Text.Length != 0) {
                try {
                    changedData.Movies = int.Parse(textBoxMovies.Text);
                }
                catch (Exception ex) {
                    textBoxMovies.Text = textBoxMovies.Text.Replace(" ", "");
                    Console.WriteLine(ex.ToString());
                }
            }

            checkIfAllOriginal();
        }

        private void clickChangeTierS(object sender, MouseButtonEventArgs e) {
            if (changedData.Tier != AnimeTier.S) {
                changedData.Tier = AnimeTier.S;
            }
            else {
                changedData.Tier = AnimeTier.Dummy;
            }

            checkIfAllOriginal();
            showTier();
        }

        private void clickChangeTierA(object sender, MouseButtonEventArgs e) {
            if (changedData.Tier != AnimeTier.A) {
                changedData.Tier = AnimeTier.A;
            }
            else {
                changedData.Tier = AnimeTier.Dummy;
            }

            checkIfAllOriginal();
            showTier();
        }

        private void clickChangeTierB(object sender, MouseButtonEventArgs e) {
            if (changedData.Tier != AnimeTier.B) {
                changedData.Tier = AnimeTier.B;
            }
            else {
                changedData.Tier = AnimeTier.Dummy;
            }

            checkIfAllOriginal();
            showTier();
        }

        private void clickChangeTierC(object sender, MouseButtonEventArgs e) {
            if (changedData.Tier != AnimeTier.C) {
                changedData.Tier = AnimeTier.C;
            }
            else {
                changedData.Tier = AnimeTier.Dummy;
            }

            checkIfAllOriginal();
            showTier();
        }

        private void clickChangeTierD(object sender, MouseButtonEventArgs e) {
            if (changedData.Tier != AnimeTier.D) {
                changedData.Tier = AnimeTier.D;
            }
            else {
                changedData.Tier = AnimeTier.Dummy;
            }

            checkIfAllOriginal();
            showTier();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e) {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        // Rating-Code ----------------------------------------------------------------------------
        private void ratingStoryIncDec(int e) {
            if ((e > 0) && (changedData.Rating.Story <= 100)) {
                if (changedData.Rating.Story != 100) {
                    if (isShiftPressed && changedData.Rating.Story < 90) {
                        changedData.Rating.Story += 10;
                    }
                    else if (isShiftPressed && changedData.Rating.Story >= 90) {
                        changedData.Rating.Story = 100;
                    }
                    else {
                        changedData.Rating.Story += 1;
                    }
                }
            }
            else {
                if (changedData.Rating.Story > 0) {
                    if (isShiftPressed && changedData.Rating.Story > 10) {
                        changedData.Rating.Story -= 10;
                    }
                    else if (isShiftPressed && changedData.Rating.Story <= 10) {
                        changedData.Rating.Story = 0;
                    }
                    else {
                        changedData.Rating.Story -= 1;
                    }
                }
            }
            labelStoryRating.Content = changedData.Rating.Story.ToString();
            progressBarStoryRating.Value = changedData.Rating.Story;
            // generalRating recalculate
            calculateAndPrintGeneralRating();
        }

        private void ratingAnimationIncDec(int e) {
            if ((e > 0) && (changedData.Rating.Animation <= 100)) {
                if (changedData.Rating.Animation != 100) {
                    if (isShiftPressed && changedData.Rating.Animation < 90) {
                        changedData.Rating.Animation += 10;
                    }
                    else if (isShiftPressed && changedData.Rating.Animation >= 90) {
                        changedData.Rating.Animation = 100;
                    }
                    else {
                        changedData.Rating.Animation += 1;
                    }
                }
            }
            else {
                if (changedData.Rating.Animation > 0) {
                    if (isShiftPressed && changedData.Rating.Animation > 10) {
                        changedData.Rating.Animation -= 10;
                    }
                    else if (isShiftPressed && changedData.Rating.Animation <= 10) {
                        changedData.Rating.Animation = 0;
                    }
                    else {
                        changedData.Rating.Animation -= 1;
                    }
                }
            }
            labelAnimationRating.Content = changedData.Rating.Animation.ToString();
            progressBarAnimationRating.Value = changedData.Rating.Animation;
            // generalRating recalculate
            calculateAndPrintGeneralRating();
        }

        private void ratingSpecialEffectsIncDec(int e) {
            if ((e > 0) && (changedData.Rating.SpecialEffect <= 100)) {
                if (changedData.Rating.SpecialEffect != 100) {
                    if (isShiftPressed && changedData.Rating.SpecialEffect < 90) {
                        changedData.Rating.SpecialEffect += 10;
                    }
                    else if (isShiftPressed && changedData.Rating.SpecialEffect >= 90) {
                        changedData.Rating.SpecialEffect = 100;
                    }
                    else {
                        changedData.Rating.SpecialEffect += 1;
                    }
                }
            }
            else {
                if (changedData.Rating.SpecialEffect > 0) {
                    if (isShiftPressed && changedData.Rating.SpecialEffect > 10) {
                        changedData.Rating.SpecialEffect -= 10;
                    }
                    else if (isShiftPressed && changedData.Rating.SpecialEffect <= 10) {
                        changedData.Rating.SpecialEffect = 0;
                    }
                    else {
                        changedData.Rating.SpecialEffect -= 1;
                    }
                }
            }
            labelSpecialEffectsRating.Content = changedData.Rating.SpecialEffect.ToString();
            progressBarSpecialEffectsRating.Value = changedData.Rating.SpecialEffect;
            // generalRating recalculate
            calculateAndPrintGeneralRating();
        }

        private void ratingSoundIncDec(int e) {
            if ((e > 0) && (changedData.Rating.Sound <= 100)) {
                if (changedData.Rating.Sound != 100) {
                    if (isShiftPressed && changedData.Rating.Sound < 90) {
                        changedData.Rating.Sound += 10;
                    }
                    else if (isShiftPressed && changedData.Rating.Sound >= 90) {
                        changedData.Rating.Sound = 100;
                    }
                    else {
                        changedData.Rating.Sound += 1;
                    }
                }
            }
            else {
                if (changedData.Rating.Sound > 0) {
                    if (isShiftPressed && changedData.Rating.Sound > 10) {
                        changedData.Rating.Sound -= 10;
                    }
                    else if (isShiftPressed && changedData.Rating.Sound <= 10) {
                        changedData.Rating.Sound = 0;
                    }
                    else {
                        changedData.Rating.Sound -= 1;
                    }
                }
            }
            labelSoundRating.Content = changedData.Rating.Sound.ToString();
            progressBarSoundRating.Value = changedData.Rating.Sound;
            // generalRating recalculate
            calculateAndPrintGeneralRating();
        }

        private void ratingGermanDubIncDec(int e) {
            if ((e > 0) && (changedData.Rating.GermanDub <= 100)) {
                if (changedData.Rating.GermanDub != 100) {
                    if (isShiftPressed && changedData.Rating.GermanDub < 90) {
                        changedData.Rating.GermanDub += 10;
                    }
                    else if (isShiftPressed && changedData.Rating.GermanDub >= 90) {
                        changedData.Rating.GermanDub = 100;
                    }
                    else {
                        changedData.Rating.GermanDub += 1;
                    }
                }
            }
            else {
                if (changedData.Rating.GermanDub > 0) {
                    if (isShiftPressed && changedData.Rating.GermanDub > 10) {
                        changedData.Rating.GermanDub -= 10;
                    }
                    else if (isShiftPressed && changedData.Rating.GermanDub <= 10) {
                        changedData.Rating.GermanDub = 0;
                    }
                    else {
                        changedData.Rating.GermanDub -= 1;
                    }
                }
            }
            labelGermanDubRating.Content = changedData.Rating.GermanDub.ToString();
            progressBarGermanDubRating.Value = changedData.Rating.GermanDub;
            // generalRating recalculate
            calculateAndPrintGeneralRating();
        }

        private void click_RatingStoryPlus(object sender, RoutedEventArgs e) {
            ratingStoryIncDec(1);
        }

        private void click_RatingStoryMinus(object sender, RoutedEventArgs e) {
            ratingStoryIncDec(-1);
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

        private void comboBoxStatusChanged(object sender, SelectionChangedEventArgs e) {
            if (comboBoxStatus.SelectedItem != null) {
                ComboBoxItem cbi = (ComboBoxItem)comboBoxStatus.SelectedItem;
                switch (cbi.Content.ToString()) {
                    case "Wunschliste": changedData.Status = AnimeStatus.Wunschliste; break;
                    case "Angefangen": changedData.Status = AnimeStatus.Angefangen; break;
                    case "Fertig": changedData.Status = AnimeStatus.Fertig; break;
                    case "Unterbrochen": changedData.Status = AnimeStatus.Unterbrochen; break;
                    case "Abgebrochen": changedData.Status = AnimeStatus.Abgebrochen; break;
                    default: break;
                }

                checkIfAllOriginal();
            }
        }
    }
}