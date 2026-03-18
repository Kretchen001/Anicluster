using Anicluster.dialogs;
using Modells.Anime.SoundRating;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Anicluster.userControls {

    /// <summary>
    /// Interaktionslogik für MusicPiece.xaml
    /// </summary>
    public partial class UcMusicPiece : UserControl, INotifyPropertyChanged {

        private MusicPiece _MusicPiece = new MusicPiece();
        public MusicPiece MusicPiece {
            get {                 
                return this._MusicPiece; 
            }
            set {
                if (this._MusicPiece != value) {
                    this._MusicPiece = value;
                    this.OnPropertyChanged(nameof(this.MusicPiece));
                }
            }
        }

        public event EventHandler RemoveThisUc;
        public event PropertyChangedEventHandler? PropertyChanged;

        public UcMusicPiece(MusicPieceType musicPieceType, MusicPiece? musicPiece = null) {
            if (musicPiece is not null) {
                this.MusicPiece = musicPiece;
            }
            else {
                this.MusicPiece.Type = musicPieceType;
                this.MusicPiece.Comment = "";
            }
            this.InitializeComponent();
            this.MusicPiece.General = this.MusicPiece.General != -1 ? this.MusicPiece.General : 0;
            this.DataContext = this.MusicPiece;
        }

        private void ProgBarGeneral_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e) {
            if (e.Delta > 0) {
                this.MusicPiece.General = Math.Min(this.MusicPiece.General + 1, 100);
            }
            else {
                this.MusicPiece.General = Math.Max(this.MusicPiece.General - 1, 0);
            }
        }

        private void ProgBarGeneral_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            NumberInputDialog inputDialog = new NumberInputDialog(this.MusicPiece.General);
            if (inputDialog.ShowDialog() == true) {
                this.MusicPiece.General = inputDialog.Result;
            }
        }

        private void MenuItem_Delete_Click(object sender, RoutedEventArgs e) {
            RemoveThisUc.Invoke(this, e);
        }

        private void BtnRemoveEntry_Click(object sender, RoutedEventArgs e) {
            RemoveThisUc(this, e);
        }

        protected void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
