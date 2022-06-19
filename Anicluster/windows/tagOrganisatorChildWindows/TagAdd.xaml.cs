using BiboAnime.databaseLiteDB;
using BiboAnime.datatypes;
using System.Windows;
using System.Windows.Controls;

namespace Anicluster.windows.tagOrganisatorChildWindows {
    /// <summary>
    /// Interaktionslogik für TagAdd.xaml
    /// </summary>
    public partial class TagAdd : Window {

        private string tagDesignator = "";

        public TagAdd() {
            InitializeComponent();
        }

        private void textChangeTagDesignator(object sender, TextChangedEventArgs e) {
            tagDesignator = textBoxTagDesignator.Text;
        }

        private void clickAdd(object sender, RoutedEventArgs e) {
            if (tagDesignator != "") {
                databaseController dbController = new databaseController();
                AnimeTag newTag = new AnimeTag() {
                    tagDesignator = tagDesignator,
                };
                if (!dbController.checkIfTagExists(newTag)) {
                    dbController.addOneTag(newTag);
                    this.Close();
                }
                else {
                    MessageBox.Show("Tag existiert schon!", "Warnhinweis", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else {
                MessageBox.Show("Tag ohne Text!", "Warnhinweis", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void clickAbort(object sender, RoutedEventArgs e) {
            this.Close();
        }
    }
}
