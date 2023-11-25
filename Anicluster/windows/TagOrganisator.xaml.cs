using Anicluster.windows.tagOrganisatorChildWindows;
using BiboAnime;
using BiboAnime.databaseLiteDB;
using BiboAnime.datatypes;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace Anicluster.windows {
    /// <summary>
    /// Interaktionslogik für TagOrganisator.xaml
    /// </summary>
    public partial class TagOrganisator : Window {

        internal class TagRowO {
            public int numberOfTag { get; set; }
            public string tagDesignator { get; set; }
        }

        List<TagRowO> animeTags = new List<TagRowO>();
        databaseController dbController = new databaseController();

        public TagOrganisator() {
            InitializeComponent();

            fetchTags();
        }

        private void fetchTags() {
            List<AnimeTag> tempTags = dbController.getAllAnimeTags();
            tempTags = tempTags.OrderBy(x => x.tagDesignator).ToList();
            animeTags.Clear();
            for (int i = 0; i < tempTags.Count; i += 1) {
                animeTags.Add(new TagRowO() {
                    numberOfTag = (i + 1),
                    tagDesignator = tempTags[i].tagDesignator
                });
            }
            dataGridTags.ItemsSource = null;
            dataGridTags.ItemsSource = animeTags;
        }

        private void clickAddTag(object sender, RoutedEventArgs e) {
            TagAdd tagAddWindow = new TagAdd();
            tagAddWindow.Owner = this;
            tagAddWindow.ShowDialog();
            fetchTags();
        }

        private void clickDeleteSelectedTag(object sender, RoutedEventArgs e) {
            // get the id per clicked Details-Button
            int currentRowIndex = dataGridTags.Items.IndexOf(dataGridTags.CurrentItem); // begin by 0. Give the rowNumber | NOT THE ID
            if ((currentRowIndex != (dataGridTags.Items.Count)) && (currentRowIndex != -1)) {
                TagRowO selectedTag = (TagRowO)dataGridTags.Items[currentRowIndex];
                AnimeTag tempTag = dbController.getTagByDesignator(selectedTag.tagDesignator);
                bool result = MessageBox.Show("Sind Sie sicher, dass sie folgenden Tag löschen wollen? \n \t"
                    + selectedTag.tagDesignator + "[" + tempTag.id + "]",
                    "Error", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes;
                if (result) {
                    if (dbController.deleteOneTag(tempTag)) {
                        MessageBox.Show("Tag gelöschen: \n \t"
                            + selectedTag.tagDesignator + " [" + tempTag.id + "]",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                        fetchTags();
                    }
                }
            }
        }

        private void clickSearchAfterTagSimple(object sender, RoutedEventArgs e) {
            TagSimpleSearch simpleSearchWindow = new TagSimpleSearch();
            simpleSearchWindow.Owner = this;
            simpleSearchWindow.ShowDialog();
        }

        private void clickExport(object sender, RoutedEventArgs e) {

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

            if (!Export.ExportTagListJson(saveFileDialogWindow.FileName)) {
                MessageBox.Show("Tags exportiert", "Meldung", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else {
                MessageBox.Show("Da ist was beim Export schief gelaufen", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void clickImport(object sender, RoutedEventArgs e) {

            string dir = AppDomain.CurrentDomain.BaseDirectory + "backup";

            if (!Directory.Exists(dir)) {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory);
            }

            OpenFileDialog openFileDialogWindow = new OpenFileDialog() {
                InitialDirectory = dir,
                FileName = "tags.csv",
                Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*"
            };

            openFileDialogWindow.ShowDialog();

            Import.importAnimeTags(openFileDialogWindow.FileName, false);
        }
    }
}