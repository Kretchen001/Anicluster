using Modells.Anime.SoundRating;
using System.Windows.Controls;

namespace Anicluster.userControls {
    /// <summary>
    /// Interaktionslogik für MusicPiece.xaml
    /// </summary>
    public partial class UcMusicPiece : UserControl {

        public MusicPiece musicPiece = new MusicPiece();

        public UcMusicPiece(MusicPieceType musicPieceType) {
            musicPiece.Type = musicPieceType;
            DataContext = musicPiece;
            musicPiece.Comment = "lorem";
            InitializeComponent();
        }
    }
}
