using Anicluster.windows;
using System.Windows;

namespace Anicluster {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            SeasonGenerator a = new SeasonGenerator();
            a.Show();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
            RatingGenerator a = new RatingGenerator();
            a.Show();
        }

        private void MenuItemNewAnime_Click(object sender, RoutedEventArgs e) {
            AddNewAnime addNewAnime = new AddNewAnime();
            addNewAnime.ShowDialog();
        }
    }
}
