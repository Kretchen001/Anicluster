using Anicluster.windows;
using BiboAnime.datatypes;
using System.Collections.Generic;
using System.Windows;

namespace Anicluster {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        public MainWindow() {
            InitializeComponent();

            Rating rating = new Rating(
                story : 80,
                animation : 80,
                germanDub : 80,
                sound : 80,
                specialEffect : 80
            );

            data.listOfAnimes.Add(new AnimeData {
                id = 1,
                favorite = true,
                name = "Neon Genesis Evangelion",
                originalName = "-",
                rating = rating,
                tags = new List<string>() { "Mecha" },
                urlAnimePlanet = "aksljdf",
                status = AnimeStatus.Fertig, 
                staffeln = 1,
                episodesTotal = 24
            });

            //Dummy Daten für Zeigen.
            List<string> string1 = new List<string>();
            string1.Add("Romanze");
            data.listOfAnimeRows.Add(new AnimeRow() {
                id = (data.listOfAnimeRows.Count + 1),
                favorite = true,
                name = "Tonikawa",
                generalRating = 80,
                tags = convertTagList(string1),
                status = AnimeStatus.Fertig
            });
            List<string> string2 = new List<string>();
            string2.Add("Roboter");
            data.listOfAnimeRows.Add(new AnimeRow() {
                id = (data.listOfAnimeRows.Count + 1),
                favorite = false,
                name = "Neon Genesis Evangelion",
                generalRating = 70,
                tags = convertTagList(string2),
                status = AnimeStatus.Unterbrochen
            });

            dataGridAnimeList.ItemsSource = data.listOfAnimeRows;
        }

        private void click_DummyDaten(object sender, RoutedEventArgs e) {
            List<string> string3 = new List<string>();
            string3.Add("Romanze");
            string3.Add("Militär");
            data.listOfAnimeRows.Add(new AnimeRow() {
                id = (data.listOfAnimeRows.Count + 1),
                favorite = true,
                name = "Girls & Panzer",
                generalRating = 85,
                tags = convertTagList(string3),
                status = AnimeStatus.Wunschliste
            });
            dataGridAnimeList.Items.Refresh();
        }

        private string convertTagList(List<string> tempList) {
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
            
            // get the id per clicked Details-Button
            int currentRowIndex = dataGridAnimeList.Items.IndexOf(dataGridAnimeList.CurrentItem);
            //buttonHinzufuegen.Content = currentRowIndex.ToString();
            AnimeRow selectedAnime = (AnimeRow) dataGridAnimeList.Items[currentRowIndex];
            //buttonHinzufuegen.Content = selectedAnime.id.ToString();

            // give the anime to the ShowDetails-Window
            ShowDetails showDetailsScreen = new ShowDetails(data.listOfAnimes[selectedAnime.id - 1]);
            showDetailsScreen.Show();

            //int index = dataGridAnimeList.Items.IndexOf(dataGridAnimeList.CurrentItem); // begin by 0. Give the rowNumber | NOT THE ID
        }

        private void click_UpdateView(object sender, RoutedEventArgs e) {
            dataGridAnimeList.ItemsSource = null;
            dataGridAnimeList.ItemsSource = data.listOfAnimeRows;
        }

        private void click_AddNewAnime(object sender, RoutedEventArgs e) {

        }

        private void click_OpenInformation(object sender, RoutedEventArgs e) {

        }
    }
}