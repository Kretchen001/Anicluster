using Modells.Anime;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Anicluster.windows {
    /// <summary>
    /// Interaktionslogik für SeasonGenerator.xaml
    /// </summary>
    public partial class SeasonGenerator : Window {

        public ObservableCollection<Season> Seasons { get; set; }

        public SeasonGenerator(List<Season>? seasons = null) {
            InitializeComponent();
            Seasons = new ObservableCollection<Season>(seasons ?? new List<Season>());
            ListViewSeasons.ItemsSource = Seasons;
        }

        private void BtnHinzufuegen_Click(object sender, RoutedEventArgs e) {
            List<VideoAnimation> list = [];
            for (int i = 0; i < int.Parse(TextBoxEpisodenAnzahl.Text); i += 1) {
                list.Add(new VideoAnimation(VideoType.Episode, null));
            }
            int startYear = ParseOrDefault(TextBoxStartdatum.Text);
            int endYear = ParseOrDefault(TextBoxEnddatum.Text);
            if (startYear > endYear) {
                MessageBox.Show("Start- und Endjahr sind in der Konstellation unmöglich!", "Fehlerhafte eingabe", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (startYear > 9999 || endYear > 9999) {
                MessageBox.Show("Start- und Endjahr sind zu hoch!", "Fehlerhafte eingabe", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Seasons.Add(new Season() {
                Number = Seasons.Count + 1,
                Episodes = list,
                PublishingTime = new PublishingTime() {
                    StartDate = new DateTime(startYear, 1, 1),
                    EndDate = new DateTime(endYear, 1, 1)
                },
                Comment = null
            });
        }

        private void ListViewSeasons_KeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
            if (MessageBox.Show(
                "Beim Löschen wird die Staffel entfernt, der Rest jedoch nicht nachgezogen! " +
                "Beim Ersetzen bitte die passende an die entsprechende Stelle ziehen.",
                "Warnung",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Warning) == MessageBoxResult.Cancel) {
                return;
            }
            if (e.Key == Key.Delete) {
                object selectedItem = ListViewSeasons.SelectedItem;
                if (selectedItem != null) {
                    Seasons.Remove((Season)selectedItem);
                    UpdateNumbers();
                }
            }
        }

        private void ListViewSeasons_DragOver(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(typeof(Season))) {
                e.Effects = DragDropEffects.Move;
            }
            else {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void ListViewSeasons_Drop(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(typeof(Season))) {
                Season droppedData = (Season)e.Data.GetData(typeof(Season));
                ListViewItem targetItem = FindAncestor<ListViewItem>((DependencyObject)e.OriginalSource)!;

                if (targetItem != null) {
                    int targetIndex = ListViewSeasons.Items.IndexOf(targetItem.DataContext);
                    Seasons.Remove(droppedData);
                    Seasons.Insert(targetIndex, droppedData);

                    UpdateNumbers();
                }
            }
        }

        private void ListViewSeasons_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            ListViewItem listViewItem = FindAncestor<ListViewItem>((DependencyObject)e.OriginalSource)!;
            if (listViewItem != null) {
                DragDrop.DoDragDrop(listViewItem, listViewItem.DataContext, DragDropEffects.Move);
            }
        }

        private void UpdateNumbers() {
            List<Season> s = [];
            for (int i = 0; i < Seasons.Count; i += 1) {
                s.Add(Seasons[i]);
                s[i].Number = i + 1;
            }
            Seasons.Clear();
            s.ForEach(s => { Seasons.Add(s); });
        }

        private static T? FindAncestor<T>(DependencyObject current) where T : DependencyObject {
            while (current != null) {
                if (current is T ancestor) {
                    return ancestor;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }

        public static int ParseOrDefault(string input) {
            if (int.TryParse(input, out int result)) {
                return result;
            }
            return 1;
        }
    }
}
