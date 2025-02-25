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
        public List<UcMusicPiece> SoundtrackPieces = [];
        public List<UC.Syncro> Syncros = [];
        public string? CommentAcoustic { get; set; }

        public int History { get; set; } = 0;
        public int Animation { get; set; } = 0;
        public int SpecialEffects { get; set; } = 0;
        public int Soundtrack { get; set; } = 0;

        public RatingGenerator(Rating? rating = null) {
            InitializeComponent();
            if (rating is not null) {
                History = rating.Story;
                Animation = rating.Animation;
                SpecialEffects = rating.SpecialEffects;
                Soundtrack = rating.Acoustic.Soundtrack;
                rating.Acoustic.Opening.ForEach(x => MusicPiecesO.Add(new UcMusicPiece(x.Type, x)));
                rating.Acoustic.Ending.ForEach(x => MusicPiecesE.Add(new UcMusicPiece(x.Type, x)));
                rating.Acoustic.Syncro.ForEach(x => Syncros.Add(new UC.Syncro(x)));
                MusicPiecesO.ForEach(x => StackPanelOpening.Children.Add(x));
                MusicPiecesE.ForEach(x => StackPanelEnding.Children.Add(x));
                Syncros.ForEach(x => StackPanelSyncro.Children.Add(x));
            }
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

        private void BtnSoundtrackAdd_Click(object sender, RoutedEventArgs e) {
            UcMusicPiece piece = new UcMusicPiece(MusicPieceType.Other);
            piece.RemoveThisUc += RemoveSoundtrack!;
            SoundtrackPieces.Add(piece);
            StackPanelSoundtracks.Children.Add(piece);
        }

        private void RemoveSoundtrack(object sender, EventArgs e) {
            SoundtrackPieces.Remove((UcMusicPiece)sender);
            StackPanelSoundtracks.Children.Remove((UcMusicPiece)sender);
        }

        private void BtnSoundtrackCalculate_Click(object sender, RoutedEventArgs e) {
            if (SoundtrackPieces.Count > 0) {
                Soundtrack = 0;
                foreach (UcMusicPiece x in SoundtrackPieces) {
                    Soundtrack += x.MusicPiece.General;
                }
                this.Soundtrack /= SoundtrackPieces.Count;
                LbSoundtrack.Content = Soundtrack;
                ProgBarSoundtrack.Value = Soundtrack;
                RatingDisplayFunction();
            }
            else {
                MessageBox.Show("Keine Soundtracks zum Berechnen eingetragen!");
            }
        }

        private void BtnOpeningAdd_Click(object sender, RoutedEventArgs e) {
            UcMusicPiece piece = new UcMusicPiece(MusicPieceType.Opening);
            piece.RemoveThisUc += RemoveOpening!;
            MusicPiecesO.Add(piece);
            StackPanelOpening.Children.Add(piece);
        }

        private void RemoveOpening(object sender, EventArgs e) {
            MusicPiecesO.Remove((UcMusicPiece)sender);
            StackPanelOpening.Children.Remove((UcMusicPiece)sender);
        }

        private void BtnEndingAdd_Click(object sender, RoutedEventArgs e) {
            UcMusicPiece piece = new UcMusicPiece(MusicPieceType.Ending);
            piece.RemoveThisUc += RemoveEnding!;
            MusicPiecesE.Add(piece);
            StackPanelEnding.Children.Add(piece);
        }

        private void RemoveEnding(object sender, EventArgs e) {
            MusicPiecesE.Remove((UcMusicPiece)sender);
            StackPanelEnding.Children.Remove((UcMusicPiece)sender);
        }

        private void BtnSyncroAdd_Click(object sender, RoutedEventArgs e) {
            UC.Syncro syncro = new UC.Syncro();
            syncro.RemoveThisSynco += RemoveSyncro!;
            Syncros.Add(syncro);
            StackPanelSyncro.Children.Add(syncro);
        }

        private void RemoveSyncro(object sender, EventArgs e) {
            Syncros.Remove((UC.Syncro)sender);
            StackPanelSyncro.Children.Remove((UC.Syncro)sender);
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
            RatingDisplayFunction();
        }

        private void ProgBarSoundtrack_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            NumberInputDialog inputDialog = new NumberInputDialog();
            if (inputDialog.ShowDialog() == true) {
                Soundtrack = inputDialog.Result;
                ProgBarSoundtrack.Value = inputDialog.Result;
                LbSoundtrack.Content = inputDialog.Result;
                RatingDisplayFunction();
            }
        }

        private void TabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
            RatingDisplayFunction();
        }

        private void RatingDisplayFunction() {
            if (TabItemAuswertung.IsSelected) {
                Rating temp = GenerateRating();
                FormulaInsert.Formula = $"\\frac{{{temp.Story} \\cdot 40 + {temp.Animation} \\cdot 25 + {temp.SpecialEffects} \\cdot 10 + {temp.Acoustic.General} \\cdot 25}}{{100}} = {temp.General}";
            }
            else if (TabItemBereiche.IsSelected) {
                Rating temp = GenerateRating();
                int tempOpening = 0, tempEnding = 0, tempSyncros = 0;
                foreach (UcMusicPiece x in MusicPiecesO) {
                    tempOpening += x.MusicPiece.General;
                }
                tempOpening /= MusicPiecesO.Count != 0 ? MusicPiecesO.Count : 1;
                foreach (UcMusicPiece x in MusicPiecesE) {
                    tempEnding += x.MusicPiece.General;
                }
                tempEnding /= MusicPiecesE.Count != 0 ? MusicPiecesE.Count : 1;
                foreach (UC.Syncro x in Syncros) {
                    tempOpening += x.NewSyncro.General;
                }
                tempOpening /= Syncros.Count != 0 ? Syncros.Count : 1;
                int c = 0;
                c += tempEnding != 0 ? 1 : 0;
                c += tempOpening != 0 ? 1 : 0;
                c += tempSyncros != 0 ? 1 : 0;
                c += Soundtrack != 0 ? 1 : 0;
                LbAkustikGesamt.Content = $"({tempOpening} + {tempEnding} + {tempSyncros} + {Soundtrack})/{c} = {temp.Acoustic.General}";
            }
        }

        private void MenuItemAceptRating_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void MenuItemHelp_Click(object sender, RoutedEventArgs e) {
            PopUpHelp.IsOpen = true;
        }

        private void PopUpHelp_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e) {
            PopUpHelp.IsOpen = false;
        }
    }
}
