using System.Collections.Generic;
using System.Windows;

namespace Anicluster {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        public List<AnimeRow> listOfAnimeRows = new List<AnimeRow>();

        public MainWindow() {
            InitializeComponent();

            //Dummy Daten für Zeigen.
            List<AnimeTags> animeTags = new List<AnimeTags>();
            animeTags.Add(AnimeTags.Romanze);
            listOfAnimeRows.Add(new AnimeRow() {
                id = (listOfAnimeRows.Count + 1),
                favorite = true,
                name = "Tonikawa",
                rating = 80,
                tags = convertTagList(animeTags),
            });
            List<AnimeTags> animeTags2 = new List<AnimeTags>();
            animeTags2.Add(AnimeTags.Roboter);
            listOfAnimeRows.Add(new AnimeRow() {
                id = (listOfAnimeRows.Count + 1),
                favorite = false,
                name = "Neon Genesis Evangelion",
                rating = 70,
                tags = convertTagList(animeTags2),
            });

            dataGridAnimeList.ItemsSource = listOfAnimeRows;
        }

        private void click_DummyDaten(object sender, RoutedEventArgs e) {
            List<AnimeTags> animeTags3 = new List<AnimeTags>();
            animeTags3.Add(AnimeTags.Romanze);
            animeTags3.Add(AnimeTags.Militär);
            listOfAnimeRows.Add(new AnimeRow() {
                id = (listOfAnimeRows.Count + 1),
                favorite = true,
                name = "Girls & Panzer",
                rating = 85,
                tags = convertTagList(animeTags3),
            });
            dataGridAnimeList.Items.Refresh();
        }

        private string convertTagList(List<AnimeTags> tempList) {
            string tempString = "";
            for (int i = 0; i < tempList.Count; i += 1) {
                tempString += tempList[i].ToString();
                if (i != tempList.Count - 1) {
                    tempString += ", ";
                }
            }
            return tempString;
        }

        private void click_ShowDetails(object sender, RoutedEventArgs e) {
            // open new Window with Details of the selected Anime.
            
        }

        private void click_UpdateView(object sender, RoutedEventArgs e) {
            dataGridAnimeList.ItemsSource = null;
            dataGridAnimeList.ItemsSource = listOfAnimeRows;
        }
    }

    public class AnimeRow {
        public int id { get; set; }
        public bool favorite { get; set; }
        public string name { get; set; }
        public int rating { get; set; }
        public string tags { get; set; }
    }

    public enum AnimeTags {
        Romanze,
        Militär,
        Roboter
    }
}
