using Anicluster.Metadata;
using Anicluster.Settings;
using Serilog;
using System.Collections.ObjectModel;

namespace Anicluster.ViewModels {

    /// <summary>
    /// Dialog "Metadaten laden": Der Benutzer wählt eine Quelle (AniList, Jikan oder Kitsu), sucht den Anime, sieht Cover, Beschreibung
    /// und Zeitraum des gewählten Treffers und bestimmt, was davon übernommen wird. Abgefragt wird nur auf Knopfdruck (und einmal
    /// beim Öffnen). Der Dialog schreibt selbst nichts, das Ergebnis steht nach dem Übernehmen in <see cref="Result"/>.
    /// </summary>
    public class MetadataImportViewModel : DialogViewModelBase {

        private readonly IMetadataClient _client;
        private readonly AppSettingsService _settingsService;
        private readonly MetadataImportContext _context;
        private CancellationTokenSource? _requestCts;
        private CancellationTokenSource? _coverCts;
        private int _selectionVersion;
        private bool _started;
        private MetadataProviderKind _provider;
        private string _searchText;
        private bool _isBusy;
        private string _statusText = string.Empty;
        private bool _isStatusError;
        private MetadataCandidateViewModel? _selectedCandidate;
        private byte[]? _coverData;
        private bool _isCoverLoading;
        private string _coverStatusText = string.Empty;
        private bool _applyCover;
        private bool _applyDescription;
        private bool _applyPeriod;

        public override string Title => "Metadaten laden";

        public override bool FillsOwnerWindow => true;

        public ObservableCollection<MetadataCandidateViewModel> Candidates { get; } = [];

        /// <summary>Die bestätigte Auswahl, <c>null</c> solange nichts übernommen wurde.</summary>
        public MetadataImportResult? Result { get; private set; }

        public MetadataImportViewModel(IMetadataClient client, AppSettingsService settingsService, MetadataImportContext context) {
            this._client = client;
            this._settingsService = settingsService;
            this._context = context;
            this._provider = settingsService.Current.MetadataProvider;
            this._searchText = context.SearchText;
        }

        #region Quelle und Suche
        /// <summary>Die Quelle für die nächste Abfrage. Die Wahl wird in den Einstellungen gemerkt (Standard ist AniList).</summary>
        public MetadataProviderKind Provider {
            get => this._provider;
            set {
                if (this.SetProperty(ref this._provider, value)) {
                    this._settingsService.Current.MetadataProvider = value;
                    this.CancelRunningRequests();
                    this.Candidates.Clear();
                    this.SelectedCandidate = null;
                    this.OnPropertyChanged(nameof(this.HasCandidates));
                    this.SetStatus($"{value.GetDisplayName()} ist gewählt. Mit „Suchen“ wird dort abgefragt.", false);
                }
            }
        }

        public string SearchText {
            get => this._searchText;
            set {
                if (this.SetProperty(ref this._searchText, value)) {
                    this.OnPropertyChanged(nameof(this.CanSearch));
                }
            }
        }

        public bool IsBusy {
            get => this._isBusy;
            private set {
                if (this.SetProperty(ref this._isBusy, value)) {
                    this.OnPropertyChanged(nameof(this.CanSearch));
                    this.OnPropertyChanged(nameof(this.CanApply));
                }
            }
        }

        public bool CanSearch => !this.IsBusy && this.SearchText.Trim().Length > 0;

        public string StatusText {
            get => this._statusText;
            private set => this.SetProperty(ref this._statusText, value);
        }

        /// <summary>Ist der Statustext eine Fehlermeldung? (die View färbt ihn dann entsprechend)</summary>
        public bool IsStatusError {
            get => this._isStatusError;
            private set => this.SetProperty(ref this._isStatusError, value);
        }

