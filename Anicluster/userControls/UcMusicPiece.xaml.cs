using Anicluster.dialogs;
using Modells.Anime.SoundRating;
using System.Windows;
using System.Windows.Controls;

namespace Anicluster.userControls {
    /// <summary>
    /// Interaktionslogik für MusicPiece.xaml
    /// </summary>
    public partial class UcMusicPiece : UserControl {

        public MusicPiece MusicPiece = new MusicPiece();

        public UcMusicPiece(MusicPieceType musicPieceType, MusicPiece? musicPiece = null) {
            if (musicPiece is not null) {
                this.MusicPiece = musicPiece;
            }
            else {
                this.MusicPiece.Type = musicPieceType;
                this.MusicPiece.Comment = "";
            }
            this.DataContext = this.MusicPiece;
            this.InitializeComponent();
            this.ProgBarGeneral.Value = this.MusicPiece.General != -1 ? this.MusicPiece.General : 0;
        }

        private void ProgBarGeneral_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e) {
            if (e.Delta > 0) {
                this.MusicPiece.General = Math.Min(this.MusicPiece.General + 1, 100);
            }
            else {
                this.MusicPiece.General = Math.Max(this.MusicPiece.General - 1, 0);
            }
            this.ProgBarGeneral.Value = this.MusicPiece.General; // Update ProgressBar
        }

        private void ProgBarGeneral_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            NumberInputDialog inputDialog = new NumberInputDialog();
            if (inputDialog.ShowDialog() == true) {
                this.MusicPiece.General = inputDialog.Result;
                this.ProgBarGeneral.Value = inputDialog.Result;
            }
        }

        public event EventHandler RemoveThisUc;
        private void MenuItem_Delete_Click(object sender, RoutedEventArgs e) {
            RemoveThisUc.Invoke(this, e);
        }

        private void BtnRemoveEntry_Click(object sender, RoutedEventArgs e) {
            RemoveThisUc(this, e);
        }
    }
}
