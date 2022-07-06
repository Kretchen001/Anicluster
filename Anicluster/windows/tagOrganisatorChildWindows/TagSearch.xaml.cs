using BiboAnime.databaseLiteDB;
using BiboAnime.datatypes;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
        bool containsString = false;
        bool ignoreSpecialCharacters = false;

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
                string tagDesginatorToCompare = animeTagList[i].tagDesignator;
                if (ignoreSpecialCharacters) {
                    Regex regex = new Regex("[^a-zA-Z]");
                    inputToSearch = regex.Replace(inputToSearch, "");
                    tagDesginatorToCompare = regex.Replace(tagDesginatorToCompare, "");
                }
                if (letterBig) {
                    inputToSearch = inputToSearch.ToLower();
                    if (!containsString) {
                        if (tagDesginatorToCompare.ToLower().Contains(inputToSearch)) {
                            resultList.Add(new ResultRow() {
                                nr = resultList.Count + 1,
                                tagDesignatorR = animeTagList[i].tagDesignator,
                            });
                        }
                    }
                    else {
                        if (tagDesginatorToCompare.ToLower().StartsWith(inputToSearch)) {
                            resultList.Add(new ResultRow() {
                                nr = resultList.Count + 1,
                                tagDesignatorR = animeTagList[i].tagDesignator,
                            });
                        }
                    }
                }
                else {
                    if (!containsString) {
                        if (tagDesginatorToCompare.Contains(inputToSearch)) {
                            resultList.Add(new ResultRow() {
                                nr = resultList.Count + 1,
                                tagDesignatorR = animeTagList[i].tagDesignator,
                            });
                        }
                    }
                    else {
                        if (tagDesginatorToCompare.StartsWith(inputToSearch)) {
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

        private void toggleIgnoreSpecialCharacters(object sender, RoutedEventArgs e) {
            ignoreSpecialCharacters = !ignoreSpecialCharacters;
            updateTable();
        }
    }
}