        /// <summary>
        /// Wird beim Öffnen des Dialogs einmal aufgerufen. Ist der Anime bei der gewählten Quelle schon verknüpft, wird dieser
        /// Eintrag direkt geladen, sonst nach dem Namen gesucht.
        /// </summary>
        public async Task StartAsync() {
            if (this._started) {
                return;
            }
            this._started = true;
            MetadataProviderKind provider = this.Provider;
            string? linkedId = this._context.GetLinkedId(provider);
            if (linkedId is not null) {
                CancellationToken token = this.BeginRequest();
                MetadataResult<MetadataCandidate> result = await this._client.GetByIdAsync(provider, linkedId, token);
                if (token.IsCancellationRequested) {
                    return;
                }
                this.IsBusy = false;
                if (result.IsSuccess && result.Value is not null) {
                    this.ShowCandidates([result.Value]);
                    this.SetStatus(
                        $"Der Anime ist schon mit diesem Eintrag bei {provider.GetDisplayName()} verknüpft. Mit „Suchen“ gibt es andere Treffer.",
                        false
                    );
                    return;
                }
                if (result.Status != MetadataStatus.NotFound) {
                    this.ShowFailure(result.Status, result.RetryAfterSeconds, provider);
                    return;
                }
            }
            if (this.CanSearch) {
                await this.SearchAsync();
            }
        }

        /// <summary>Sucht den Text bei der gewählten Quelle.</summary>
        public async Task SearchAsync() {
            string query = this.SearchText.Trim();
            if (query.Length == 0) {
                this.SetStatus("Bitte einen Namen zum Suchen eingeben.", true);
                return;
            }
            MetadataProviderKind provider = this.Provider;
            CancellationToken token = this.BeginRequest();
            MetadataResult<IReadOnlyList<MetadataCandidate>> result = await this._client.SearchAsync(provider, query, token);
            if (token.IsCancellationRequested) {
                return;
            }
            this.IsBusy = false;
            if (!result.IsSuccess || result.Value is null) {
                this.ShowFailure(result.Status, result.RetryAfterSeconds, provider);
                return;
            }
            this.ShowCandidates(result.Value);
            if (result.Value.Count == 0) {
                this.SetStatus("Keine Treffer. Ein englischer Titel oder die Romaji-Schreibweise passt oft besser.", false);
            }
            else {
                this.SetStatus($"{result.Value.Count} Treffer bei {provider.GetDisplayName()}. Wähle den passenden Eintrag.", false);
            }
        }

        /// <summary>Beendet laufende Abfragen und startet eine neue. Gibt deren Abbruch-Token zurück.</summary>
        private CancellationToken BeginRequest() {
            this._requestCts?.Cancel();
            this._requestCts = new CancellationTokenSource();
            this.IsBusy = true;
            this.SetStatus("Es wird abgefragt …", false);
            return this._requestCts.Token;
        }

        private void CancelRunningRequests() {
            this._requestCts?.Cancel();
            this._coverCts?.Cancel();
            this.IsBusy = false;
            this.IsCoverLoading = false;
        }

        private void ShowCandidates(IReadOnlyList<MetadataCandidate> candidates) {
            this.Candidates.Clear();
            foreach (MetadataCandidate candidate in candidates) {
                this.Candidates.Add(new MetadataCandidateViewModel(candidate));
            }
            this.SelectedCandidate = this.Candidates.FirstOrDefault();
            this.OnPropertyChanged(nameof(this.HasCandidates));
        }

        public bool HasCandidates => this.Candidates.Count > 0;

        /// <summary>Übersetzt den Fehler einer Abfrage in eine verständliche Meldung (die Einzelheiten stehen im Log).</summary>
        private void ShowFailure(MetadataStatus status, int? retryAfterSeconds, MetadataProviderKind provider) {
            string name = provider.GetDisplayName();
            string message = status switch {
                MetadataStatus.Offline => $"{name} ist nicht erreichbar. Bitte die Internetverbindung prüfen.",
                MetadataStatus.Timeout => $"{name} antwortet nicht (Zeitüberschreitung). Bitte später erneut versuchen.",
                MetadataStatus.RateLimited => $"{name} bremst gerade (zu viele Anfragen). Bitte in {retryAfterSeconds ?? 60} Sekunden erneut versuchen.",
                MetadataStatus.NotFound => "Dazu wurde nichts gefunden.",
                MetadataStatus.ServiceError => $"{name} meldet einen Fehler. Bitte später erneut versuchen oder eine andere Quelle wählen.",
                _ => string.Empty
            };
            if (message.Length > 0) {
                this.SetStatus(message, true);
            }
        }

        private void SetStatus(string text, bool isError) {
            this.StatusText = text;
            this.IsStatusError = isError;
        }
        #endregion

        #region Auswahl und Vorschau
        public MetadataCandidateViewModel? SelectedCandidate {
            get => this._selectedCandidate;
            set {
                if (this.SetProperty(ref this._selectedCandidate, value)) {
                    this.OnSelectionChanged();
                }
            }
        }

