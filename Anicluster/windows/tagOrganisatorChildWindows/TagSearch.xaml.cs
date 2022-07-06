using BiboAnime.databaseLiteDB;
using BiboAnime.datatypes;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Anicluster.windows.tagOrganisatorChildWindows {

    /// <summary>
    /// Interaktionslogik für TagSimpleSearch.xaml
    /// </summary>
    public partial class TagSimpleSearch : Window {

        internal class ResultRow {
            public int nr { get; set; }
            public string tagDesignatorR { get; set; } = "";
        }

        databaseController dbController = new databaseController();
        List<AnimeTag> animeTagList = new List<AnimeTag>();
        List<ResultRow> resultList = new List<ResultRow>();

        string inputToSearch = "";

        bool letterBig = true;
        bool containsString = true;

        public TagSimpleSearch() {
            InitializeComponent();
            animeTagList = dbController.getAllAnimeTags();
            updateTable();
        }

        private void textBoxTextChange(object sender, TextChangedEventArgs e) {
            updateTable();
        }

        private void updateTable() {
            resultList.Clear();

            inputToSearch = textBoxTagInputForSearch.Text;

            animeTagList = animeTagList.OrderBy(q => q.tagDesignator).ToList();

            for (int i = 0; i < animeTagList.Count; i += 1) {
                if (letterBig) {
                    inputToSearch = inputToSearch.ToLower();
                    if (!containsString) {
                        if (animeTagList[i].tagDesignator.ToLower().Contains(inputToSearch)) {
                            resultList.Add(new ResultRow() {
                                nr = resultList.Count + 1,
                                tagDesignatorR = animeTagList[i].tagDesignator,
                            });
                        }
                    }
                    else {
                        if (animeTagList[i].tagDesignator.ToLower().StartsWith(inputToSearch)) {
                            resultList.Add(new ResultRow() {
                                nr = resultList.Count + 1,
                                tagDesignatorR = animeTagList[i].tagDesignator,
                            });
                        }
                    }
                }
                else {
                    if (!containsString) {
                        if (animeTagList[i].tagDesignator.Contains(inputToSearch)) {
                            resultList.Add(new ResultRow() {
                                nr = resultList.Count + 1,
                                tagDesignatorR = animeTagList[i].tagDesignator,
                            });
                        }
                    }
                    else {
                        if (animeTagList[i].tagDesignator.StartsWith(inputToSearch)) {
                            resultList.Add(new ResultRow() {
                                nr = resultList.Count + 1,
                                tagDesignatorR = animeTagList[i].tagDesignator,
                            });
                        }
                    }
                }
            }

            dataGridSolutions.ItemsSource = null;
            dataGridSolutions.ItemsSource = resultList;
        }

        private void toggleLetterCheck(object sender, RoutedEventArgs e) {
            letterBig = !letterBig;
            updateTable();
        }

        private void toggleContains(object sender, RoutedEventArgs e) {
            containsString = !containsString;
            updateTable();
        }
    }
}
