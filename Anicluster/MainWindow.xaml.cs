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

            //Dummy Daten für Zeigen.
            List<string> string1 = new List<string>();
            string1.Add("Romanze");
            data.listOfAnimeRows.Add(new AnimeRow() {
                id = (data.listOfAnimeRows.Count + 1),
                favorite = true,
                name = "Tonikawa",
                rating = 80,
                tags = convertTagList(string1),
                status = AnimeStatus.Fertig
            });
            List<string> string2 = new List<string>();
            string2.Add("Roboter");
            data.listOfAnimeRows.Add(new AnimeRow() {
                id = (data.listOfAnimeRows.Count + 1),
                favorite = false,
                name = "Neon Genesis Evangelion",
                rating = 70,
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
                rating = 85,
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
