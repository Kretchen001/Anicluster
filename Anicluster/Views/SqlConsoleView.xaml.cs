using Anicluster.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Anicluster.Views {

    /// <summary>
    /// Eingabe und Ergebnisanzeige der SQL-Konsole.
    /// </summary>
    public partial class SqlConsoleView : UserControl {

        private SqlConsoleViewModel? ViewModel => this.DataContext as SqlConsoleViewModel;

        public SqlConsoleView() {
            this.InitializeComponent();
            this.Loaded += (sender, e) => this.TBSql.Focus();
        }

        private void TBSql_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Control) {
                e.Handled = true;
                this.ViewModel?.Execute();
            }
        }

        private void BtnExecute_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.Execute();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.Close();
        }
    }
}
