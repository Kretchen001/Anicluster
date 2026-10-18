using Anicluster.ViewModels;
using Modells.Anime;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Anicluster.Views {

    /// <summary>
    /// Dialog zum Verwalten der Staffeln. Die Handler leiten an das <see cref="SeasonEditorViewModel"/> weiter,
    /// das Ziehen und Ablegen ist reine UI-Geste (<see cref="ListReorderHelper"/>).
    /// </summary>
    public partial class SeasonEditorView : UserControl {

        private SeasonEditorViewModel? ViewModel => this.DataContext as SeasonEditorViewModel;

        public SeasonEditorView() {
            this.InitializeComponent();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.AddSeason();
        }

        private void BtnAccept_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.Accept();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.Cancel();
        }

        private void ListViewSeasons_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Delete) {
                this.ViewModel?.RemoveSelectedSeason();
            }
        }

        private void ListViewSeasons_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            ListReorderHelper.StartDrag(e);
        }

        private void ListViewSeasons_DragOver(object sender, DragEventArgs e) {
            ListReorderHelper.AllowDrop<Season>(e);
        }

        private void ListViewSeasons_Drop(object sender, DragEventArgs e) {
            if (ListReorderHelper.TryGetDrop(this.ListViewSeasons, e, out Season season, out int targetIndex)) {
                this.ViewModel?.MoveSeason(season, targetIndex);
            }
        }
    }
}
