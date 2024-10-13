using Anicluster.dialogs;
using Anicluster.userControls;
using Modells.Anime.SoundRating;
using Modells.Anime;
using System.Windows;
using UC = Anicluster.userControls;

namespace Anicluster.windows {
    /// <summary>
    /// Interaktionslogik für RatingGenerator.xaml
    /// </summary>
    public partial class RatingGenerator : Window {

        public List<UcMusicPiece> MusicPiecesO = [];
        public List<UcMusicPiece> MusicPiecesE = [];
        public List<UC.Syncro> Syncros = [];
        public string? CommentAcoustic {  get; set; }

        public int History { get; set; } = 0;
        public int Animation { get; set; } = 0;
        public int SpecialEffects { get; set; } = 0;
        public int Soundtrack { get; set; } = 0;

        public RatingGenerator() {
            InitializeComponent();
            DataContext = this;
        }

        public Rating GenerateRating() {
            List<MusicPiece> o = [];
            foreach (UcMusicPiece piece in MusicPiecesO) {
                o.Add(piece.MusicPiece);
            }
            List<MusicPiece> e = [];
            foreach (UcMusicPiece piece in MusicPiecesE) {
                e.Add(piece.MusicPiece);
            }
            List<Modells.Anime.SoundRating.Syncro> s = [];
            foreach (UC.Syncro piece in Syncros) {
                s.Add(piece.NewSyncro);
            }
            return new Rating(
                story: History,
                animation: Animation,
                specialEffects: SpecialEffects,
                acoustic: new Acoustic(
                    opening: o,
                    ending: e,
                    soundtrack: Soundtrack,
                    syncros: s,
                    comment: CommentAcoustic
                    ),
                isRated: true
                );
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

        private void ProgBarHistory_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e) {
            if (e.Delta > 0) {
                History = Math.Min(History + 1, 100);
            }
            else {
                History = Math.Max(History - 1, 0);
            }
            ProgBarHistory.Value = History;
            LbHistory.Content = History;
        }
        private void ProgBarHistory_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            NumberInputDialog inputDialog = new NumberInputDialog();
            if (inputDialog.ShowDialog() == true) {
                History = inputDialog.Result;
                ProgBarHistory.Value = inputDialog.Result;
                LbHistory.Content = inputDialog.Result;
            }
        }

        private void ProgBarAnimation_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e) {
            if (e.Delta > 0) {
                Animation = Math.Min(Animation + 1, 100);
            }
            else {
                Animation = Math.Max(Animation - 1, 0);
            }
            ProgBarAnimation.Value = Animation;
            LbAnimation.Content = Animation;
        }
        private void ProgBarAnimation_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            NumberInputDialog inputDialog = new NumberInputDialog();
            if (inputDialog.ShowDialog() == true) {
                Animation = inputDialog.Result;
                ProgBarAnimation.Value = inputDialog.Result;
                LbAnimation.Content = inputDialog.Result;
            }
        }

        private void ProgBarSpecialEffects_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e) {
            if (e.Delta > 0) {
                SpecialEffects = Math.Min(SpecialEffects + 1, 100);
            }
            else {
                SpecialEffects = Math.Max(SpecialEffects - 1, 0);
            }
            ProgBarSpecialEffects.Value = SpecialEffects;
            LbSpecialEffects.Content = SpecialEffects;
        }
        private void ProgBarSpecialEffects_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            NumberInputDialog inputDialog = new NumberInputDialog();
            if (inputDialog.ShowDialog() == true) {
                SpecialEffects = inputDialog.Result;
                ProgBarSpecialEffects.Value = inputDialog.Result;
                LbSpecialEffects.Content = inputDialog.Result;
            }
        }

        private void ProgBarSoundtrack_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e) {
            if (e.Delta > 0) {
                Soundtrack = Math.Min(Soundtrack + 1, 100);
            }
            else {
                Soundtrack = Math.Max(Soundtrack - 1, 0);
            }
            ProgBarSoundtrack.Value = Soundtrack;
            LbSoundtrack.Content = Soundtrack;
        }
        private void ProgBarSoundtrack_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            NumberInputDialog inputDialog = new NumberInputDialog();
            if (inputDialog.ShowDialog() == true) {
                Soundtrack = inputDialog.Result;
                ProgBarSoundtrack.Value = inputDialog.Result;
                LbSoundtrack.Content = inputDialog.Result;
            }
        }
    }
}
