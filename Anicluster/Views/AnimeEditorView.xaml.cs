using Anicluster.ViewModels;
using Modells.Anime;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Anicluster.Views {

    /// <summary>
    /// Gemeinsame View zum Anlegen und Bearbeiten eines Animes.
    /// Die Klick-Handler leiten nur an das <see cref="AnimeEditorViewModel"/> weiter,
    /// im Code-Behind liegt ausschließlich das Umschalten des Namensfelds.
    /// </summary>
    public partial class AnimeEditorView : UserControl {

        private AnimeEditorViewModel? ViewModel => this.DataContext as AnimeEditorViewModel;

        public AnimeEditorView() {
            this.InitializeComponent();
        }

        /// <summary>Bei einem neuen Anime ohne Namen startet das Namensfeld direkt im Eingabemodus.</summary>
        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            if (string.IsNullOrEmpty(this.ViewModel?.Name)) {
                this.ShowNameEditor();
            }
        }

        #region Name (Label <-> TextBox)
        private void ShowNameEditor() {
            this.TBName.Visibility = Visibility.Visible;
            this.LbName.Visibility = Visibility.Collapsed;
            this.TBName.Focus();
        }

        private void ShowNameLabel() {
            this.TBName.Visibility = Visibility.Collapsed;
            this.LbName.Visibility = Visibility.Visible;
        }

        private void LbName_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            this.ShowNameEditor();
        }

        private void TBName_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                this.ShowNameLabel();
            }
        }

        private void TBName_LostFocus(object sender, RoutedEventArgs e) {
            this.ShowNameLabel();
        }
        #endregion

        private void StarPath_MouseDown(object sender, MouseButtonEventArgs e) {
            this.ViewModel?.ToggleFavorite();
        }

        /// <summary>Gemeinsamer Handler aller Tier-Buttons, das Tier steht im Tag des Buttons.</summary>
        private void BtnTier_Click(object sender, RoutedEventArgs e) {
            if (sender is Button { Tag: Tier tier }) {
                this.ViewModel?.SetTier(tier);
            }
        }

        private void BtnAutoLinkGeneration_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.GenerateUrl();
        }

        private void BtnRatingGenerator_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.EditRating();
        }

        private void BtnSeasonGenerator_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.EditSeasons();
        }

        private void BtnOvaGenerator_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.EditOvas();
        }

        private void BtnTagsSelector_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.EditTags();
        }

        private void BtnPublishingTime_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.EditPublishingTime();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.Save();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.Cancel();
        }
    }
}
