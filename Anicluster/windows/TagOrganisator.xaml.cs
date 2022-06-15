using BiboAnime.databaseLiteDB;
using BiboAnime.datatypes;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Anicluster.windows {
    /// <summary>
    /// Interaktionslogik für TagOrganisator.xaml
    /// </summary>
    public partial class TagOrganisator : Window {

        List<AnimeTag> animeTags = new List<AnimeTag>();
        databaseController dbController = new databaseController();

        public TagOrganisator() {
            InitializeComponent();

            AnimeTag dummyTag = new AnimeTag {
                id = dbController.getTagDbCountForIdPlusOne(),
                tagDesignator = "Drachen"
            };
            if (!dbController.checkIfTagExists(dummyTag)) {
                dbController.addOneTag(dummyTag);
            }

            fetchTags();
        }

        private void fetchTags() {
            animeTags = dbController.getAllAnimeTags();
            dataGridTags.ItemsSource = animeTags;
        }

        private void clickAddTag(object sender, RoutedEventArgs e) {

        }

        private void clickDeleteSelectedTag(object sender, RoutedEventArgs e) {
            // get the id per clicked Details-Button
            int currentRowIndex = dataGridTags.Items.IndexOf(dataGridTags.CurrentItem); // begin by 0. Give the rowNumber | NOT THE ID
            if ((currentRowIndex != (dataGridTags.Items.Count - 1)) && (currentRowIndex != -1)) {
                AnimeTag selectedTag = (AnimeTag) dataGridTags.Items[currentRowIndex];

                bool result = MessageBox.Show("Sind Sie sicher, dass sie folgenden Tag löschen wollen? \n \t" 
                    + selectedTag.tagDesignator + "[" + selectedTag.id + "]",
                    "Error", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes;
                if (result) {
                    if (dbController.deleteOneTag(selectedTag)) {
                        MessageBox.Show("Tag gelöschen: \n \t"
                            + selectedTag.tagDesignator + "[" + selectedTag.id + "]",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                        fetchTags();
                    }
                }
            }
        }

        private void clickSearchAfterTagSimple(object sender, RoutedEventArgs e) {

        }

        private void clickSearchAfterTagComplex(object sender, RoutedEventArgs e) {

        }
    }
}