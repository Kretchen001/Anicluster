using Anicluster.ViewModels;
using Modells.Anime;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Anicluster.Views {

    /// <summary>
    /// Dialog zum Verwalten von OVAs, Filmen usw. Die Handler leiten an das <see cref="VideoAnimationEditorViewModel"/> weiter,
    /// das Ziehen und Ablegen ist reine UI-Geste (<see cref="ListReorderHelper"/>).
    /// </summary>
    public partial class VideoAnimationEditorView : UserControl {

        private VideoAnimationEditorViewModel? ViewModel => this.DataContext as VideoAnimationEditorViewModel;

        public VideoAnimationEditorView() {
            this.InitializeComponent();
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

        private void ListViewVideos_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Delete) {
                this.ViewModel?.RemoveSelectedVideo();
            }
        }

        private void ListViewVideos_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            ListReorderHelper.StartDrag(e);
        }

        private void ListViewVideos_DragOver(object sender, DragEventArgs e) {
            ListReorderHelper.AllowDrop<VideoAnimation>(e);
        }

        private void ListViewVideos_Drop(object sender, DragEventArgs e) {
            if (ListReorderHelper.TryGetDrop(this.ListViewVideos, e, out VideoAnimation video, out int targetIndex)) {
                this.ViewModel?.MoveVideo(video, targetIndex);
            }
        }
    }
}
