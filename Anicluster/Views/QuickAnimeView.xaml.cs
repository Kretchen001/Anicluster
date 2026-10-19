using Anicluster.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Anicluster.Views {

    /// <summary>
    /// Dialog zum schnellen Anlegen eines Animes (Name, Favorit, Tier).
    /// </summary>
    public partial class QuickAnimeView : UserControl {

        private QuickAnimeViewModel? ViewModel => this.DataContext as QuickAnimeViewModel;

        public QuickAnimeView() {
            this.InitializeComponent();
            // Der Name wird zuerst eingegeben
            this.Loaded += (sender, e) => this.TBName.Focus();
        }

        private void TBName_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                this.ViewModel?.Save();
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.Save();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.Cancel();
        }
    }
}
