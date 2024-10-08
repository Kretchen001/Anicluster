using Anicluster.userControls;
using Modells.Anime.SoundRating;
using System.Windows;
using AnimeSR = Modells.Anime.SoundRating;
using UC = Anicluster.userControls;

namespace Anicluster.windows {
    /// <summary>
    /// Interaktionslogik für RatingGenerator.xaml
    /// </summary>
    public partial class RatingGenerator : Window {

        public List<UcMusicPiece> MusicPiecesO = [];
        public List<UcMusicPiece> MusicPiecesE = [];
        public List<UC.Syncro> Syncros = [];

        public RatingGenerator() {
            InitializeComponent();
        }

        private void BtnOpeningAdd_Click(object sender, RoutedEventArgs e) {
            UcMusicPiece piece = new UcMusicPiece(MusicPieceType.Opening);
            MusicPiecesO.Add(piece);
            StackPanelOpening.Children.Add(piece);
        }

        private void BtnEndingAdd_Click(object sender, RoutedEventArgs e) {
            UcMusicPiece piece = new UcMusicPiece(MusicPieceType.Ending);
            MusicPiecesE.Add(piece);
            StackPanelEnding.Children.Add(piece);
        }

        private void BtnSyncorAdd_Click(object sender, RoutedEventArgs e) {
            UC.Syncro syncro = new UC.Syncro();
            Syncros.Add(syncro);
            StackPanelSyncro.Children.Add(syncro);
        }
    }
}
