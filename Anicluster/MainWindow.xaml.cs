using Anicluster.windows;
using BiboAnime;
using BiboAnime.databaseLiteDB;
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

            databaseController dbController = new databaseController();
            dataGridAnimeList.ItemsSource = dbController.getAllAnimesAsRow();
        }

        private void click_DummyDaten(object sender, RoutedEventArgs e) {
            //List<string> string3 = new List<string>();
            //string3.Add("Romanze");
            //string3.Add("Militär");
            //data.listOfAnimeRows.Add(new AnimeRow() {
            //    id = (data.listOfAnimeRows.Count + 1),
            //    favorite = true,
            //    name = "Girls & Panzer",
            //    generalRating = 85,
            //    tags = convertTagList(string3),
            //    status = AnimeStatus.Wunschliste
            //});

            databaseController dbController = new databaseController();

            Rating rating = new Rating(
                story: 74,
                sound: 98,
                animation: 14,
                specialEffect: 42,
                germanDub: 0
            );
            List<string> tags = new List<string> { "Romanze", "Militär"};

            AnimeData dummyAnime = new AnimeData {
                id = dbController.getDbCountForIdPlusOne(),
                favorite = true,
                name = "Girls & Panzer",
                rating = rating,
                status = AnimeStatus.Fertig,
                tags = tags,
                tier = AnimeTier.A
            };
            dbController.addAnimeToDB(dummyAnime);

            dataGridAnimeList.ItemsSource = null;
            dataGridAnimeList.ItemsSource = dbController.getAllAnimesAsRow();
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
            AnimeRow selectedAnime = (AnimeRow) dataGridAnimeList.Items[currentRowIndex];

            // give the anime to the ShowDetails-Window
            databaseController dbController = new databaseController();
            ShowDetails showDetailsScreen = new ShowDetails(dbController.getAnimeById(selectedAnime.id));
            showDetailsScreen.Show();

            //int index = dataGridAnimeList.Items.IndexOf(dataGridAnimeList.CurrentItem); // begin by 0. Give the rowNumber | NOT THE ID
        }

        private void click_UpdateView(object sender, RoutedEventArgs e) {
            databaseController dbController = new databaseController();

            dataGridAnimeList.ItemsSource = null;
            dataGridAnimeList.ItemsSource = dbController.getAllAnimesAsRow();
        }

        private void click_AddNewAnime(object sender, RoutedEventArgs e) {
            AddAnime addAnime = new AddAnime();
            addAnime.Show();
        }

        private void click_OpenInformation(object sender, RoutedEventArgs e) {

        }

        private void clickExportData(object sender, RoutedEventArgs e) {
            Export.exportAnimeList();
            Export.exportAnimeTags();
        }
    }
}