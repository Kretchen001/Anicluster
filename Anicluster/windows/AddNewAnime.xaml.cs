using Modells.Anime;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Anicluster.windows {
    /// <summary>
    /// Interaktionslogik für AddNewAnime.xaml
    /// </summary>
    public partial class AddNewAnime : Window {

        public Anime NewAnime { get; set; } = new Anime();

        public AddNewAnime() {
            InitializeComponent();
            this.DataContext = this;
            TBName.Visibility = Visibility.Visible;
            TBName.Focus();
            if (!NewAnime.Rating.IsRated) {
                LbGeneral.Content = "n/a";
            }
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (e.ButtonState == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void TBName_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                TBName.Visibility = Visibility.Collapsed;
                LbName.Visibility = Visibility.Visible;
                LbName.Content = TBName.Text;
            }
        }

        private void TBName_LostFocus(object sender, RoutedEventArgs e) {
            TBName.Visibility = Visibility.Collapsed;
            LbName.Visibility = Visibility.Visible;
            LbName.Content = TBName.Text;
        }

        private void LbName_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            TBName.Visibility = Visibility.Visible;
            LbName.Visibility = Visibility.Collapsed;
            TBName.Focus();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void StarPath_MouseDown(object sender, MouseButtonEventArgs e) {
            if (NewAnime.Favorite) {
                StarPath.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFAFAFAF")); ;
            }
            else {
                StarPath.Fill = Brushes.Yellow;
            }
            NewAnime.Favorite = !NewAnime.Favorite;
        }
    }
}
