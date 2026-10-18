using Anicluster.Dialogs;
using Modells.Anime.SoundRating;

namespace Anicluster.ViewModels {

    /// <summary>
    /// Bearbeitungszustand eines Musikstücks (Opening, Ending oder Soundtrack) im Bewertungsdialog.
    /// </summary>
    public class MusicPieceViewModel : PropertyBaseChange {

        private readonly int _id;
        private string? _name;
        private string? _comment;

        public MusicPieceType Type { get; }
        public ScoreViewModel Score { get; }

        public string? Name {
            get => this._name;
            set => this.SetProperty(ref this._name, value);
        }

        public string? Comment {
            get => this._comment;
            set => this.SetProperty(ref this._comment, value);
        }

        /// <summary>Wird ausgelöst, wenn der Eintrag entfernt werden soll. Das Entfernen übernimmt der Besitzer der Liste.</summary>
        public event EventHandler? RemoveRequested;

        /// <param name="model">Vorhandenes Musikstück, <c>null</c> für einen neuen Eintrag</param>
        public MusicPieceViewModel(IDialogService dialogService, MusicPieceType type, MusicPiece? model = null) {
            this.Type = model?.Type ?? type;
            this._id = model?.Id ?? 0;
            this._name = model?.Name;
            this._comment = model is null ? string.Empty : model.Comment;
            // -1 bedeutet "noch nicht bewertet" und wird als 0 dargestellt
            this.Score = new ScoreViewModel(dialogService, Math.Max(model?.General ?? 0, 0));
        }

        public void Remove() {
            this.RemoveRequested?.Invoke(this, EventArgs.Empty);
        }

        public MusicPiece ToModel() {
            return new MusicPiece(this._id, this.Type, this.Name, this.Comment, this.Score.Value);
        }
    }
}
