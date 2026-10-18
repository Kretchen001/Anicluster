using Anicluster.ViewModels;
using Modells.Anime;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Anicluster.Views {

    /// <summary>
    /// Dialog zum schnellen Anlegen eines Animes (Name, Favorit, Tier).
    /// </summary>
    public partial class QuickAnimeView : UserControl {

        private QuickAnimeViewModel? ViewModel => this.DataContext as QuickAnimeViewModel;

        public QuickAnimeView() {
            this.InitializeComponent();
            // Der Name wird zuerst eingegeben, deshalb startet das Eingabefeld sichtbar und fokussiert
            this.Loaded += (sender, e) => this.ShowNameEditor();
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

        private void BtnAdd_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.Save();
        }
    }
}
