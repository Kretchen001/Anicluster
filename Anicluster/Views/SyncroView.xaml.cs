using Anicluster.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Anicluster.Views {

    /// <summary>
    /// Eine Syncro im Bewertungsdialog.
    /// </summary>
    public partial class SyncroView : UserControl {

        public SyncroView() {
            this.InitializeComponent();
        }

        private void BtnRemoveEntry_Click(object sender, RoutedEventArgs e) {
            (this.DataContext as SyncroViewModel)?.Remove();
        }
    }
}
