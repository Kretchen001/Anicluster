using Anicluster.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Anicluster.Views {

    /// <summary>
    /// Eingabe einer Zahl zwischen 0 und 100.
    /// </summary>
    public partial class NumberInputView : UserControl {

        private NumberInputViewModel? ViewModel => this.DataContext as NumberInputViewModel;

        public NumberInputView() {
            this.InitializeComponent();
            this.Loaded += (sender, e) => this.TBInputNumber.Focus();
        }

        private void TBInputNumber_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                this.ViewModel?.Confirm();
            }
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.Confirm();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.Cancel();
        }
    }
}
