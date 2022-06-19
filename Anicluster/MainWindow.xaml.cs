using Anicluster.windows;
using BiboAnime;
using BiboAnime.databaseLiteDB;
using BiboAnime.datatypes;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace Anicluster {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        private databaseController dbController = new databaseController();

        public MainWindow() {
            InitializeComponent();

            dataGridAnimeList.ItemsSource = dbController.getAllAnimesAsRow();
        }

        private void click_DummyDaten(object sender, RoutedEventArgs e) {

            databaseController dbController = new databaseController();

            Rating rating = new Rating(
                story: 74,
                sound: 98,
                animation: 14,
                specialEffect: 42,
                germanDub: 0
            );
            List<AnimeTag> tags = new List<AnimeTag> {
                new AnimeTag() { tagDesignator = "Romanze" },
                new AnimeTag() { tagDesignator = "Militär" }
            };

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
            // get the id per clicked Details-Button
            int currentRowIndex = dataGridAnimeList.Items.IndexOf(dataGridAnimeList.CurrentItem); // begin by 0. Give the rowNumber | NOT THE ID
            if ((currentRowIndex != (dataGridAnimeList.Items.Count)) || (currentRowIndex == 0)) {
                AnimeRow selectedAnime = (AnimeRow)dataGridAnimeList.Items[currentRowIndex];

                // give the anime to the ShowDetails-Window
                ShowDetails showDetailsScreen = new ShowDetails(dbController.getAnimeById(selectedAnime.id));
                showDetailsScreen.Show();
            }
        }

        private void click_UpdateView(object sender, RoutedEventArgs e) {
            dataGridAnimeList.ItemsSource = null;
            dataGridAnimeList.ItemsSource = dbController.getAllAnimesAsRow();
        }

        private void click_AddNewAnime(object sender, RoutedEventArgs e) {
            AddAnime addAnime = new AddAnime();
            addAnime.Show();
        }

        private void click_OpenInformation(object sender, RoutedEventArgs e) {

        }

        private void unimplementetYet() {
            MessageBox.Show("Komm später wieder ;)", "Unimplementiert", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void clickExportDataTags(object sender, RoutedEventArgs e) {

            string dir = AppDomain.CurrentDomain.BaseDirectory + "backup";

            if (!Directory.Exists(dir)) {
                Directory.CreateDirectory(dir);
            }

            SaveFileDialog saveFileDialogWindow = new SaveFileDialog() {
                InitialDirectory = dir,
                FileName = "tags.csv",
            };

            saveFileDialogWindow.ShowDialog();

            Export.exportAnimeTags(saveFileDialogWindow.FileName);
        }

        private void click_tagsOrganisation(object sender, RoutedEventArgs e) {
            TagOrganisator tagOrganisatorWindow = new TagOrganisator();
            tagOrganisatorWindow.Show();
        }

        private void clickExportDataAnimes(object sender, RoutedEventArgs e) {
            unimplementetYet();
        }

        private void clickImportDataAnimes(object sender, RoutedEventArgs e) {
            unimplementetYet();
        }

        private void clickImporDataTags(object sender, RoutedEventArgs e) {
            unimplementetYet();
        }
    }
}