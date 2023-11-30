using BiboAnime.databaseLiteDB;
using BiboAnime.datatypes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Anicluster.windows.tagOrganisatorChildWindows {
    /// <summary>
    /// Interaktionslogik für TagAdd.xaml
    /// </summary>
    public partial class TagAdd : Window {

        private string tagDesignator = "";

        public TagAdd() {
            InitializeComponent();

            CenterWindowOnScreen();
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

        private void textChangeTagDesignator(object sender, TextChangedEventArgs e) {
            tagDesignator = textBoxTagDesignator.Text;
        }

        private void clickAdd(object sender, RoutedEventArgs e) {
            addLogic();
        }

        private void clickAbort(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void keyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Return) {
                addLogic();
            }
        }

        private void addLogic() {
            if (tagDesignator != "") {
                databaseController dbController = new databaseController();
                AnimeTag newTag = new AnimeTag() {
                    tagDesignator = tagDesignator,
                };
                if (!dbController.checkIfTagExistsByDesignator(newTag)) {
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
    }
}
