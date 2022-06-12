using BiboAnime.datatypes;
using System.Collections.Generic;
using System.Windows;

namespace Anicluster.windows {
    /// <summary>
    /// Interaktionslogik für TagList.xaml
    /// </summary>
    public partial class TagList : Window {

        public TagList(List<AnimeTag> tagList) {
            InitializeComponent();

            dataGridTagList.ItemsSource = tagList;
        }

        private void clickConfirmTagChoice(object sender, RoutedEventArgs e) {

        }
    }
}
