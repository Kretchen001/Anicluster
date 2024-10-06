using Modells.Anime.SoundRating;
using System.Windows.Controls;

namespace Anicluster.userControls {
    /// <summary>
    /// Interaktionslogik für MusicPiece.xaml
    /// </summary>
    public partial class UcMusicPiece : UserControl {

        MusicPiece musicPiece = new MusicPiece();

        public UcMusicPiece(MusicPieceType musicPieceType) {
            musicPiece.Type = musicPieceType;
            InitializeComponent();
        }
    }
}
