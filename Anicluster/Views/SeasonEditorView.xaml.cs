using Anicluster.ViewModels;
using Modells.Anime;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Anicluster.Views {

    /// <summary>
    /// Dialog zum Verwalten der Staffeln. Die Handler leiten an das <see cref="SeasonEditorViewModel"/> weiter,
    /// das Umsortieren per Drag &amp; Drop ist reine UI-Geste (<see cref="ListReorderHelper{T}"/>).
    /// </summary>
    public partial class SeasonEditorView : UserControl {

        private SeasonEditorViewModel? ViewModel => this.DataContext as SeasonEditorViewModel;

        public SeasonEditorView() {
            this.InitializeComponent();
            _ = new ListReorderHelper<Season>(this.GridSeasons, (season, index) => this.ViewModel?.MoveSeason(season, index));
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

        private void GridSeasons_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Delete) {
                this.ViewModel?.RemoveSelectedSeason();
            }
        }
    }
}
