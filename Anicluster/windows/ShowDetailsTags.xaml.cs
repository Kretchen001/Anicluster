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

            dataGridTagList.ItemsSource = choosenTagList.OrderBy(x => x.tagDesignator).ToList();
        }

        private void clickConfirmTagChoice(object sender, RoutedEventArgs e) {
            this.Close();
        }
    }
}
