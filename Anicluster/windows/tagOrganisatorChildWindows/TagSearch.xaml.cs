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

        DatabaseController dbController = new DatabaseController();
        List<AnimeTag> animeTagList = new List<AnimeTag>();
        List<ResultRow> resultList = new List<ResultRow>();

        string inputToSearch = "";

        bool letterBig = true;
        bool containsString = false;
        bool ignoreSpecialCharacters = false;

        public TagSimpleSearch() {
            InitializeComponent();

            CenterWindowOnScreen();

            animeTagList = DatabaseController.GetAllAnimeTags();
            updateTable();
        }

        private void CenterWindowOnScreen() {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            double windowWidth = this.Width;
            double windowHeight = this.Height;
            if ((windowHeight > screenHeight) || (windowWidth > screenWidth)) {
                WindowState = WindowState.Maximized;
            }
            else {
                this.Left = (screenWidth / 2) - (windowWidth / 2);
                this.Top = (screenHeight / 2) - (windowHeight / 2);
            }
        }

        private void textBoxTextChange(object sender, TextChangedEventArgs e) {
            updateTable();
        }

        private void updateTable() {
            resultList.Clear();

            inputToSearch = textBoxTagInputForSearch.Text;

            animeTagList = animeTagList.OrderBy(q => q.TagDesignator).ToList();

            for (int i = 0; i < animeTagList.Count; i += 1) {
                string tagDesginatorToCompare = animeTagList[i].TagDesignator;
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
                                tagDesignatorR = animeTagList[i].TagDesignator,
                            });
                        }
                    }
                    else {
                        if (tagDesginatorToCompare.ToLower().StartsWith(inputToSearch)) {
                            resultList.Add(new ResultRow() {
                                nr = resultList.Count + 1,
                                tagDesignatorR = animeTagList[i].TagDesignator,
                            });
                        }
                    }
                }
                else {
                    if (!containsString) {
                        if (tagDesginatorToCompare.Contains(inputToSearch)) {
                            resultList.Add(new ResultRow() {
                                nr = resultList.Count + 1,
                                tagDesignatorR = animeTagList[i].TagDesignator,
                            });
                        }
                    }
                    else {
                        if (tagDesginatorToCompare.StartsWith(inputToSearch)) {
                            resultList.Add(new ResultRow() {
                                nr = resultList.Count + 1,
                                tagDesignatorR = animeTagList[i].TagDesignator,
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
