using BiboAnime.databaseLiteDB;
using BiboAnime.datatypes;
using System.Collections.Generic;
using System.Windows;

namespace Anicluster.windows {

    /// <summary>
    /// Interaktionslogik für TagList.xaml
    /// </summary>
    public partial class TagList : Window {

        private List<TagRow> tagRows = new List<TagRow>();
        public List<AnimeTag> selectedTagList = new List<AnimeTag>();

        internal class TagRow {
            public int id { get; set; }
            public string tag { get; set; }
            public bool isChecked { get; set; }
        }

        public TagList(List<AnimeTag> givenTagList) {
            InitializeComponent();

            databaseController dbController = new databaseController();

            List<AnimeTag> tagListDB = dbController.getAllAnimeTags();

            for (int i = 0; i < tagListDB.Count; i += 1) {
                tagRows.Add(new TagRow (){
                    id = tagListDB[i].id,
                    tag = tagListDB[i].tagDesignator,
                    isChecked = false
                });
            }

            for (int i = 0; i < givenTagList.Count; i += 1) {
                for (int j = 0; j < tagRows.Count; j += 1) {
                    if (givenTagList[i].tagDesignator == tagListDB[j].tagDesignator) {
                        tagRows[i].isChecked = true;
                    } 
                }
            }

            dataGridTagList.ItemsSource = tagRows;
        }

        private void clickConfirmTagChoice(object sender, RoutedEventArgs e) {

        }
    }
}