        public bool HasSelection => this.SelectedCandidate is not null;

        public string PreviewTitle => this.SelectedCandidate?.Title ?? string.Empty;

        public string PreviewSubtitle => this.SelectedCandidate?.Subtitle ?? string.Empty;

        public bool HasPreviewSubtitle => this.PreviewSubtitle.Length > 0;

        public string PreviewDescription => this.SelectedCandidate?.Candidate.Description ?? "Dieser Eintrag hat keine Beschreibung.";

        public string StartText => DescribeDate(this.SelectedCandidate?.Candidate.Start);

        public string EndText => DescribeDate(this.SelectedCandidate?.Candidate.End);

        /// <summary>Das heruntergeladene Cover des gewählten Treffers, <c>null</c> solange es lädt oder fehlt</summary>
        public byte[]? CoverData {
            get => this._coverData;
            private set => this.SetProperty(ref this._coverData, value);
        }

        public bool IsCoverLoading {
            get => this._isCoverLoading;
            private set {
                if (this.SetProperty(ref this._isCoverLoading, value)) {
                    this.OnPropertyChanged(nameof(this.CanApply));
                }
            }
        }

        /// <summary>Hinweis anstelle des Covers ("Cover wird geladen …", "Kein Cover vorhanden")</summary>
        public string CoverStatusText {
            get => this._coverStatusText;
            private set => this.SetProperty(ref this._coverStatusText, value);
        }

        private void OnSelectionChanged() {
            this._coverCts?.Cancel();
            this._selectionVersion += 1;
            MetadataCandidate? candidate = this.SelectedCandidate?.Candidate;
            this.CoverData = null;
            this.IsCoverLoading = false;
            this.CoverStatusText = candidate is null || candidate.CoverUrl is not null ? string.Empty : "Dieser Eintrag hat kein Cover.";

            // Vorschläge: Nur Leeres wird vorgeschlagen, Vorhandenes bleibt, wenn der Benutzer es nicht selbst anhakt
            this.ApplyCover = false;
            this.ApplyDescription = this.CanApplyDescription && !this._context.HasDescription;
            this.ApplyPeriod = this.CanApplyPeriod && !this._context.HasStartDate && !this._context.HasEndDate;

            this.OnPropertyChanged(nameof(this.HasSelection));
            this.OnPropertyChanged(nameof(this.PreviewTitle));
            this.OnPropertyChanged(nameof(this.PreviewSubtitle));
            this.OnPropertyChanged(nameof(this.HasPreviewSubtitle));
            this.OnPropertyChanged(nameof(this.PreviewDescription));
            this.OnPropertyChanged(nameof(this.StartText));
            this.OnPropertyChanged(nameof(this.EndText));
            this.OnPropertyChanged(nameof(this.CanApplyDescription));
            this.OnPropertyChanged(nameof(this.CanApplyPeriod));
            this.OnPropertyChanged(nameof(this.DescriptionNote));
            this.OnPropertyChanged(nameof(this.HasDescriptionNote));
            this.OnPropertyChanged(nameof(this.PeriodNote));
            this.OnPropertyChanged(nameof(this.HasPeriodNote));
            this.OnPropertyChanged(nameof(this.CanApply));

            if (candidate?.CoverUrl is string coverUrl) {
                // Fire-and-forget: Die Methode fängt ihre Fehler selbst ab
                _ = this.LoadCoverAsync(coverUrl, this._selectionVersion);
            }
        }

        private async Task LoadCoverAsync(string url, int selectionVersion) {
            try {
                this._coverCts = new CancellationTokenSource();
                CancellationToken token = this._coverCts.Token;
                this.IsCoverLoading = true;
                this.CoverStatusText = "Cover wird geladen …";
                MetadataResult<byte[]> result = await this._client.DownloadCoverAsync(url, token);
                if (token.IsCancellationRequested || selectionVersion != this._selectionVersion) {
                    return;
                }
                this.IsCoverLoading = false;
                if (result.IsSuccess && result.Value is not null) {
                    this.CoverData = result.Value;
                    this.CoverStatusText = string.Empty;
                    this.ApplyCover = !this._context.HasImage;
                }
                else {
                    this.CoverStatusText = "Das Cover konnte nicht geladen werden.";
                }
                this.OnPropertyChanged(nameof(this.CanApplyCover));
                this.OnPropertyChanged(nameof(this.CanApply));
            }
            catch (Exception ex) {
                // Letzte Absicherung, weil niemand auf diese Aufgabe wartet
                Log.Error(ex, "Das Cover konnte nicht geladen werden.");
                if (selectionVersion == this._selectionVersion) {
                    this.IsCoverLoading = false;
                    this.CoverStatusText = "Das Cover konnte nicht geladen werden.";
                }
            }
        }

