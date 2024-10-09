using Anicluster.dialogs;
using Modells.Anime.SoundRating;
using System.Windows.Controls;

namespace Anicluster.userControls {
    /// <summary>
    /// Interaktionslogik für MusicPiece.xaml
    /// </summary>
    public partial class UcMusicPiece : UserControl {

        public MusicPiece MusicPiece = new MusicPiece();

        public UcMusicPiece(MusicPieceType musicPieceType) {
            MusicPiece.Type = musicPieceType;
            DataContext = MusicPiece;
            MusicPiece.Comment = "lorem";
            InitializeComponent();
            ProgBarGeneral.Value = 0;
        }

        private void ProgBarGeneral_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e) {
            if (e.Delta > 0) {
                MusicPiece.General = Math.Min(MusicPiece.General + 1, 100);
            }
            else {
                MusicPiece.General = Math.Max(MusicPiece.General - 1, 0);
            }
            ProgBarGeneral.Value = MusicPiece.General; // Update ProgressBar
        }

        private void ProgBarGeneral_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            NumberInputDialog inputDialog = new NumberInputDialog();
            if (inputDialog.ShowDialog() == true) {
                MusicPiece.General = inputDialog.Result;
                ProgBarGeneral.Value = inputDialog.Result;
            }
        }
    }
}
