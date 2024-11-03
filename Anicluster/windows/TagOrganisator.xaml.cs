using Modells.Anime;
using System.Collections.ObjectModel;
using System.Windows;

namespace Anicluster.windows {
    /// <summary>
    /// Interaktionslogik für TagOrganisator.xaml
    /// </summary>
    public partial class TagOrganisator : Window {

        public ObservableCollection<Tag> Tags { get; set; } = new ObservableCollection<Tag>();

        public TagOrganisator() {
            InitializeComponent();
        }

        private void BtnPopUp_Click(object sender, RoutedEventArgs e) {

        }

        private void MenuItemAddTag_Click(object sender, RoutedEventArgs e) {
            PopupInputNewTag.IsOpen = true;
        }
    }
}
