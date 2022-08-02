using BiboAnime.databaseLiteDB;
using BiboAnime.datatypes;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Anicluster.windows.statistic {
    /// <summary>
    /// Interaktionslogik für Statistic.xaml
    /// </summary>
    public partial class Statistic : Window {

        databaseController dbController = new databaseController();
        List<AnimeData> animes = new List<AnimeData>();

        public Statistic() {
            InitializeComponent();

            // load data
            animes = dbController.getAllAnimes();

            this.Show(); // this line makes ActualHeight work

            // show Statistics
            loadTiers();
        }

        private void loadTiers() {

            double s = 0;
            double a = 0;
            double b = 0;
            double c = 0;
            double d = 0;
            double dummy = 0;

            for (int i = 0; i < animes.Count; i += 1) {
                switch (animes[i].tier) {
                    case AnimeTier.S: s += 1; break;
                    case AnimeTier.A: a += 1; break;
                    case AnimeTier.B: b += 1; break;
                    case AnimeTier.C: c += 1; break;
                    case AnimeTier.D: d += 1; break;
                    case AnimeTier.Dummy: dummy += 1; break;
                    default: break;
                }
            }

            // print data in bar
            double maxTierValue = new double[] { s, a, b, c, d, dummy }.Max();

            barTierS.Height = (s / maxTierValue) * barBehindTierS.ActualHeight;
            barTierA.Height = (a / maxTierValue) * barBehindTierA.ActualHeight;
            barTierB.Height = (b / maxTierValue) * barBehindTierB.ActualHeight;
            barTierC.Height = (c / maxTierValue) * barBehindTierC.ActualHeight;
            barTierD.Height = (d / maxTierValue) * barBehindTierD.ActualHeight;
            barTierDummy.Height = (dummy / maxTierValue) * barBehindTierDummy.ActualHeight;

            // print data at label
            labelTierSAmount.Content = s.ToString();
            labelTierAAmount.Content = a.ToString();
            labelTierBAmount.Content = b.ToString();
            labelTierCAmount.Content = c.ToString();
            labelTierDAmount.Content = d.ToString();
            labelTierDummyAmount.Content = dummy.ToString();
        }

        private void focusOnTiers(object sender, RoutedEventArgs e) {
            loadTiers();
        }

        private void loadFavorite() {

            double nonFav = 0;
            double fav = 0;

            for (int i = 0; i < animes.Count; i += 1) {
                switch (animes[i].favorite) {
                    case true: fav += 1; break;
                    case false: nonFav += 1; break;
                }
            }

            this.Show();

            labelFavoriteBar.Width = (fav / animes.Count) * labelNonFavoriteBar.ActualWidth;

            labelTotalNumber.Content = "Ausgewertete Animes: " + animes.Count.ToString();
        }

        private void focusOnFavorite(object sender, RoutedEventArgs e) {
            loadFavorite();
        }

        private void loadTagUsage() {
            if (animes.Count == 0) {
                dataGridTags.ItemsSource = null;
            }
            else {
                Dictionary<string, int> tagDictionary = new Dictionary<string, int>();


                for (int i = 0; i < animes.Count; i += 1) {
                    for (int j = 0; j < animes[i].tags.Count; j += 1) {
                        if (tagDictionary.ContainsKey(animes[i].tags[j].tagDesignator)) {
                            tagDictionary[animes[i].tags[j].tagDesignator] += 1;
                        }
                        else {
                            tagDictionary.Add(animes[i].tags[j].tagDesignator, 1);
                        }
                    }
                }

                int maxTag = tagDictionary.Values.Max();

                List<DatatypForTable> tempListForTable = new List<DatatypForTable>();
                for (int i = 0; i < tagDictionary.Count; i += 1) {
                    tempListForTable.Add(new DatatypForTable() {
                        id = i + 1,
                        name = tagDictionary.ElementAt(i).Key,
                        value = tagDictionary.ElementAt(i).Value,
                        maxTag = maxTag
                    });
                }

                dataGridTags.ItemsSource = null;
                dataGridTags.ItemsSource = tempListForTable;
            }
        }

        internal class DatatypForTable {
            public int id { get; set; }
            public string name { get; set; }
            public int value { get; set; }
            public int maxTag { get; set; }
        }

        private void focusOnTagsUsage(object sender, RoutedEventArgs e) {
            loadTagUsage();
        }

        private void loadStatus() {

            double wishList = 0;
            double start = 0;
            double finish = 0;
            double stalled = 0;
            double dropped = 0;

            for (int i = 0; i < animes.Count; i += 1) {
                switch (animes[i].status) {
                    case AnimeStatus.Wunschliste: wishList += 1; break;
                    case AnimeStatus.Angefangen: start += 1; break;
                    case AnimeStatus.Fertig: finish += 1; break;
                    case AnimeStatus.Unterbrochen: stalled += 1; break;
                    case AnimeStatus.Abgebrochen: dropped += 1; break;
                    default: break;
                }
            }

            double maxStatusCount = new double[] { wishList, start, finish, stalled, dropped }.Max();

            labelBarWishList.Height = (wishList / maxStatusCount) * labelBarWishListBackground.ActualHeight;
            labelBarStart.Height = (start / maxStatusCount) * labelBarStartBackground.ActualHeight;
            labelBarFinish.Height = (finish/ maxStatusCount) * labelBarFinishBackground.ActualHeight;
            labelBarStalled.Height = (stalled/ maxStatusCount) * labelBarStalledBackground.ActualHeight;
            labelBarDropped.Height = (dropped / maxStatusCount) * labelBarDroppedBackground.ActualHeight;

            labelNumberWishList.Content = wishList.ToString();
            labelNumberStart.Content = start.ToString();
            labelNumberFinish.Content = finish.ToString();
            labelNumberStalled.Content = stalled.ToString();
            labelNumberDropped.Content = dropped.ToString();
        }

        private void focusOnStatus(object sender, RoutedEventArgs e) {
            loadStatus();
        }
    }
}
