using Anicluster.ViewModels;
using Modells.Anime;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Anicluster.Views {

    /// <summary>
    /// Dialog zum Verwalten von OVAs, Filmen usw. Die Handler leiten an das <see cref="VideoAnimationEditorViewModel"/> weiter,
    /// das Umsortieren per Drag &amp; Drop ist reine UI-Geste (<see cref="ListReorderHelper{T}"/>).
    /// </summary>
    public partial class VideoAnimationEditorView : UserControl {

        private VideoAnimationEditorViewModel? ViewModel => this.DataContext as VideoAnimationEditorViewModel;

        public VideoAnimationEditorView() {
            this.InitializeComponent();
            _ = new ListReorderHelper<VideoAnimation>(this.GridVideos, (video, index) => this.ViewModel?.MoveVideo(video, index));
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.AddVideo();
        }

        private void BtnAccept_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.Accept();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.Cancel();
        }

        private void GridVideos_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Delete) {
                this.ViewModel?.RemoveSelectedVideo();
            }
        }
    }
}
