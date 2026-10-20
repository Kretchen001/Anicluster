using Anicluster.Dialogs;
using Modells.Anime.SoundRating;

namespace Anicluster.ViewModels {

    /// <summary>
    /// Bearbeitungszustand einer Syncro im Bewertungsdialog.
    /// </summary>
    public class SyncroViewModel : PropertyBaseChange {

        private static readonly Dictionary<SyncroLanuage, string> TranslationOfLanguage = new Dictionary<SyncroLanuage, string>() {
            { SyncroLanuage.en, "Englisch" },
            { SyncroLanuage.jp, "Japanisch" },
            { SyncroLanuage.de, "Deutsch" },
            { SyncroLanuage.es, "Spanisch" },
            { SyncroLanuage.ru, "Russisch" },
            { SyncroLanuage.ko, "Koreanisch" },
            { SyncroLanuage.ch, "Chinesisch" },
        };

        private SyncroLanuage _language;
        private bool _isAssessed;
        private string? _comment;

        /// <summary>Auswahlliste für die Sprache (Schlüssel = Enum, Wert = deutscher Anzeigetext)</summary>
        public IEnumerable<KeyValuePair<SyncroLanuage, string>> Languages => TranslationOfLanguage;

        public ScoreViewModel Score { get; }

        public SyncroLanuage Language {
            get => this._language;
            set => this.SetProperty(ref this._language, value);
        }

        /// <summary>Soll diese Syncro in der Bewertung berücksichtigt werden?</summary>
        public bool IsAssessed {
            get => this._isAssessed;
            set => this.SetProperty(ref this._isAssessed, value);
        }

        public string? Comment {
            get => this._comment;
            set => this.SetProperty(ref this._comment, value);
        }

        /// <summary>Wird ausgelöst, wenn der Eintrag entfernt werden soll. Das Entfernen übernimmt der Besitzer der Liste.</summary>
        public event EventHandler? RemoveRequested;

        /// <param name="model">Vorhandene Syncro, <c>null</c> für einen neuen Eintrag</param>
        public SyncroViewModel(IDialogService dialogService, Syncro? model = null) {
            this._language = model?.Language ?? SyncroLanuage.jp;
            this._isAssessed = model?.IsAssessed ?? false;
            this._comment = model?.Comment;
            // -1 bedeutet "noch nicht bewertet" und wird als 0 dargestellt
            this.Score = new ScoreViewModel(dialogService, Math.Max(model?.General ?? 0, 0));
        }

        public void Remove() {
            this.RemoveRequested?.Invoke(this, EventArgs.Empty);
        }

        public Syncro ToModel() {
            return new Syncro(this.Score.Value, this.Language, this.IsAssessed, this.Comment);
        }
    }
}
