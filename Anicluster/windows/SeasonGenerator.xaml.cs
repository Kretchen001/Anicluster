using Modells.Anime;
using System.Collections.ObjectModel;
using System.Windows;

namespace Anicluster.windows {
    /// <summary>
    /// Interaktionslogik für SeasonGenerator.xaml
    /// </summary>
    public partial class SeasonGenerator : Window {

        public ObservableCollection<Season> Seasons { get; set; }

        public SeasonGenerator() {
            InitializeComponent();
            Seasons = new ObservableCollection<Season>();
            ListViewSeasons.ItemsSource = Seasons;
        }

        private void BtnHinzufuegen_Click(object sender, RoutedEventArgs e) {
            List<VideoAnimation> list = [];
            for (int i = 0; i < int.Parse(TextBoxEpisodenAnzahl.Text); i+=1) {
                list.Add(new VideoAnimation(VideoType.Episode, null));
            }
            int startYear = ParseOrDefault(TextBoxStartdatum.Text);
            int endYear = ParseOrDefault(TextBoxEnddatum.Text);
            if (startYear >= endYear) {
                MessageBox.Show("Start- und Endjahr sind in der Konstellation unmöglich!", "Fehlerhafte eingabe", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (startYear > 9999 || endYear > 9999) {
                MessageBox.Show("Start- und Endjahr sind zu hoch!", "Fehlerhafte eingabe", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Seasons.Add(new Season() {
                Episodes = list,
                PublishingTime = new PublishingTime() {
                    StartDate = new DateTime(startYear, 1, 1),
                    EndDate = new DateTime(endYear, 1, 1)
                },
                Comment = null
            });
        }

        public int ParseOrDefault(string input) {
            if (int.TryParse(input, out int result)) {
                return result;
            }
            return 1;
        }
    }
}
