using BiboAnime.datatypes;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Anicluster.windows {
    /// <summary>
    /// Interaktionslogik für ShowDetailsTags.xaml
    /// </summary
    public partial class ShowDetailsTags : Window {

        public ShowDetailsTags(List<AnimeTag> choosenTagList) {
            InitializeComponent();

            List<ShowTag> list = new List<ShowTag>();
            for (int i = 0; i < choosenTagList.Count; i += 1) {
                list.Add(new ShowTag() {
                    number = i + 1,
                    tagDesignator = choosenTagList[i].tagDesignator,
                });
            }

            dataGridTagList.ItemsSource = list.OrderBy(x => x.tagDesignator).ToList();
        }

        private class ShowTag {
            public int number { get; set; }
            public string tagDesignator { get; set; }
        }

        private void clickConfirmTagChoice(object sender, RoutedEventArgs e) {
            this.Close();
        }
    }
}
