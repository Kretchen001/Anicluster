using Anicluster.Database.Services;
using Anicluster.Dialogs;
using Modells.Anime;
using System.Collections.ObjectModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Anicluster.ViewModels {

    /// <summary>
    /// Gemeinsames ViewModel zum Anlegen und Bearbeiten eines Animes.
    /// Die Eingaben werden direkt in den übergebenen <see cref="Anime"/> geschrieben,
    /// gespeichert wird erst über <see cref="Save"/>.
    /// </summary>
    public class AnimeEditorViewModel : PropertyBaseChange {

        private readonly IAnimeService _animeService;
        private readonly ITagService _tagService;
        private readonly IDialogService _dialogService;
        private readonly Anime _anime;
        private bool _isModified;

        /// <summary><c>true</c> = neuer Anime wird angelegt, <c>false</c> = vorhandener wird bearbeitet</summary>
        public bool IsNewAnime { get; }

        /// <summary>Wurde seit dem Öffnen (bzw. dem letzten Speichern) etwas geändert?</summary>
        public bool IsModified => this._isModified;

        /// <summary>Wird ausgelöst, wenn der Editor geschlossen werden soll.</summary>
        public event EventHandler? CloseRequested;

        /// <param name="anime">Der zu bearbeitende Anime, <c>null</c> legt einen neuen an</param>
        public AnimeEditorViewModel(IAnimeService animeService, ITagService tagService, IDialogService dialogService, Anime? anime = null) {
            this._animeService = animeService;
            this._tagService = tagService;
            this._dialogService = dialogService;
            this.IsNewAnime = anime is null;
            this._anime = anime ?? new Anime();
            this.AnimeChoices = this.LoadAnimeChoices();
        }

        #region Properties
        public string Name {
            get => this._anime.Name;
            set => this.SetAnimeValue(this._anime.Name, value, x => this._anime.Name = x);
        }

        public string? OriginalName {
            get => this._anime.OriginalName;
            set => this.SetAnimeValue(this._anime.OriginalName, value, x => this._anime.OriginalName = x);
        }

        public string? Url {
            get => this._anime.Url;
            set => this.SetAnimeValue(this._anime.Url, value, x => this._anime.Url = x);
        }

        public bool Favorite {
            get => this._anime.Favorite;
            set => this.SetAnimeValue(this._anime.Favorite, value, x => this._anime.Favorite = x);
        }

        public Tier Tier {
            get => this._anime.Tier;
            set => this.SetAnimeValue(this._anime.Tier, value, x => this._anime.Tier = x);
        }

        public string? RecommendedFrom {
            get => this._anime.RecommendedFrom;
            set => this.SetAnimeValue(this._anime.RecommendedFrom, value, x => this._anime.RecommendedFrom = x);
        }

        /// <summary>Kommentar zum Anime, ein leerer Text wird als <c>null</c> gespeichert.</summary>
        public string? Comment {
            get => this._anime.Comment;
            set => this.SetAnimeValue(this._anime.Comment, string.IsNullOrEmpty(value) ? null : value, x => this._anime.Comment = x);
        }

        public State State {
            get => this._anime.Status.State;
            set {
                if (this.SetAnimeValue(this._anime.Status.State, value, x => this._anime.Status.State = x)) {
                    this.OnPropertyChanged(nameof(this.IsRatingEnabled));
                    this.OnPropertyChanged(nameof(this.IsWishlist));
                }
            }
        }

        /// <summary>Kommentar zum Status, ein leerer Text wird als <c>null</c> gespeichert.</summary>
        public string? StatusComment {
            get => this._anime.Status.Comment;
            set => this.SetAnimeValue(this._anime.Status.Comment, string.IsNullOrEmpty(value) ? null : value, x => this._anime.Status.Comment = x);
        }

        public string? Author {
            get => this._anime.MediaInfo.Author;
            set => this.SetAnimeValue(this._anime.MediaInfo.Author, value, x => this._anime.MediaInfo.Author = x);
        }

        public string? Producer {
            get => this._anime.MediaInfo.Producer;
            set => this.SetAnimeValue(this._anime.MediaInfo.Producer, value, x => this._anime.MediaInfo.Producer = x);
        }

        public string? Publisher {
            get => this._anime.MediaInfo.Publisher;
            set => this.SetAnimeValue(this._anime.MediaInfo.Publisher, value, x => this._anime.MediaInfo.Publisher = x);
        }

        public int Predecessor {
            get => this._anime.Predecessor;
            set => this.SetAnimeValue(this._anime.Predecessor, value, x => this._anime.Predecessor = x);
        }

        public int Successor {
            get => this._anime.Successor;
            set => this.SetAnimeValue(this._anime.Successor, value, x => this._anime.Successor = x);
        }

        public int Related {
            get => this._anime.Related;
            set => this.SetAnimeValue(this._anime.Related, value, x => this._anime.Related = x);
        }

        /// <summary>Alle anderen Animes (plus Eintrag "keiner") zur Auswahl von Vorgänger, Nachfolger und Verwandt</summary>
        public ObservableCollection<AnimeChoice> AnimeChoices { get; }

        public IReadOnlyList<Tag> Tags => this._anime.Tags;

        public bool HasTags => this._anime.Tags.Count > 0;

        public int SeasonCount => this._anime.Seasons.Count;

        public int OvaCount => this._anime.Ovas.Count;
        #endregion

        #region Rating
        /// <summary>Bei Wunschlisten-Animes kann noch nicht bewertet werden.</summary>
        public bool IsRatingEnabled => this.State != State.Wishlist;

        public bool IsWishlist => this.State == State.Wishlist;

        public bool IsRated => this._anime.Rating.IsRated;

        /// <summary>Gesamtbewertung für die Fortschrittsanzeige (0, wenn noch nicht bewertet)</summary>
        public int RatingGeneral => this.IsRated ? this._anime.Rating.General : 0;

        public string RatingText => this.IsRated ? this._anime.Rating.General.ToString() : "n/a";
        #endregion

        #region Save buttons
        /// <summary>Überschrift des Editors</summary>
        public string ModeText => this.IsNewAnime ? "Neuer Anime" : "Anime bearbeiten";

        /// <summary>Der Speichern-Button wird bei neuen Animes immer, sonst erst nach einer Änderung angezeigt.</summary>
        public bool CanSave => this.IsNewAnime || this.IsModified;

        public string SaveText => this.IsNewAnime ? "Hinzufügen" : "Änderungen speichern";

        public string CancelText => this.IsNewAnime ? "Abbrechen" : (this.IsModified ? "Änderungen verwerfen" : "Schließen");
        #endregion

        #region Actions
        public void SetTier(Tier tier) {
            this.Tier = tier;
        }

        public void ToggleFavorite() {
            this.Favorite = !this.Favorite;
        }

        /// <summary>Erzeugt aus dem Namen einen Anime-Planet-Link.</summary>
        public void GenerateUrl() {
            this.Url = $"https://www.anime-planet.com/anime/{WebUtility.UrlEncode(this.Name.Replace(" ", "-"))}";
        }

        public void EditRating() {
            RatingEditorViewModel editor = new RatingEditorViewModel(this._dialogService, this._anime.Rating);
            if (this._dialogService.ShowDialog(editor) == true && editor.Result is not null) {
                this._anime.Rating = editor.Result;
                this.OnPropertyChanged(nameof(this.IsRated));
                this.OnPropertyChanged(nameof(this.RatingGeneral));
                this.OnPropertyChanged(nameof(this.RatingText));
                this.MarkAsModified();
            }
        }

        public void EditSeasons() {
            SeasonEditorViewModel editor = new SeasonEditorViewModel(this._dialogService, this._anime.Seasons);
            if (this._dialogService.ShowDialog(editor) == true) {
                this._anime.Seasons = editor.Result;
                this.OnPropertyChanged(nameof(this.SeasonCount));
                this.MarkAsModified();
            }
        }

        public void EditOvas() {
            VideoAnimationEditorViewModel editor = new VideoAnimationEditorViewModel(this._anime.Ovas);
            if (this._dialogService.ShowDialog(editor) == true) {
                this._anime.Ovas = editor.Result;
                this.OnPropertyChanged(nameof(this.OvaCount));
                this.MarkAsModified();
            }
        }

        public void EditTags() {
            TagSelectionViewModel editor = new TagSelectionViewModel(this._tagService, this._anime.Tags);
            if (this._dialogService.ShowDialog(editor) == true) {
                this._anime.Tags = editor.Result;
                this.OnPropertyChanged(nameof(this.Tags));
                this.OnPropertyChanged(nameof(this.HasTags));
                this.MarkAsModified();
            }
        }

        public void EditPublishingTime() {
            PublishingTimeEditorViewModel editor = new PublishingTimeEditorViewModel(this._dialogService, this._anime.MediaInfo.PublishingTime);
            if (this._dialogService.ShowDialog(editor) == true && editor.Result is not null) {
                this._anime.MediaInfo.PublishingTime = editor.Result;
                this.MarkAsModified();
            }
        }

        /// <summary>
        /// Speichert den Anime. Ein neuer Anime wird eingefügt und der Editor danach geschlossen,
        /// ein vorhandener wird aktualisiert und bleibt geöffnet.
        /// </summary>
        public void Save() {
            if (string.IsNullOrWhiteSpace(this.Name)) {
                this._dialogService.ShowMessage("Pflichtfelder sind leider nicht ausgefüllt!", string.Empty, MessageBoxImage.Warning);
                return;
            }
            bool saved = this.IsNewAnime
                ? this._animeService.Insert(this._anime)
                : this._animeService.UpdateAnime(this._anime);
            if (!saved) {
                this._dialogService.ShowMessage("Fehler beim Speichern!\nBitte beim Log aufbewahren und Programmierer informieren.", string.Empty, MessageBoxImage.Error);
                return;
            }
            if (this.IsNewAnime) {
                this.Cancel();
            }
            else {
                this.ResetModified();
            }
        }

        /// <summary>Schließt den Editor, nicht gespeicherte Änderungen gehen verloren.</summary>
        public void Cancel() {
            this.CloseRequested?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        /// <summary>
        /// Schreibt den neuen Wert in den Anime, benachrichtigt die View und merkt sich die Änderung.
        /// </summary>
        /// <returns><c>true</c>, wenn sich der Wert geändert hat.</returns>
        private bool SetAnimeValue<T>(T currentValue, T newValue, Action<T> assign, [CallerMemberName] string? propertyName = null) {
            if (EqualityComparer<T>.Default.Equals(currentValue, newValue)) {
                return false;
            }
            assign(newValue);
            this.OnPropertyChanged(propertyName);
            this.MarkAsModified();
            return true;
        }

        private void MarkAsModified() {
            if (!this._isModified) {
                this._isModified = true;
                this.NotifyModifiedChanged();
            }
        }

        private void ResetModified() {
            this._isModified = false;
            this.NotifyModifiedChanged();
        }

        private void NotifyModifiedChanged() {
            this.OnPropertyChanged(nameof(this.IsModified));
            this.OnPropertyChanged(nameof(this.CanSave));
            this.OnPropertyChanged(nameof(this.CancelText));
        }

        private ObservableCollection<AnimeChoice> LoadAnimeChoices() {
            ObservableCollection<AnimeChoice> choices = [new AnimeChoice(-1, "(keiner)")];
            foreach (AnimeListEntry entry in this._animeService.SelectAnimeListEntries()) {
                if (entry.Id != this._anime.Id) {
                    choices.Add(new AnimeChoice(entry.Id, entry.Name));
                }
            }
            return choices;
        }
    }
}
