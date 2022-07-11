using BiboAnime.datatypes;
using System.Collections.Generic;
using System.Windows;

namespace Anicluster.windows {
    /// <summary>
    /// Interaktionslogik für StaffelManager.xaml
    /// </summary>
    public partial class StaffelManager : Window {

        public List<Staffel> managerStaffeln = new List<Staffel>();

        private int epi = 0;

        public StaffelManager(List<Staffel> givenStaffeln) {

            this.managerStaffeln = givenStaffeln;

            InitializeComponent();

            givenEpisodes.Text = epi.ToString();

            updateDataGrid();
        }

        private void updateDataGrid() {
            dataGrid.ItemsSource = null;
            dataGrid.ItemsSource = managerStaffeln;
        }

        private void textChangeEpisodes(object sender, System.Windows.Controls.TextChangedEventArgs e) {

            if (int.TryParse(givenEpisodes.Text, out int numericValue)) {
                epi = numericValue;
            }
            else {
                givenEpisodes.Text = epi.ToString();
            }

        }

        private void clickAdd(object sender, RoutedEventArgs e) {

            if (epi == 0) {
                MessageBox.Show("Eine Staffel hat doch keine 0 Folgen, oder?", "Anzahl?", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            managerStaffeln.Add(new Staffel() {
                counter = (managerStaffeln.Count + 1),
                episodes = epi
            });

            updateDataGrid();

        }

        private void clickDelete(object sender, RoutedEventArgs e) {
            // TODO: impl
        }

        private void clickFinish(object sender, RoutedEventArgs e) {
            this.Close();
        }
    }
}
