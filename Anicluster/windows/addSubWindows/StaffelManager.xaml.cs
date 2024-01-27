using BiboAnime.datatypes;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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

            CenterWindowOnScreen();

            textBoxGivenEpisodes.Text = epi.ToString();

            updateDataGrid();
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

        private void updateDataGrid() {
            dataGridStaffeln.ItemsSource = null;
            dataGridStaffeln.ItemsSource = managerStaffeln;
        }

        private void textChangeEpisodes(object sender, TextChangedEventArgs e) {

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
                if (managerStaffeln[i].Counter != (i + 1)) {
                    c = i + 1;
                    break;
                }
            }
            managerStaffeln.Add(new Staffel() {
                Counter = c,
                Episodes = epi
            });

            managerStaffeln = managerStaffeln.OrderBy(x => x.Counter).ToList();
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
