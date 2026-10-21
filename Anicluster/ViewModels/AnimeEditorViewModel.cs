using Anicluster.Database.Services;
using Anicluster.Dialogs;
using Anicluster.Imaging;
using Anicluster.Metadata;
using Anicluster.Settings;
using Modells.Anime;
using Serilog;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
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

        /// <summary>Größere Bilddateien werden abgelehnt, bevor sie vollständig eingelesen werden (Schutz vor Speicherverbrauch)</summary>
        private const long MaxImageFileSize = 25 * 1024 * 1024;

        /// <summary>Platzhalter-Datum des Modells für "noch nicht gesetzt"</summary>
        private static readonly DateTime UnsetDate = new DateTime(1, 1, 1);

        private readonly IAnimeService _animeService;
        private readonly ITagService _tagService;
        private readonly IDialogService _dialogService;
        private readonly IMetadataClient _metadataClient;
        private readonly AppSettingsService _settingsService;
        private readonly Anime _anime;
        private bool _isModified;

        /// <summary><c>true</c> = neuer Anime wird angelegt, <c>false</c> = vorhandener wird bearbeitet</summary>
        public bool IsNewAnime { get; }

        /// <summary>Wurde seit dem Öffnen (bzw. dem letzten Speichern) etwas geändert?</summary>
        public bool IsModified => this._isModified;

        /// <summary>Wird ausgelöst, wenn der Editor geschlossen werden soll.</summary>
        public event EventHandler? CloseRequested;

        /// <param name="anime">Der zu bearbeitende Anime, <c>null</c> legt einen neuen an</param>
        public AnimeEditorViewModel(
            IAnimeService animeService,
            ITagService tagService,
            IDialogService dialogService,
            IMetadataClient metadataClient,
            AppSettingsService settingsService,
            Anime? anime = null
        ) {
            this._animeService = animeService;
            this._tagService = tagService;
            this._dialogService = dialogService;
            this._metadataClient = metadataClient;
            this._settingsService = settingsService;
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

        /// <summary>Ausführliche Beschreibung, ein leerer Text wird als <c>null</c> gespeichert.</summary>
        public string? Description {
            get => this._anime.Description;
            set => this.SetAnimeValue(this._anime.Description, string.IsNullOrEmpty(value) ? null : value, x => this._anime.Description = x);
        }

        /// <summary>Bei welchen Online-Quellen der Anime verknüpft ist, leer ohne Verknüpfung</summary>
        public string MetadataLinkText {
            get {
                List<string> sources = [];
                if (this._anime.AniListId is not null) {
                    sources.Add("AniList");
                }
                if (this._anime.MyAnimeListId is not null) {
                    sources.Add("MyAnimeList");
                }
                if (this._anime.KitsuId is not null) {
                    sources.Add("Kitsu");
                }
                return sources.Count == 0 ? string.Empty : "Verknüpft mit: " + string.Join(", ", sources);
            }
        }

        public bool HasMetadataLink => this.MetadataLinkText.Length > 0;

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

        /// <summary>Das Bild in voller (verkleinerter) Größe, <c>null</c> ohne Bild</summary>
        public byte[]? ImageData => this._anime.Image?.Data;

        public bool HasImage => this._anime.Image is not null;

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

        /// <summary>Lässt den Benutzer eine Bilddatei wählen und übernimmt sie als Bild des Animes.</summary>
        public void SelectImage() {
            string? path = this._dialogService.PickImageFile();
            if (path is not null) {
                this.SetImageFromFile(path);
            }
        }

        /// <summary>Liest eine Bilddatei ein und übernimmt sie als Bild des Animes.</summary>
        public void SetImageFromFile(string path) {
            try {
                if (new FileInfo(path).Length > MaxImageFileSize) {
                    this._dialogService.ShowMessage(
                        "Die Bilddatei ist zu groß (höchstens 25 MB).",
                        string.Empty,
                        MessageBoxImage.Warning
                    );
                    return;
                }
                this.SetImage(File.ReadAllBytes(path));
            }
            catch (IOException ex) {
                this.ReportUnreadableImage(ex, path);
            }
            catch (UnauthorizedAccessException ex) {
                this.ReportUnreadableImage(ex, path);
            }
        }

        /// <summary>
        /// Übernimmt Bilddaten (beliebiges gängiges Format) als Bild des Animes. Das Bild wird verkleinert
        /// und gespeichert, sobald der Anime gespeichert wird.
        /// </summary>
        public void SetImage(byte[] source) {
            AnimeImage? image = ImageProcessor.TryCreate(source);
            if (image is null) {
                this._dialogService.ShowMessage(
                    "Dieses Bild kann nicht verwendet werden.\nUnterstützt werden z. B. JPG, PNG, BMP, GIF und TIFF.",
                    string.Empty,
                    MessageBoxImage.Warning
                );
                return;
            }
            this._anime.Image = image;
            this.NotifyImageChanged();
        }

        /// <summary>Entfernt das Bild des Animes.</summary>
        public void RemoveImage() {
            if (this._anime.Image is null) {
                return;
            }
            this._anime.Image = null;
            this.NotifyImageChanged();
        }

        /// <summary>Meldet dem Benutzer, dass die Zwischenablage kein Bild enthält.</summary>
        public void ReportNoImageInClipboard() {
            this._dialogService.ShowMessage("In der Zwischenablage befindet sich kein Bild.", string.Empty, MessageBoxImage.Information);
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

        /// <summary>
        /// Öffnet den Dialog, in dem Cover, Beschreibung und Veröffentlichungszeitraum aus einer Online-Quelle gewählt werden.
        /// Was bestätigt wird, landet im Editor und wird wie alles andere erst mit dem Speichern übernommen.
        /// </summary>
        public void LoadMetadata() {
            PublishingTime period = this._anime.MediaInfo.PublishingTime;
            MetadataImportContext context = new MetadataImportContext(
                string.IsNullOrWhiteSpace(this._anime.Name) ? this._anime.OriginalName ?? string.Empty : this._anime.Name,
                this.HasImage,
                !string.IsNullOrWhiteSpace(this._anime.Description),
                period.StartDate != UnsetDate,
                period.EndDate != UnsetDate,
                !string.IsNullOrWhiteSpace(this._anime.MediaInfo.Producer),
                !string.IsNullOrWhiteSpace(this._anime.MediaInfo.Publisher),
                !string.IsNullOrWhiteSpace(this._anime.MediaInfo.Author),
                !string.IsNullOrWhiteSpace(this._anime.OriginalName),
                this._anime.AniListId,
                this._anime.MyAnimeListId,
                this._anime.KitsuId
            );
            MetadataImportViewModel dialog = new MetadataImportViewModel(this._metadataClient, this._settingsService, context);
            if (this._dialogService.ShowDialog(dialog) == true && dialog.Result is not null) {
                this.ApplyMetadata(dialog.Result);
            }
        }

        private void ApplyMetadata(MetadataImportResult result) {
            if (result.CoverData is not null) {
                this.SetImage(result.CoverData);
            }
            if (result.Description is not null) {
                this.Description = result.Description;
            }
            this.ApplyPublishingPeriod(result.StartDate, result.EndDate);
            if (result.Producer is not null) {
                this.Producer = result.Producer;
            }
            if (result.Publisher is not null) {
                this.Publisher = result.Publisher;
            }
            if (result.Author is not null) {
                this.Author = result.Author;
            }
            if (result.OriginalName is not null) {
                this.OriginalName = result.OriginalName;
            }
            this.LinkMetadata(result);
        }

        /// <summary>Setzt Beginn und Ende des Gesamtzeitraums. Fehlende Angaben (<c>null</c>) lassen den vorhandenen Wert unverändert.</summary>
        private void ApplyPublishingPeriod(DateTime? start, DateTime? end) {
            PublishingTime period = this._anime.MediaInfo.PublishingTime;
            DateTime newStart = start ?? period.StartDate;
            DateTime newEnd = end ?? period.EndDate;
            if (newStart != UnsetDate && newEnd != UnsetDate && newEnd < newStart) {
                this._dialogService.ShowMessage(
                    "Der Zeitraum passt nicht zu dem vorhandenen Datum (das Ende läge vor dem Beginn) und wurde nicht übernommen.",
                    string.Empty,
                    MessageBoxImage.Warning
                );
                return;
            }
            if (newStart == period.StartDate && newEnd == period.EndDate) {
                return;
            }
            period.StartDate = newStart;
            period.EndDate = newEnd;
            this.MarkAsModified();
        }

        /// <summary>Merkt sich die Id des gewählten Eintrags, damit der Anime später ohne neue Suche abgeglichen werden kann.</summary>
        private void LinkMetadata(MetadataImportResult result) {
            if (int.TryParse(result.ExternalId, NumberStyles.None, CultureInfo.InvariantCulture, out int id)) {
                switch (result.Provider) {
                    case MetadataProviderKind.AniList:
                        this._anime.AniListId = id;
                        break;
                    case MetadataProviderKind.Jikan:
                        this._anime.MyAnimeListId = id;
                        break;
                    case MetadataProviderKind.Kitsu:
                        this._anime.KitsuId = id;
                        break;
                }
            }
            if (result.MyAnimeListId is int myAnimeListId) {
                this._anime.MyAnimeListId = myAnimeListId;
            }
            this.OnPropertyChanged(nameof(this.MetadataLinkText));
            this.OnPropertyChanged(nameof(this.HasMetadataLink));
            this.MarkAsModified();
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

        private void NotifyImageChanged() {
            this.OnPropertyChanged(nameof(this.ImageData));
            this.OnPropertyChanged(nameof(this.HasImage));
            this.MarkAsModified();
        }

        private void ReportUnreadableImage(Exception ex, string path) {
            Log.Error(ex, "Die Bilddatei '{ImagePath}' konnte nicht gelesen werden.", path);
            this._dialogService.ShowMessage("Die Bilddatei konnte nicht gelesen werden.", string.Empty, MessageBoxImage.Error);
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
