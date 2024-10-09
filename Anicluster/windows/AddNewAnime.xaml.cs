using Modells.Anime;
using System.Windows;
using System.Windows.Input;

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
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (e.ButtonState == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void EditableTextBox_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                TBName.Visibility = Visibility.Collapsed;
                LbName.Visibility = Visibility.Visible;
                LbName.Content = TBName.Text;
            }
        }

        private void LbName_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            TBName.Visibility = Visibility.Visible;
            LbName.Visibility = Visibility.Collapsed;
            TBName.Focus();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }
    }
}
