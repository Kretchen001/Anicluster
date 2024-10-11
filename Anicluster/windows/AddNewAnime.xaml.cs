using Modells.Anime;
using System.Windows;
using System.Windows.Controls;
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

        private void RBtn_Checked(object sender, RoutedEventArgs e) {
            if (sender is RadioButton radioBtn) {
                List<RadioButton> radioButtons = ((StackPanel)radioBtn.Parent).Children.OfType<RadioButton>().ToList();
                int selectedIndex = radioButtons.IndexOf(radioBtn);
                if (selectedIndex >= 0 && selectedIndex < Enum.GetValues(typeof(State)).Length) {
                    NewAnime.Status.State = (State)selectedIndex;
                }
                if (NewAnime.Status.State.Equals(State.Wishlist)) {
                    BtnRatingGenerator.IsEnabled = false;
                    BtnTierS.IsEnabled = false;
                    BtnTierA.IsEnabled = false;
                    BtnTierB.IsEnabled = false;
                    BtnTierC.IsEnabled = false;
                    BtnTierD.IsEnabled = false;
                    BtnTierE.IsEnabled = false;
                }
            }
        }
    }
}
