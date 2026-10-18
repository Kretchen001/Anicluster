using Anicluster.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Anicluster.Views {

    /// <summary>
    /// Eingabe einer Zahl zwischen 0 und 100.
    /// </summary>
    public partial class NumberInputView : UserControl {

        public NumberInputView() {
            this.InitializeComponent();
            this.Loaded += (sender, e) => this.TBInputNumber.Focus();
        }

        private void TBInputNumber_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                (this.DataContext as NumberInputViewModel)?.Confirm();
            }
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e) {
            (this.DataContext as NumberInputViewModel)?.Confirm();
        }
    }
}
