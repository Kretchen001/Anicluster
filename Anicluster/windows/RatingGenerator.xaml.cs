using Anicluster.userControls;
using Modells.Anime.SoundRating;
using System.Windows;

namespace Anicluster.windows {
    /// <summary>
    /// Interaktionslogik für RatingGenerator.xaml
    /// </summary>
    public partial class RatingGenerator : Window {

        List<UcMusicPiece> musicPiecesO = [];
        List<UcMusicPiece> musicPiecesE = [];

        public RatingGenerator() {
            InitializeComponent();
        }

        private void btnOpeningAdd_Click(object sender, RoutedEventArgs e) {
            UcMusicPiece piece = new UcMusicPiece(MusicPieceType.Opening);
            musicPiecesO.Add(piece);
            StackPanelOpening.Children.Add(piece);
        }

        private void btnEndingAdd_Click(object sender, RoutedEventArgs e) {
            UcMusicPiece piece = new UcMusicPiece(MusicPieceType.Ending);
            musicPiecesE.Add(piece);
            StackPanelEnding.Children.Add(piece);
        }
    }
}
