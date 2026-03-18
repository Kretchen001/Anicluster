using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Modells.Anime.SoundRating {

    public class MusicPiece : ValueComparableObject, INotifyPropertyChanged {

        private int _id;
        private MusicPieceType _type;
        private string? _name;
        private string? _comment;
        private int _general = -1;

        public int Id {
            get => this._id;
            set => this.SetField(ref this._id, value);
        }

        public MusicPieceType Type {
            get => this._type;
            set => this.SetField(ref this._type, value);
        }

        public string? Name {
            get => this._name;
            set => this.SetField(ref this._name, value);
        }

        public string? Comment {
            get => this._comment;
            set => this.SetField(ref this._comment, value);
        }

        public int General {
            get => this._general;
            set => this.SetField(ref this._general, value);
        }

        public MusicPiece() { }

        public MusicPiece(int id, MusicPieceType type, string? name, string? comment, int general) {
            this._id = id;
            this._type = type;
            this._name = name;
            this._comment = comment;
            this._general = general;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
            if (Equals(field, value)) return false;
            field = value;
            this.OnPropertyChanged(propertyName!);
            return true;
        }

        protected void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}