        private static string DescribeDate(PartialDate? date) {
            if (date is null || date.ToDisplayText().Length == 0) {
                return "unbekannt";
            }
            return date.IsComplete ? date.ToDisplayText() : $"{date.ToDisplayText()} (unvollständig)";
        }
        #endregion

        #region Übernehmen
        public bool ApplyCover {
            get => this._applyCover;
            set {
                if (this.SetProperty(ref this._applyCover, value)) {
                    this.OnPropertyChanged(nameof(this.CanApply));
                }
            }
        }

        public bool ApplyDescription {
            get => this._applyDescription;
            set {
                if (this.SetProperty(ref this._applyDescription, value)) {
                    this.OnPropertyChanged(nameof(this.CanApply));
                }
            }
        }

        public bool ApplyPeriod {
            get => this._applyPeriod;
            set {
                if (this.SetProperty(ref this._applyPeriod, value)) {
                    this.OnPropertyChanged(nameof(this.CanApply));
                }
            }
        }

        public bool CanApplyCover => this.CoverData is not null;

        public bool CanApplyDescription => this.SelectedCandidate?.Candidate.Description is not null;

        /// <summary>Nur ein vollständiges Datum kann übernommen werden.</summary>
        public bool CanApplyPeriod => this.SelectedCandidate?.Candidate is { } candidate
            && (candidate.Start?.IsComplete == true || candidate.End?.IsComplete == true);

        public string CoverNote => this._context.HasImage ? "Es ist schon ein Bild vorhanden, es würde ersetzt." : string.Empty;

        public bool HasCoverNote => this.CoverNote.Length > 0;

        public string DescriptionNote => this._context.HasDescription ? "Die vorhandene Beschreibung würde ersetzt." : string.Empty;

        public bool HasDescriptionNote => this.DescriptionNote.Length > 0;

        /// <summary>Hinweis zum Zeitraum: unvollständige Angaben bleiben leer, ein vorhandener Zeitraum würde überschrieben.</summary>
        public string PeriodNote {
            get {
                MetadataCandidate? candidate = this.SelectedCandidate?.Candidate;
                if (candidate is null) {
                    return string.Empty;
                }
                List<string> notes = [];
                if (!this.CanApplyPeriod) {
                    notes.Add("Der Zeitraum ist unvollständig und wird nicht übernommen.");
                }
                else {
                    if (candidate.Start?.IsComplete == false || candidate.End?.IsComplete == false) {
                        notes.Add("Unvollständige Angaben bleiben leer.");
                    }
                    if (this._context.HasStartDate || this._context.HasEndDate) {
                        notes.Add("Ein vorhandenes Datum würde ersetzt.");
                    }
                }
                return string.Join(" ", notes);
            }
        }

        public bool HasPeriodNote => this.PeriodNote.Length > 0;

        /// <summary>Gibt es etwas, das jetzt übernommen werden kann?</summary>
        public bool CanApply => this.SelectedCandidate is not null && !this.IsBusy && !this.IsCoverLoading
            && ((this.ApplyCover && this.CanApplyCover)
                || (this.ApplyDescription && this.CanApplyDescription)
                || (this.ApplyPeriod && this.CanApplyPeriod));

        public void Apply() {
            MetadataCandidate? candidate = this.SelectedCandidate?.Candidate;
            if (candidate is null || !this.CanApply) {
                return;
            }
            bool withPeriod = this.ApplyPeriod && this.CanApplyPeriod;
            this.Result = new MetadataImportResult(
                candidate.Provider,
                candidate.ExternalId,
                candidate.MyAnimeListId,
                this.ApplyCover && this.CanApplyCover ? this.CoverData : null,
                this.ApplyDescription && this.CanApplyDescription ? candidate.Description : null,
                withPeriod ? candidate.Start?.ToDateTime() : null,
                withPeriod ? candidate.End?.ToDateTime() : null
            );
            this.CancelRunningRequests();
            this.RequestClose(true);
        }

        public void Cancel() {
            this.CancelRunningRequests();
            this.RequestClose(false);
        }
        #endregion
    }
}
