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
using System.Linq;
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
                FileName = "tags.json",
                Filter = "json files (*.json)|*.json|All files (*.*)|*.*"
            };

            if (saveFileDialogWindow.ShowDialog() == false) {
                return;
            }

            if (Export.ExportTagListJson(saveFileDialogWindow.FileName)) {
                MessageBox.Show("Tags exportiert", "Meldung", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else {
                MessageBox.Show("Da ist was beim Export schief gelaufen", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void clickExportDataAnimes(object sender, RoutedEventArgs e) {
            string dir = AppDomain.CurrentDomain.BaseDirectory + "backup";

            if (!Directory.Exists(dir)) {
                Directory.CreateDirectory(dir);
            }

            SaveFileDialog saveFileDialogWindow = new SaveFileDialog() {
                InitialDirectory = dir,
                FileName = "animes.json",
                Filter = "json files (*.json)|*.json|All files (*.*)|*.*"
            };

            if (saveFileDialogWindow.ShowDialog() == false) {
                return;
            }

            if (Export.ExportAnimeListJson(saveFileDialogWindow.FileName)) {
                MessageBox.Show("Animes exportiert", "Meldung", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else {
                MessageBox.Show("Da ist was beim Export schief gelaufen", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void clickImportDataAnimes(object sender, RoutedEventArgs e) {
            string dir = AppDomain.CurrentDomain.BaseDirectory + "backup";

            if (!Directory.Exists(dir)) {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory);
            }

            OpenFileDialog openFileDialogWindow = new OpenFileDialog() {
                InitialDirectory = dir,
                FileName = "animes.json",
                Filter = "json files (*.json)|*.json|All files (*.*)|*.*"
            };

            openFileDialogWindow.ShowDialog();

            MessageBoxResult resultAddOrReset = MessageBox.Show(
                "Datenbank um die Animes erweitern?\nJa ist erweitern!\nNein ist Ersetzten! <- Dies löscht die aktuellen Daten!",
                "Datenbankaktion",
                MessageBoxButton.YesNoCancel);

            if (resultAddOrReset == MessageBoxResult.Yes) {
                if (!Import.importAnimeList(openFileDialogWindow.FileName, true)) {
                    var mail = MessageBox.Show(
                        "Import-Datei schadhaft.\nBitte an den Programmierer wenden!\nDatei bestenfalls mitsenden.",
                        "Importfehler",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    if (mail == MessageBoxResult.Yes) {
                        // TODO: mailto einrichten?
                    }
                }
            }
            else if (resultAddOrReset == MessageBoxResult.No) {
                if (!Import.importAnimeList(openFileDialogWindow.FileName, false)) {
                    var mail = MessageBox.Show(
                        "Import-Datei schadhaft.\nBitte an den Programmierer wenden!\nDatei bestenfalls mitsenden.",
                        "Importfehler",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    if (mail == MessageBoxResult.Yes) {
                        // TODO: mailto einrichten?
                    }
                }
            }
            else { //resultAddOrReset == MessageBoxResult.Cancel
                return;
            }
            updateDataGrid();
        }

        private void clickImportDataTags(object sender, RoutedEventArgs e) {
            string dir = AppDomain.CurrentDomain.BaseDirectory + "backup";

            if (!Directory.Exists(dir)) {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory);
            }

            OpenFileDialog openFileDialogWindow = new OpenFileDialog() {
                InitialDirectory = dir,
                FileName = "tags.json",
                Filter = "json files (*.json)|*.json|All files (*.*)|*.*"
            };

            openFileDialogWindow.ShowDialog();

            MessageBoxResult resultAddOrReset = MessageBox.Show(
                "Datenbank um Tags erweitern?\nJa ist erweitern!\nNein ist Ersetzten!",
                "Datenbankaktion",
                MessageBoxButton.YesNoCancel);

            if (resultAddOrReset == MessageBoxResult.Yes) {
                if (!Import.importAnimeTags(openFileDialogWindow.FileName, true)) {
                    var mail = MessageBox.Show(
                        "Import-Datei schadhaft.\nBitte an den Programmierer wenden!\nDatei bestenfalls mitsenden.",
                        "Importfehler",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    if (mail == MessageBoxResult.Yes) {
                        // TODO: mailto einrichten?
                    }
                }
            }
            else if (resultAddOrReset == MessageBoxResult.No) {
                if (!Import.importAnimeTags(openFileDialogWindow.FileName, false)) {
                    var mail = MessageBox.Show(
                        "Import-Datei schadhaft.\nBitte an den Programmierer wenden!\nDatei bestenfalls mitsenden.",
                        "Importfehler",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    if (mail == MessageBoxResult.Yes) {
                        // TODO: mailto einrichten?
                    }
                }
            }
            else { //resultAddOrReset == MessageBoxResult.Cancel
                return;
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

        private void clickExportAll(object sender, RoutedEventArgs e) {
            clickExportDataAnimes(sender, e);
            clickExportDataTags(sender, e);
        }
    }
}