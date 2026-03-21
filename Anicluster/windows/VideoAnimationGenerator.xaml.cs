using Modells.Anime;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Anicluster.windows {

    public partial class VideoAnimationGenerator : Window {

        public ObservableCollection<VideoAnimation> Videos { get; set; }

        public VideoAnimationGenerator(List<VideoAnimation>? videos = null) {
            this.InitializeComponent();

            this.Videos = new ObservableCollection<VideoAnimation>(videos ?? new List<VideoAnimation>());
            this.ListViewVideos.ItemsSource = this.Videos;

            this.ComboBoxType.ItemsSource = Enum.GetValues(typeof(VideoType));
            this.ComboBoxType.SelectedIndex = 0;

            this.UpdateIds();
        }

        private void BtnHinzufuegen_Click(object sender, RoutedEventArgs e) {
            this.Videos.Add(new VideoAnimation() {
                Id = this.Videos.Count + 1,
                VideoType = (VideoType)this.ComboBoxType.SelectedItem,
                Comment = this.TextBoxKommentar.Text
            });
        }

        private void ListViewVideos_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Delete) {
                if (this.ListViewVideos.SelectedItem is VideoAnimation selected) {
                    this.Videos.Remove(selected);
                    this.UpdateIds();
                }
            }
        }

        private void ListViewVideos_DragOver(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(typeof(VideoAnimation))) {
                e.Effects = DragDropEffects.Move;
            }
            else {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void ListViewVideos_Drop(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(typeof(VideoAnimation))) {
                VideoAnimation dropped = (VideoAnimation)e.Data.GetData(typeof(VideoAnimation));
                ListViewItem? targetItem = FindAncestor<ListViewItem>((DependencyObject)e.OriginalSource);

                if (targetItem != null) {
                    int index = this.ListViewVideos.Items.IndexOf(targetItem.DataContext);
                    this.Videos.Remove(dropped);
                    this.Videos.Insert(index, dropped);
                    this.UpdateIds();
                }
            }
        }

        private void ListViewVideos_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            ListViewItem? item = FindAncestor<ListViewItem>((DependencyObject)e.OriginalSource);
            if (item != null) {
                DragDrop.DoDragDrop(item, item.DataContext, DragDropEffects.Move);
            }
        }

        private void UpdateIds() {
            for (int i = 0; i < this.Videos.Count; i++) {
                this.Videos[i].Id = i + 1;
            }
        }

        private static T? FindAncestor<T>(DependencyObject current) where T : DependencyObject {
            while (current != null) {
                if (current is T ancestor)
                    return ancestor;

                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }
    }
}