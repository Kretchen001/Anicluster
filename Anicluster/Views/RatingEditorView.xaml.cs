using Anicluster.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Anicluster.Views {

    /// <summary>
    /// Dialog zum Bewerten eines Animes. Die Handler leiten nur an das <see cref="RatingEditorViewModel"/> weiter.
    /// </summary>
    public partial class RatingEditorView : UserControl {

        private RatingEditorViewModel? ViewModel => this.DataContext as RatingEditorViewModel;

        public RatingEditorView() {
            this.InitializeComponent();
        }

        private void MenuItemAcceptRating_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.Accept();
        }

        private void MenuItemCancel_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.Cancel();
        }

        private void BtnSoundtrackCalculate_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.CalculateSoundtrack();
        }

        private void BtnSoundtrackAdd_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.AddSoundtrack();
        }

        private void BtnOpeningAdd_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.AddOpening();
        }

        private void BtnEndingAdd_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.AddEnding();
        }

        private void BtnSyncroAdd_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.AddSyncro();
        }

        #region Hilfe-Popup (reine UI-Logik)
        private void MenuItemHelp_Click(object sender, RoutedEventArgs e) {
            this.PopUpHelp.IsOpen = true;
        }

        private void PopUpHelp_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e) {
            this.PopUpHelp.IsOpen = false;
        }
        #endregion
    }
}
