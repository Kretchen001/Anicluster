using Anicluster.userControls;
using Modells.Anime.SoundRating;
using System.Windows;

namespace Anicluster.windows {
    /// <summary>
    /// Interaktionslogik für RatingGenerator.xaml
    /// </summary>
    public partial class RatingGenerator : Window {

        public List<UcMusicPiece> musicPiecesO = [];
        public List<UcMusicPiece> musicPiecesE = [];

        public RatingGenerator() {
            InitializeComponent();
        }

        private void BtnOpeningAdd_Click(object sender, RoutedEventArgs e) {
            UcMusicPiece piece = new UcMusicPiece(MusicPieceType.Opening);
            musicPiecesO.Add(piece);
            StackPanelOpening.Children.Add(piece);
        }

        private void BtnEndingAdd_Click(object sender, RoutedEventArgs e) {
            UcMusicPiece piece = new UcMusicPiece(MusicPieceType.Ending);
            musicPiecesE.Add(piece);
            StackPanelEnding.Children.Add(piece);
        }
    }
}
