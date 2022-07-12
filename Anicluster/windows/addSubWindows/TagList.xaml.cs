using BiboAnime.databaseLiteDB;
using BiboAnime.datatypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Anicluster.windows {

    /// <summary>
    /// Interaktionslogik für TagList.xaml
    /// </summary>
    public partial class TagList : Window {

        private List<TagRow> tagRows = new List<TagRow>();
        public List<AnimeTag> selectedTagList = new List<AnimeTag>();

        internal class TagRow {
            public Guid id { get; set; }
            public string tagDesignator { get; set; } = "";
            public bool isChecked { get; set; }
        }

        public TagList(List<AnimeTag> givenTagList) {
            InitializeComponent();

            selectedTagList = givenTagList;

            databaseController dbController = new databaseController();

            List<AnimeTag> tagListDB = dbController.getAllAnimeTags();

            for (int i = 0; i < tagListDB.Count; i += 1) {
                TagRow tempTagRow = new TagRow() {
                    id = tagListDB[i].id,
                    tagDesignator = tagListDB[i].tagDesignator,
                    isChecked = false
                };

                for (int j = 0; j < givenTagList.Count; j += 1) {
                    if (givenTagList[j].id == tempTagRow.id) {
                        tempTagRow.isChecked = true;
                    }
                }

                tagRows.Add(tempTagRow);
            }

            updateDataGrid();
        }

        private void clickConfirmTagChoice(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void doubleMouseClickChangeCheck(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            // get the id per clicked Details-Button
            int currentRowIndex = dataGridTagList.Items.IndexOf(dataGridTagList.CurrentItem); // begin by 0. Give the rowNumber | NOT THE ID
            AnimeTag tempTag = new AnimeTag() {
                id = tagRows[currentRowIndex].id,
                tagDesignator = tagRows[currentRowIndex].tagDesignator
            };
            if (tagRows[currentRowIndex].isChecked) {
                tagRows[currentRowIndex].isChecked = false;
                selectedTagList.RemoveAll(x => x.id == tempTag.id);
            }
            else {
                tagRows[currentRowIndex].isChecked = true;
                selectedTagList.Add(tempTag);
            }
            updateDataGrid();
        }

        private void updateDataGrid() {
            tagRows = tagRows.OrderBy(x => x.tagDesignator).ToList();
            dataGridTagList.ItemsSource = null;
            dataGridTagList.ItemsSource = tagRows;
        }
    }
}
