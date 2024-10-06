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
    }
}
