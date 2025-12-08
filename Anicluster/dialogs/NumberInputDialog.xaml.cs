using System.Windows;
using System.Windows.Input;

namespace Anicluster.dialogs {
    /// <summary>
    /// Interaktionslogik für NumberInputDialog.xaml
    /// </summary>
    public partial class NumberInputDialog : Window {

        public int Result { get; private set; }
        
        public NumberInputDialog() {
            this.InitializeComponent();
            this.TBInputNumber.Focus();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) {
            this.CheckInput();
        }

        private void InputTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                this.CheckInput();
            }
        }

        private void CheckInput() {
            if (int.TryParse(this.TBInputNumber.Text, out int result) && result <= 100 && result >= 0) {
                this.Result = result;
                this.DialogResult = true;
                this.Close();
            }
            else {
                MessageBox.Show("Bitte geben Sie eine gültige Zahl unter 100 ein.");
            }
        }
    }
}
