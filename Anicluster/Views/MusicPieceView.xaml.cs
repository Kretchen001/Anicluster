using Anicluster.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Anicluster.Views {

    /// <summary>
    /// Ein Musikstück (Opening, Ending oder Soundtrack) im Bewertungsdialog.
    /// </summary>
    public partial class MusicPieceView : UserControl {

        public MusicPieceView() {
            this.InitializeComponent();
        }

        private void BtnRemoveEntry_Click(object sender, RoutedEventArgs e) {
            (this.DataContext as MusicPieceViewModel)?.Remove();
        }
    }
}
