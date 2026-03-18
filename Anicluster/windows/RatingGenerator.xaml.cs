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
            this.InitializeComponent();
            if (rating is not null) {
                this.History = rating.Story;
                this.Animation = rating.Animation;
                this.SpecialEffects = rating.SpecialEffects;
                this.Soundtrack = rating.Acoustic.Soundtrack;
                rating.Acoustic.Opening.ForEach(x => this.MusicPiecesO.Add(new UcMusicPiece(x.Type, x)));
                rating.Acoustic.Ending.ForEach(x => this.MusicPiecesE.Add(new UcMusicPiece(x.Type, x)));
                rating.Acoustic.Syncro.ForEach(x => this.Syncros.Add(new UC.Syncro(x)));
                this.MusicPiecesO.ForEach(x => this.StackPanelOpening.Children.Add(x));
                this.MusicPiecesE.ForEach(x => this.StackPanelEnding.Children.Add(x));
                this.Syncros.ForEach(x => this.StackPanelSyncro.Children.Add(x));
            }
            this.DataContext = this;
        }

        public Rating GenerateRating() {
            List<MusicPiece> o = [];
            foreach (UcMusicPiece piece in this.MusicPiecesO) {
                o.Add(piece.MusicPiece);
            }
            List<MusicPiece> e = [];
            foreach (UcMusicPiece piece in this.MusicPiecesE) {
                e.Add(piece.MusicPiece);
            }
            List<Modells.Anime.SoundRating.Syncro> s = [];
            foreach (UC.Syncro piece in this.Syncros) {
                s.Add(piece.NewSyncro);
            }
            return new Rating(
                story: this.History,
                animation: this.Animation,
                specialEffects: this.SpecialEffects,
                acoustic: new Acoustic(
                    opening: o,
                    ending: e,
                    soundtrack: this.Soundtrack,
                    syncros: s,
                    comment: this.CommentAcoustic
                    ),
                isRated: true
                );
        }

        private void BtnSoundtrackAdd_Click(object sender, RoutedEventArgs e) {
            UcMusicPiece piece = new UcMusicPiece(MusicPieceType.Other);
            piece.RemoveThisUc += this.RemoveSoundtrack!;
            this.SoundtrackPieces.Add(piece);
            this.StackPanelSoundtracks.Children.Add(piece);
        }

        private void RemoveSoundtrack(object sender, EventArgs e) {
            this.SoundtrackPieces.Remove((UcMusicPiece)sender);
            this.StackPanelSoundtracks.Children.Remove((UcMusicPiece)sender);
        }

        private void BtnSoundtrackCalculate_Click(object sender, RoutedEventArgs e) {
            if (this.SoundtrackPieces.Count > 0) {
                this.Soundtrack = 0;
                foreach (UcMusicPiece x in this.SoundtrackPieces) {
                    this.Soundtrack += x.MusicPiece.General;
                }
                this.Soundtrack /= this.SoundtrackPieces.Count;
                this.LbSoundtrack.Content = this.Soundtrack;
                this.ProgBarSoundtrack.Value = this.Soundtrack;
                this.RatingDisplayFunction();
            }
            else {
                MessageBox.Show("Keine Soundtracks zum Berechnen eingetragen!");
            }
        }

        private void BtnOpeningAdd_Click(object sender, RoutedEventArgs e) {
            UcMusicPiece piece = new UcMusicPiece(MusicPieceType.Opening);
            piece.RemoveThisUc += this.RemoveOpening!;
            this.MusicPiecesO.Add(piece);
            this.StackPanelOpening.Children.Add(piece);
        }

        private void RemoveOpening(object sender, EventArgs e) {
            this.MusicPiecesO.Remove((UcMusicPiece)sender);
            this.StackPanelOpening.Children.Remove((UcMusicPiece)sender);
        }

        private void BtnEndingAdd_Click(object sender, RoutedEventArgs e) {
            UcMusicPiece piece = new UcMusicPiece(MusicPieceType.Ending);
            piece.RemoveThisUc += this.RemoveEnding!;
            this.MusicPiecesE.Add(piece);
            this.StackPanelEnding.Children.Add(piece);
        }

        private void RemoveEnding(object sender, EventArgs e) {
            this.MusicPiecesE.Remove((UcMusicPiece)sender);
            this.StackPanelEnding.Children.Remove((UcMusicPiece)sender);
        }

        private void BtnSyncroAdd_Click(object sender, RoutedEventArgs e) {
            UC.Syncro syncro = new UC.Syncro();
            syncro.RemoveThisSynco += this.RemoveSyncro!;
            this.Syncros.Add(syncro);
            this.StackPanelSyncro.Children.Add(syncro);
        }

        private void RemoveSyncro(object sender, EventArgs e) {
            this.Syncros.Remove((UC.Syncro)sender);
            this.StackPanelSyncro.Children.Remove((UC.Syncro)sender);
        }

        private void ProgBarHistory_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e) {
            if (e.Delta > 0) {
                this.History = Math.Min(this.History + 1, 100);
            }
            else {
                this.History = Math.Max(this.History - 1, 0);
            }
            this.ProgBarHistory.Value = this.History;
            this.LbHistory.Content = this.History;
        }
        private void ProgBarHistory_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            NumberInputDialog inputDialog = new NumberInputDialog(this.History);
            if (inputDialog.ShowDialog() == true) {
                this.History = inputDialog.Result;
                this.ProgBarHistory.Value = inputDialog.Result;
                this.LbHistory.Content = inputDialog.Result;
            }
        }

        private void ProgBarAnimation_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e) {
            if (e.Delta > 0) {
                this.Animation = Math.Min(this.Animation + 1, 100);
            }
            else {
                this.Animation = Math.Max(this.Animation - 1, 0);
            }
            this.ProgBarAnimation.Value = this.Animation;
            this.LbAnimation.Content = this.Animation;
        }
        private void ProgBarAnimation_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            NumberInputDialog inputDialog = new NumberInputDialog();
            if (inputDialog.ShowDialog() == true) {
                this.Animation = inputDialog.Result;
                this.ProgBarAnimation.Value = inputDialog.Result;
                this.LbAnimation.Content = inputDialog.Result;
            }
        }

        private void ProgBarSpecialEffects_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e) {
            if (e.Delta > 0) {
                this.SpecialEffects = Math.Min(this.SpecialEffects + 1, 100);
            }
            else {
                this.SpecialEffects = Math.Max(this.SpecialEffects - 1, 0);
            }
            this.ProgBarSpecialEffects.Value = this.SpecialEffects;
            this.LbSpecialEffects.Content = this.SpecialEffects;
        }
        private void ProgBarSpecialEffects_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            NumberInputDialog inputDialog = new NumberInputDialog(this.SpecialEffects);
            if (inputDialog.ShowDialog() == true) {
                this.SpecialEffects = inputDialog.Result;
                this.ProgBarSpecialEffects.Value = inputDialog.Result;
                this.LbSpecialEffects.Content = inputDialog.Result;
            }
        }

        private void ProgBarSoundtrack_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e) {
            if (e.Delta > 0) {
                this.Soundtrack = Math.Min(this.Soundtrack + 1, 100);
            }
            else {
                this.Soundtrack = Math.Max(this.Soundtrack - 1, 0);
            }
            this.ProgBarSoundtrack.Value = this.Soundtrack;
            this.LbSoundtrack.Content = this.Soundtrack;
            this.RatingDisplayFunction();
        }

        private void ProgBarSoundtrack_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            NumberInputDialog inputDialog = new NumberInputDialog(this.Soundtrack);
            if (inputDialog.ShowDialog() == true) {
                this.Soundtrack = inputDialog.Result;
                this.ProgBarSoundtrack.Value = inputDialog.Result;
                this.LbSoundtrack.Content = inputDialog.Result;
                this.RatingDisplayFunction();
            }
        }

        private void TabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
            this.RatingDisplayFunction();
        }

        private void RatingDisplayFunction() {
            if (this.TabItemAuswertung.IsSelected) {
                Rating temp = this.GenerateRating();
                this.FormulaInsert.Formula = $"\\frac{{{temp.Story} \\cdot 40 + {temp.Animation} \\cdot 25 + {temp.SpecialEffects} \\cdot 10 + {temp.Acoustic.General} \\cdot 25}}{{100}} = {temp.General}";
            }
            else if (this.TabItemBereiche.IsSelected) {
                Rating temp = this.GenerateRating();
                int tempOpening = 0, tempEnding = 0, tempSyncros = 0;
                foreach (UcMusicPiece x in this.MusicPiecesO) {
                    tempOpening += x.MusicPiece.General;
                }
                tempOpening /= this.MusicPiecesO.Count != 0 ? this.MusicPiecesO.Count : 1;
                foreach (UcMusicPiece x in this.MusicPiecesE) {
                    tempEnding += x.MusicPiece.General;
                }
                tempEnding /= this.MusicPiecesE.Count != 0 ? this.MusicPiecesE.Count : 1;
                foreach (UC.Syncro x in this.Syncros) {
                    tempOpening += x.NewSyncro.General;
                }
                tempOpening /= this.Syncros.Count != 0 ? this.Syncros.Count : 1;
                int c = 0;
                c += tempEnding != 0 ? 1 : 0;
                c += tempOpening != 0 ? 1 : 0;
                c += tempSyncros != 0 ? 1 : 0;
                c += this.Soundtrack != 0 ? 1 : 0;
                this.LbAkustikGesamt.Content = $"({this.Soundtrack} + {tempOpening} + {tempEnding} + {tempSyncros})/{c} = {temp.Acoustic.General}";
            }
        }

        private void MenuItemAceptRating_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void MenuItemHelp_Click(object sender, RoutedEventArgs e) {
            this.PopUpHelp.IsOpen = true;
        }

        private void PopUpHelp_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e) {
            this.PopUpHelp.IsOpen = false;
        }
    }
}
