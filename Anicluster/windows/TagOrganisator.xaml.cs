using Anicluster.windows.tagOrganisatorChildWindows;
using BiboAnime;
using BiboAnime.databaseLiteDB;
using BiboAnime.datatypes;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace Anicluster.windows {
    /// <summary>
    /// Interaktionslogik für TagOrganisator.xaml
    /// </summary>
    public partial class TagOrganisator : Window {

        internal class TagRowO {
            public int id { get; set; }
            public int numberOfTag { get; set; }
            public string tagDesignator { get; set; }
        }

        List<TagRowO> animeTags = new List<TagRowO>();
        databaseController dbController = new databaseController();

        public TagOrganisator() {
            InitializeComponent();

            AnimeTag dummyTag = new AnimeTag {
                tagDesignator = "Drachen"
            };
            if (!dbController.checkIfTagExists(dummyTag)) {
                AnzeigeDummy.Header = dummyTag.tagDesignator;
                dbController.addOneTag(dummyTag);
            }

            fetchTags();
        }

        private void fetchTags() {
            List<AnimeTag> tempTags = dbController.getAllAnimeTags();
            animeTags.Clear();
            for (int i = 0; i < tempTags.Count; i += 1) {
                animeTags.Add(new TagRowO() {
                    id = tempTags[i].id,
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
                AnimeTag tempTag = new AnimeTag() {
                    id = selectedTag.id,
                    tagDesignator = selectedTag.tagDesignator
                };
                bool result = MessageBox.Show("Sind Sie sicher, dass sie folgenden Tag löschen wollen? \n \t"
                    + selectedTag.tagDesignator + "[" + selectedTag.id + "]",
                    "Error", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes;
                if (result) {
                    if (dbController.deleteOneTag(tempTag)) {
                        MessageBox.Show("Tag gelöschen: \n \t"
                            + selectedTag.tagDesignator + "[" + selectedTag.id + "]",
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

        private void clickSearchAfterTagComplex(object sender, RoutedEventArgs e) {
            TagComplexSearch complexSearchWindow = new TagComplexSearch();
            complexSearchWindow.Owner = this;
            complexSearchWindow.ShowDialog();
        }

        private void clickExport(object sender, RoutedEventArgs e) {

            string dir = AppDomain.CurrentDomain.BaseDirectory + "backup";

            if (!Directory.Exists(dir)) {
                Directory.CreateDirectory(dir);
            }

            SaveFileDialog saveFileDialogWindow = new SaveFileDialog() {
                InitialDirectory = dir,
                FileName = "tags.csv",
                Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*"
            };

            saveFileDialogWindow.ShowDialog();

            if (!Export.exportAnimeTags(saveFileDialogWindow.FileName)) {
                MessageBox.Show("Da ist was beim Import schief gelaufen", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else {
                MessageBox.Show("Tags importiert", "Meldung", MessageBoxButton.OK, MessageBoxImage.Information);
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