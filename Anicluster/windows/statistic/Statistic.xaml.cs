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

        public Statistic() {
            InitializeComponent();

            this.Show(); // this line makes ActualHeight work

            loadTiers();
        }

        private void loadTiers() {
            // load data
            List<AnimeData> animes = dbController.getAllAnimes();

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
            double maxHeight = barBehindTierS.ActualHeight;
            double maxTierValue = new double[] { s, a, b, c, d, dummy }.Max();

            barTierS.Height = (s / maxTierValue) * maxHeight;
            barTierA.Height = (a / maxTierValue) * maxHeight;
            barTierB.Height = (b / maxTierValue) * maxHeight;
            barTierC.Height = (c / maxTierValue) * maxHeight;
            barTierD.Height = (d / maxTierValue) * maxHeight;
            barTierDummy.Height = (dummy / maxTierValue) * maxHeight;

            // print data at label
            labelTierSAmount.Content = s.ToString();
            labelTierAAmount.Content = a.ToString();
            labelTierBAmount.Content = b.ToString();
            labelTierCAmount.Content = c.ToString();
            labelTierDAmount.Content = d.ToString();
            labelTierDummyAmount.Content = dummy.ToString();
        }
    }
}
