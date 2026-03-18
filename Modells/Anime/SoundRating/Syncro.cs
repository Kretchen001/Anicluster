using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Modells.Anime.SoundRating {

    public class Syncro : ValueComparableObject, INotifyPropertyChanged {

        private int _general = -1;
        private SyncroLanuage _language = SyncroLanuage.jp;
        private bool _isAssessed = false;
        private string? _comment;

        public int General {
            get => this._general;
            set => this.SetField(ref this._general, value);
        }

        public SyncroLanuage Language {
            get => this._language;
            set => this.SetField(ref this._language, value);
        }

        /// <summary>this Syncro should be recognized in the rating?</summary>
        public bool IsAssessed {
            get => this._isAssessed;
            set => this.SetField(ref this._isAssessed, value);
        }

        public string? Comment {
            get => this._comment;
            set => this.SetField(ref this._comment, value);
        }

        public Syncro() { }

        public Syncro(int general, SyncroLanuage language, bool isAssessed, string? comment) {
            this._general = general;
            this._language = language;
            this._isAssessed = isAssessed;
            this._comment = comment;
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