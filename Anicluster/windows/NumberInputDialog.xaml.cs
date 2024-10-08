using System.Windows;
using System.Windows.Input;

namespace Anicluster.windows {
    /// <summary>
    /// Interaktionslogik für NumberInputDialog.xaml
    /// </summary>
    public partial class NumberInputDialog : Window {

        public int Result { get; private set; }
        
        public NumberInputDialog() {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) {
            CheckInput();
        }

        private void InputTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                CheckInput();
            }
        }

        private void CheckInput() {
            if (int.TryParse(InputTextBox.Text, out int result) && result <= 100 && result >= 0) {
                Result = result;
                DialogResult = true;
                Close();
            }
            else {
                MessageBox.Show("Bitte geben Sie eine gültige Zahl unter 100 ein.");
            }
        }
    }
}
