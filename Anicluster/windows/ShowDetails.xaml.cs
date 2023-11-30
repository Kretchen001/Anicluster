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

        databaseController dbController = new databaseController();

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
            labelStatus.Content = oneAnime.status.ToString();
            labelNameHeadLine.Content = givenAnime.name;

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

            checkIfAllOriginal();
        }

        private void showStaffeln() {

            textBlockForStaffelPrint.Text = "";
            textBoxTotalEpisodes.Text = "";

            if (changedData.staffeln != null) {
                int x = 0;
                for (int i = 0; i < changedData.staffeln.Count; i += 1) {
                    x += changedData.staffeln[i].episodes;
                    if (i != (changedData.staffeln.Count - 1)) {
                        textBlockForStaffelPrint.Text += "St." + (i + 1) + " : " + changedData.staffeln[i].episodes + " | ";
                    }
                    else {
                        textBlockForStaffelPrint.Text += "St." + (i + 1) + " : " + changedData.staffeln[i].episodes;
                    }
                }

                changedData.episodesTotal = x;
                textBoxTotalEpisodes.Text = changedData.episodesTotal.ToString();
            }

            textBoxMovies.Text = changedData.movies.ToString();
            textBoxOvhs.Text = changedData.ovh.ToString();
        }

        private void showTagsInDataGrid() {

            List<string> tagNames = new List<string>();
            for (int i = 0; i < changedData.tags.Count; i += 1) {
                tagNames.Add(changedData.tags[i].tagDesignator);
            }
            tagNames.Sort();

            dataGridTagsShowDetails.ItemsSource = null;
            dataGridTagsShowDetails.ItemsSource = tagNames;
        }

        private void clickCallLink(object sender, RoutedEventArgs e) {
            Process.Start(new ProcessStartInfo {
                FileName = oneAnime.urlAnimePlanet,
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
            dbController.updateAnime(changedData);
            shouldViewUpdated(true);
            buttonAcceptChanges.Visibility = Visibility.Hidden;
            labelStatus.Content = changedData.status.ToString();
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

            TagList tagListWindow = new TagList(changedData.tags);
            tagListWindow.ShowDialog();

            changedData.tags = tagListWindow.selectedTagList;

            checkIfAllOriginal();

            showTagsInDataGrid();
        }

        private void checkIfAllOriginal() {
            if (oneAnime.name != changedData.name) {
                buttonAcceptChanges.Visibility = Visibility.Visible;
                return;
            }
            if (oneAnime.originalName != changedData.originalName) {
                if ((oneAnime.originalName is null) && (changedData.originalName != "")) {
                    buttonAcceptChanges.Visibility = Visibility.Visible;
                    return;
                }
                if (oneAnime.originalName is not null) {
                    buttonAcceptChanges.Visibility = Visibility.Visible;
                    return;
                }
            }
            if (oneAnime.favorite != changedData.favorite) {
                buttonAcceptChanges.Visibility = Visibility.Visible;
                return;
            }
            if (!oneAnime.rating.Equals(oneAnime.rating, changedData.rating)) {
                buttonAcceptChanges.Visibility = Visibility.Visible;
                return;
            }
            AnimeTag tempTag = new AnimeTag();
            if (!tempTag.EqualsList(oneAnime.tags, changedData.tags)) {
                buttonAcceptChanges.Visibility = Visibility.Visible;
                return;
            }
            if (oneAnime.urlAnimePlanet != changedData.urlAnimePlanet) {
                if ((oneAnime.urlAnimePlanet is null) && (changedData.urlAnimePlanet != "")) {
                    buttonAcceptChanges.Visibility = Visibility.Visible;
                    return;
                }
            }
            if (oneAnime.status != changedData.status) {
                buttonAcceptChanges.Visibility = Visibility.Visible;
                return;
            }
            Staffel tempStaffel = new Staffel();
            if (!tempStaffel.EqualsList(oneAnime.staffeln, changedData.staffeln)) {
                buttonAcceptChanges.Visibility = Visibility.Visible;
                return;
            }
            // Not USE because of calculating-order
            //if (oneAnime.episodesTotal != changedData.episodesTotal) {
            //    buttonAcceptChanges.Visibility = Visibility.Visible;
            //    return;
            //}
            if (oneAnime.movies != changedData.movies) {
                buttonAcceptChanges.Visibility = Visibility.Visible;
                return;
            }
            if (oneAnime.ovh != changedData.ovh) {
                buttonAcceptChanges.Visibility = Visibility.Visible;
                return;
            }
            if (oneAnime.thirdPartyRecommendation != changedData.thirdPartyRecommendation) {
                buttonAcceptChanges.Visibility = Visibility.Visible;
                return;
            }
            if (oneAnime.tier != changedData.tier) {
                buttonAcceptChanges.Visibility = Visibility.Visible;
                return;
            }
            if (oneAnime.movies != changedData.movies) {
                buttonAcceptChanges.Visibility = Visibility.Visible;
                return;
            }
            if (oneAnime.ovh != changedData.ovh) {
                buttonAcceptChanges.Visibility = Visibility.Visible;
                return;
            }

            buttonAcceptChanges.Visibility = Visibility.Hidden;
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
            changedData.originalName = textBoxOriginalName.Text;

            checkIfAllOriginal();
        }

        private void textChangeUrl(object sender, TextChangedEventArgs e) {
            changedData.urlAnimePlanet = textBoxUrlAnimePlanet.Text;

            checkIfAllOriginal();
        }

        private void textChangeRecomendation(object sender, TextChangedEventArgs e) {
            changedData.thirdPartyRecommendation = textBoxRecommendation.Text;

            checkIfAllOriginal();
        }

        private void textChangedOvh(object sender, TextChangedEventArgs e) {
            if (textBoxOvhs.Text.Length != 0) {
                try {
                    changedData.ovh = int.Parse(textBoxOvhs.Text);
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
                    changedData.movies = int.Parse(textBoxMovies.Text);
                }
                catch (Exception ex) {
                    textBoxMovies.Text = textBoxMovies.Text.Replace(" ", "");
                    Console.WriteLine(ex.ToString());
                }
            }

            checkIfAllOriginal();
        }

        private void clickChangeTierS(object sender, MouseButtonEventArgs e) {
            if (changedData.tier != AnimeTier.S) {
                changedData.tier = AnimeTier.S;
            }
            else {
                changedData.tier = AnimeTier.Dummy;
            }

            checkIfAllOriginal();
            showTier();
        }

        private void clickChangeTierA(object sender, MouseButtonEventArgs e) {
            if (changedData.tier != AnimeTier.A) {
                changedData.tier = AnimeTier.A;
            }
            else {
                changedData.tier = AnimeTier.Dummy;
            }

            checkIfAllOriginal();
            showTier();
        }

        private void clickChangeTierB(object sender, MouseButtonEventArgs e) {
            if (changedData.tier != AnimeTier.B) {
                changedData.tier = AnimeTier.B;
            }
            else {
                changedData.tier = AnimeTier.Dummy;
            }

            checkIfAllOriginal();
            showTier();
        }

        private void clickChangeTierC(object sender, MouseButtonEventArgs e) {
            if (changedData.tier != AnimeTier.C) {
                changedData.tier = AnimeTier.C;
            }
            else {
                changedData.tier = AnimeTier.Dummy;
            }

            checkIfAllOriginal();
            showTier();
        }

        private void clickChangeTierD(object sender, MouseButtonEventArgs e) {
            if (changedData.tier != AnimeTier.D) {
                changedData.tier = AnimeTier.D;
            }
            else {
                changedData.tier = AnimeTier.Dummy;
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
            if ((e > 0) && (changedData.rating.story <= 100)) {
                if (changedData.rating.story != 100) {
                    if (isShiftPressed && changedData.rating.story < 90) {
                        changedData.rating.story += 10;
                    }
                    else if (isShiftPressed && changedData.rating.story >= 90) {
                        changedData.rating.story = 100;
                    }
                    else {
                        changedData.rating.story += 1;
                    }
                }
            }
            else {
                if (changedData.rating.story > 0) {
                    if (isShiftPressed && changedData.rating.story > 10) {
                        changedData.rating.story -= 10;
                    }
                    else if (isShiftPressed && changedData.rating.story <= 10) {
                        changedData.rating.story = 0;
                    }
                    else {
                        changedData.rating.story -= 1;
                    }
                }
            }
            labelStoryRating.Content = changedData.rating.story.ToString();
            progressBarStoryRating.Value = changedData.rating.story;
            // generalRating recalculate
            calculateAndPrintGeneralRating();
        }

        private void ratingAnimationIncDec(int e) {
            if ((e > 0) && (changedData.rating.animation <= 100)) {
                if (changedData.rating.animation != 100) {
                    if (isShiftPressed && changedData.rating.animation < 90) {
                        changedData.rating.animation += 10;
                    }
                    else if (isShiftPressed && changedData.rating.animation >= 90) {
                        changedData.rating.animation = 100;
                    }
                    else {
                        changedData.rating.animation += 1;
                    }
                }
            }
            else {
                if (changedData.rating.animation > 0) {
                    if (isShiftPressed && changedData.rating.animation > 10) {
                        changedData.rating.animation -= 10;
                    }
                    else if (isShiftPressed && changedData.rating.animation <= 10) {
                        changedData.rating.animation = 0;
                    }
                    else {
                        changedData.rating.animation -= 1;
                    }
                }
            }
            labelAnimationRating.Content = changedData.rating.animation.ToString();
            progressBarAnimationRating.Value = changedData.rating.animation;
            // generalRating recalculate
            calculateAndPrintGeneralRating();
        }

        private void ratingSpecialEffectsIncDec(int e) {
            if ((e > 0) && (changedData.rating.specialEffect <= 100)) {
                if (changedData.rating.specialEffect != 100) {
                    if (isShiftPressed && changedData.rating.specialEffect < 90) {
                        changedData.rating.specialEffect += 10;
                    }
                    else if (isShiftPressed && changedData.rating.specialEffect >= 90) {
                        changedData.rating.specialEffect = 100;
                    }
                    else {
                        changedData.rating.specialEffect += 1;
                    }
                }
            }
            else {
                if (changedData.rating.specialEffect > 0) {
                    if (isShiftPressed && changedData.rating.specialEffect > 10) {
                        changedData.rating.specialEffect -= 10;
                    }
                    else if (isShiftPressed && changedData.rating.specialEffect <= 10) {
                        changedData.rating.specialEffect = 0;
                    }
                    else {
                        changedData.rating.specialEffect -= 1;
                    }
                }
            }
            labelSpecialEffectsRating.Content = changedData.rating.specialEffect.ToString();
            progressBarSpecialEffectsRating.Value = changedData.rating.specialEffect;
            // generalRating recalculate
            calculateAndPrintGeneralRating();
        }

        private void ratingSoundIncDec(int e) {
            if ((e > 0) && (changedData.rating.sound <= 100)) {
                if (changedData.rating.sound != 100) {
                    if (isShiftPressed && changedData.rating.sound < 90) {
                        changedData.rating.sound += 10;
                    }
                    else if (isShiftPressed && changedData.rating.sound >= 90) {
                        changedData.rating.sound = 100;
                    }
                    else {
                        changedData.rating.sound += 1;
                    }
                }
            }
            else {
                if (changedData.rating.sound > 0) {
                    if (isShiftPressed && changedData.rating.sound > 10) {
                        changedData.rating.sound -= 10;
                    }
                    else if (isShiftPressed && changedData.rating.sound <= 10) {
                        changedData.rating.sound = 0;
                    }
                    else {
                        changedData.rating.sound -= 1;
                    }
                }
            }
            labelSoundRating.Content = changedData.rating.sound.ToString();
            progressBarSoundRating.Value = changedData.rating.sound;
            // generalRating recalculate
            calculateAndPrintGeneralRating();
        }

        private void ratingGermanDubIncDec(int e) {
            if ((e > 0) && (changedData.rating.germanDub <= 100)) {
                if (changedData.rating.germanDub != 100) {
                    if (isShiftPressed && changedData.rating.germanDub < 90) {
                        changedData.rating.germanDub += 10;
                    }
                    else if (isShiftPressed && changedData.rating.germanDub >= 90) {
                        changedData.rating.germanDub = 100;
                    }
                    else {
                        changedData.rating.germanDub += 1;
                    }
                }
            }
            else {
                if (changedData.rating.germanDub > 0) {
                    if (isShiftPressed && changedData.rating.germanDub > 10) {
                        changedData.rating.germanDub -= 10;
                    }
                    else if (isShiftPressed && changedData.rating.germanDub <= 10) {
                        changedData.rating.germanDub = 0;
                    }
                    else {
                        changedData.rating.germanDub -= 1;
                    }
                }
            }
            labelGermanDubRating.Content = changedData.rating.germanDub.ToString();
            progressBarGermanDubRating.Value = changedData.rating.germanDub;
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
                    case "Wunschliste": changedData.status = AnimeStatus.Wunschliste; break;
                    case "Angefangen": changedData.status = AnimeStatus.Angefangen; break;
                    case "Fertig": changedData.status = AnimeStatus.Fertig; break;
                    case "Unterbrochen": changedData.status = AnimeStatus.Unterbrochen; break;
                    case "Abgebrochen": changedData.status = AnimeStatus.Abgebrochen; break;
                    default: break;
                }

                checkIfAllOriginal();
            }
        }
    }
}