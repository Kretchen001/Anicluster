using BiboAnime.datatypes;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Anicluster.windows {
    /// <summary>
    /// Interaktionslogik für StaffelManager.xaml
    /// </summary>
    public partial class StaffelManager : Window {

        private List<Staffel> managerStaffeln = new List<Staffel>();

        private int epi = 0;

        public StaffelManager(List<Staffel> givenStaffeln) {

            if (givenStaffeln is not null) {
                managerStaffeln = givenStaffeln;
            }

            InitializeComponent();

            textBoxGivenEpisodes.Text = epi.ToString();

            updateDataGrid();
        }

        private void updateDataGrid() {
            dataGridStaffeln.ItemsSource = null;
            dataGridStaffeln.ItemsSource = managerStaffeln;
        }

        private void textChangeEpisodes(object sender, System.Windows.Controls.TextChangedEventArgs e) {

            if (int.TryParse(textBoxGivenEpisodes.Text, out int numericValue)) {
                epi = numericValue;
            }
            else {
                textBoxGivenEpisodes.Text = epi.ToString();
            }

        }

        private void clickAdd(object sender, RoutedEventArgs e) {

            if (epi == 0) {
                MessageBox.Show("Eine Staffel hat doch keine 0 Folgen, oder?", "Anzahl?", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            int c = managerStaffeln.Count + 1;
            for (int i = 0; i < managerStaffeln.Count; i++) {
                if (managerStaffeln[i].counter != (i + 1)) {
                    c = i + 1;
                    break;
                }
            }
            managerStaffeln.Add(new Staffel() {
                counter = c,
                episodes = epi
            });

            managerStaffeln = managerStaffeln.OrderBy(x => x.counter).ToList();
            updateDataGrid();

        }

        private void clickDelete(object sender, RoutedEventArgs e) {
            int currentRowIndex = dataGridStaffeln.Items.IndexOf(dataGridStaffeln.SelectedItem); // begin by 0. Give the rowNumber

            managerStaffeln.RemoveAt(currentRowIndex);
                
            updateDataGrid();
        }

        private void clickFinish(object sender, RoutedEventArgs e) {
            this.Close();
        }

        public List<Staffel> getManagerStaffeln() {
            return managerStaffeln;
        }
    }
}
