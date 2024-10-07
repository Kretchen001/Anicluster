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

        public record SeasonWithId {

            private int _nr;
            public int SeasonNr {
                get => _nr;
                set {
                    _nr = value;
                }
            }
            public Season Season { get; set; } = new Season();
        }

        public ObservableCollection<SeasonWithId> Seasons { get; set; }

        public SeasonGenerator() {
            InitializeComponent();
            Seasons = new ObservableCollection<SeasonWithId>();
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
            Seasons.Add(new SeasonWithId() {
                SeasonNr = Seasons.Count + 1,
                Season = new Season() {
                    Episodes = list,
                    PublishingTime = new PublishingTime() {
                        StartDate = new DateTime(startYear, 1, 1),
                        EndDate = new DateTime(endYear, 1, 1)
                    },
                    Comment = null
                }
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
                    Seasons.Remove((SeasonWithId)selectedItem);
                    UpdateNumbers();
                }
            }
        }

        private void ListViewSeasons_DragOver(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(typeof(SeasonWithId))) {
                e.Effects = DragDropEffects.Move;
            }
            else {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void ListViewSeasons_Drop(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(typeof(SeasonWithId))) {
                SeasonWithId droppedData = (SeasonWithId)e.Data.GetData(typeof(SeasonWithId));
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
            List<SeasonWithId> s = [];
            for (int i = 0; i < Seasons.Count; i += 1) {
                s.Add(new SeasonWithId() {
                    SeasonNr = i + 1,
                    Season = Seasons[i].Season
                });
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
