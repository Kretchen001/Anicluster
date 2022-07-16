using Anicluster.windows;
using Anicluster.windows.filter;
using Anicluster.windows.statistic;
using BiboAnime;
using BiboAnime.databaseLiteDB;
using BiboAnime.datatypes;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace Anicluster {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        private databaseController dbController = new databaseController();

        private bool showTagByColor = false;

        public MainWindow() {
            InitializeComponent();

            dataGridAnimeList.ItemsSource = dbController.getAllAnimesAsRow();
        }
        public void OnWindowClosing(object sender, CancelEventArgs e) {
            // close the application after all orders are done.
            Application.Current.Shutdown();
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

            List<Staffel> staffeln = new List<Staffel> {
                new Staffel() { counter = 1 , episodes = 12},
                new Staffel() { counter = 2 , episodes = 24},
                new Staffel() { counter = 3 , episodes = 12},
            };

            AnimeData dummyAnime = new AnimeData {
                id = Guid.NewGuid(),
                favorite = true,
                name = "Girls & Panzer",
                rating = rating,
                status = AnimeStatus.Fertig,
                tags = tags,
                tier = AnimeTier.A,
                staffeln = staffeln,
            };
            dbController.addAnimeToDB(dummyAnime);

            updateDataGrid();
        }

        private void updateDataGrid() {
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
            int currentRowIndex = dataGridAnimeList.Items.IndexOf(dataGridAnimeList.CurrentItem); // begin by 0. Give the rowNumber
            if ((currentRowIndex != (dataGridAnimeList.Items.Count)) || (currentRowIndex == 0)) {
                AnimeRow selectedAnime = (AnimeRow)dataGridAnimeList.Items[currentRowIndex];

                // give the anime to the ShowDetails-Window
                ShowDetails showDetailsScreen = new ShowDetails(dbController.getAnimeByName(selectedAnime.name));
                showDetailsScreen.shouldViewUpdated = updateAfterAddAnime;
                showDetailsScreen.Owner = this;
                showDetailsScreen.Show();

                void updateAfterAddAnime(bool e) {
                    updateDataGrid();
                }
            }
        }

        private void click_UpdateView(object sender, RoutedEventArgs e) {
            dataGridAnimeList.ItemsSource = null;
            dataGridAnimeList.ItemsSource = dbController.getAllAnimesAsRow();
        }

        private void click_AddNewAnime(object sender, RoutedEventArgs e) {
            AddAnime addAnimeWindow = new AddAnime();
            addAnimeWindow.shouldViewUpdated = updateAfterAddAnime;
            addAnimeWindow.Owner = this;
            addAnimeWindow.Show();

            void updateAfterAddAnime(bool e) {
                updateDataGrid();
            }
        }

        private void click_OpenInformation(object sender, RoutedEventArgs e) {
            unimplementetYet();
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

        private void clickExportDataAnimes(object sender, RoutedEventArgs e) {
            string dir = AppDomain.CurrentDomain.BaseDirectory + "backup";

            if (!Directory.Exists(dir)) {
                Directory.CreateDirectory(dir);
            }

            SaveFileDialog saveFileDialogWindow = new SaveFileDialog() {
                InitialDirectory = dir,
                FileName = "animes.csv",
            };

            saveFileDialogWindow.ShowDialog();

            Export.exportAnimeList(saveFileDialogWindow.FileName);
        }

        private void clickImportDataAnimes(object sender, RoutedEventArgs e) {
            unimplementetYet();
        }

        private void clickImporDataTags(object sender, RoutedEventArgs e) {
            string dir = AppDomain.CurrentDomain.BaseDirectory + "backup";

            if (!Directory.Exists(dir)) {
                dir = AppDomain.CurrentDomain.BaseDirectory;
            }

            OpenFileDialog openFileDialogWindow = new OpenFileDialog() {
                InitialDirectory = dir,
                FileName = "tags.csv",
            };

            openFileDialogWindow.ShowDialog();

            if (Import.checkTagListValid(openFileDialogWindow.FileName)) {
                Import.importAnimeTags(openFileDialogWindow.FileName, true);
            }
            else {
                var mail = MessageBox.Show("Import-Datei schadhaft.\nBitte an den Programmierer wenden!\nDatei bestenfalls mitsenden.",
                    "Importfehler",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                if (mail == MessageBoxResult.Yes) { 
                    // TODO: mailto einrichten?
                }
            }
        }

        private void click_tagsOrganisation(object sender, RoutedEventArgs e) {
            TagOrganisator tagOrganisatorWindow = new TagOrganisator();
            tagOrganisatorWindow.Owner = this;
            tagOrganisatorWindow.Show();
        }

        private void clickFilterWindow(object sender, RoutedEventArgs e) {
            FilterWindow filterWindow = new FilterWindow();
            filterWindow.Owner = this;
            filterWindow.Show();
        }

        private void toggleTierColorShown(object sender, RoutedEventArgs e) {
            updateDataGrid();
            if (showTagByColor) {
                menuItemToggleTierColorShown.Header = "Nach Tier einfärben";
            }
            else {
                menuItemToggleTierColorShown.Header = "Tierfärbung ausschalten";
            }
            showTagByColor = !showTagByColor;
        }

        private void loadingRowDataGrid(object sender, System.Windows.Controls.DataGridRowEventArgs e) {
            if (showTagByColor) {
                AnimeRow tempRow = (AnimeRow)e.Row.DataContext;
                switch (dbController.getAnimeByName(tempRow.name).tier) {
                    case AnimeTier.S: e.Row.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF00D4D4")); break;
                    case AnimeTier.A: e.Row.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF3FFF2F")); break;
                    case AnimeTier.B: e.Row.Background = new SolidColorBrush(Colors.Yellow); break;
                    case AnimeTier.C: e.Row.Background = new SolidColorBrush(Colors.Orange); break;
                    case AnimeTier.D: e.Row.Background = new SolidColorBrush(Colors.Red); break;
                    case AnimeTier.Dummy: e.Row.Background = new SolidColorBrush(Colors.Silver); break;
                    default: break;
                }
            }
            else {
                e.Row.Background = new SolidColorBrush(Colors.White);
            }
        }

        private void click_OpenStatistik(object sender, RoutedEventArgs e) {
            Statistic statisticWindow = new Statistic();
            statisticWindow.Owner = this;
            statisticWindow.Show();
        }

        private void clickImpressum(object sender, RoutedEventArgs e) {
            Impressum impressumWindow = new Impressum();
            impressumWindow.Owner = this;
            impressumWindow.Show();
        }
    }
}