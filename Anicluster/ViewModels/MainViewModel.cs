using Anicluster.Backup;
using Anicluster.Database;
using Anicluster.Database.Services;
using Anicluster.Dialogs;
using Anicluster.Metadata;
using Anicluster.Settings;
using Anicluster.Theming;
using Modells.Anime;
using Serilog;
using System.Reflection;
using System.Windows;

namespace Anicluster.ViewModels {

    /// <summary>
    /// ViewModel des Hauptfensters: Seitenleiste mit den Bereichen Bibliothek, Tags und Einstellungen.
    /// Der Editor ersetzt beim Anlegen oder Bearbeiten eines Animes die Seite der Bibliothek.
    /// </summary>
    public class MainViewModel : PropertyBaseChange {

        private readonly IAnimeService _animeService;
        private readonly ITagService _tagService;
        private readonly IDialogService _dialogService;
        private readonly DatabaseManager _databaseManager;
        private readonly IMetadataClient _metadataClient;
        private readonly AppSettingsService _settingsService;
        private readonly SettingsViewModel _settingsPage;
        private object _currentPage;
        private NavigationTarget _navigation = NavigationTarget.Library;

        public string AppTitle { get; }

        public string VersionText { get; }

        public LibraryViewModel Library { get; }

        /// <summary>Die angezeigte Seite, die View dazu liefern die DataTemplates.</summary>
        public object CurrentPage {
            get => this._currentPage;
            private set {
                object previous = this._currentPage;
                if (this.SetProperty(ref this._currentPage, value) && previous is IDisposable disposable) {
                    disposable.Dispose(); // z. B. stoppt das Tag-Netzwerk seine Simulation
                }
            }
        }

        /// <summary>Der markierte Bereich der Seitenleiste. Das Setzen wechselt die Seite.</summary>
        public NavigationTarget Navigation {
            get => this._navigation;
            set => this.NavigateTo(value);
        }

        public MainViewModel(
            IAnimeService animeService,
            ITagService tagService,
            IDialogService dialogService,
            ThemeService themeService,
            AppSettingsService settingsService,
            DatabaseManager databaseManager,
            IMetadataClient metadataClient,
            IBackupService backupService
        ) {
            this._databaseManager = databaseManager;
            this._metadataClient = metadataClient;
            this._settingsService = settingsService;
            this._animeService = animeService;
            this._tagService = tagService;
            this._dialogService = dialogService;
            this.AppTitle = GetAssemblyTitle();
            this.VersionText = GetVersionText();
            this._settingsPage = new SettingsViewModel(themeService, settingsService, dialogService, backupService);
            this.Library = new LibraryViewModel(animeService, settingsService);
            this._settingsPage.DataImported += (sender, e) => this.Library.Refresh();
            this.Library.EditRequested += (sender, item) => this.EditAnime(item.Id);
            this.Library.DeleteRequested += (sender, item) => this.DeleteAnime(item.Id, item.Name);
            this._currentPage = this.Library;
        }

        /// <summary>Öffnet die versteckte SQL-Konsole.</summary>
        public void OpenSqlConsole() {
            this._dialogService.ShowDialog(new SqlConsoleViewModel(this._databaseManager, this._dialogService));
            // Die Konsole kann Daten verändert haben, daher die Bibliothek neu laden
            this.Library.Refresh();
        }

        /// <summary>Wechselt den Bereich. Offene Änderungen im Editor werden vorher abgefragt.</summary>
        public void NavigateTo(NavigationTarget target) {
            if (!this.ConfirmLeaveEditor()) {
                this.OnPropertyChanged(nameof(this.Navigation)); // Markierung der Seitenleiste zurücksetzen
                return;
            }
            this.DetachEditor();
            this._navigation = target;
            switch (target) {
                case NavigationTarget.Tags:
                    this.CurrentPage = new TagManagerViewModel(this._tagService, this._dialogService);
                    break;
                case NavigationTarget.TagNetwork:
                    this.CurrentPage = this.CreateTagNetwork();
                    break;
                case NavigationTarget.AnimeNetwork:
                    this.CurrentPage = this.CreateAnimeNetwork();
                    break;
                case NavigationTarget.TasteProfile:
                    this.CurrentPage = this.CreateTasteProfile();
                    break;
                case NavigationTarget.Timeline:
                    this.CurrentPage = this.CreateTimeline();
                    break;
                case NavigationTarget.Settings:
                    this.CurrentPage = this._settingsPage;
                    break;
                default:
                    this.Library.Refresh();
                    this.CurrentPage = this.Library;
                    break;
            }
            this.OnPropertyChanged(nameof(this.Navigation));
        }

        /// <summary>Öffnet den Editor für einen neuen Anime.</summary>
        public void CreateNewAnime() {
            if (this.CurrentPage is AnimeEditorViewModel) {
                return;
            }
            this.OpenEditor(null);
        }

        /// <summary>Öffnet den Editor für einen vorhandenen Anime.</summary>
        public void EditAnime(int animeId) {
            Anime? anime = this._animeService.SelectAnimeByX(animeId);
            if (anime is null) {
                Log.Error("EditAnime load with id: {AnimeId}", animeId);
                this._dialogService.ShowMessage("Fehler beim Laden eines Animes\nBitte Datenbank sichern!", string.Empty, MessageBoxImage.Error);
                return;
            }
            this.OpenEditor(anime);
        }

        /// <summary>Wechselt zur Bibliothek und zeigt dort nur Animes mit den übergebenen Tags.</summary>
        public void ShowLibraryWithTags(IReadOnlyList<TagFilterItem> tags, TagMatchMode mode, LibraryStatusFilter? status = null) {
            this.NavigateTo(NavigationTarget.Library);
            this.Library.SetTagFilter(tags, mode);
            if (status.HasValue) {
                this.Library.StatusFilter = status.Value;
            }
        }

        /// <summary>Wechselt zur Bibliothek und sucht dort nach dem Text (zum Beispiel dem Namen eines Animes).</summary>
        public void ShowLibraryWithSearch(string searchText) {
            this.NavigateTo(NavigationTarget.Library);
            this.Library.SearchText = searchText;
        }

        /// <summary>Zeigt den Dialog zum schnellen Anlegen und lädt danach die Liste neu.</summary>
        public void CreateQuickAnime() {
            this._dialogService.ShowDialog(new QuickAnimeViewModel(this._animeService, this._dialogService));
            this.Library.Refresh();
        }

        /// <summary>Löscht den Anime nach Rückfrage samt allen zugehörigen Daten und lädt die Liste neu.</summary>
        public void DeleteAnime(int animeId, string animeName) {
            bool confirmed = this._dialogService.Confirm(
                $"Soll der Anime '{animeName}' mit allen Daten (Bewertung, Staffeln, Tags, ...) wirklich gelöscht werden?\nDas kann nicht rückgängig gemacht werden.",
                "Anime löschen"
            );
            if (!confirmed) {
                return;
            }
            if (!this._animeService.DeleteAnime(animeId)) {
                this._dialogService.ShowMessage("Fehler beim Löschen!\nBitte beim Log aufbewahren und Programmierer informieren.", string.Empty, MessageBoxImage.Error);
                return;
            }
            this.Library.Refresh();
        }

        private TagNetworkViewModel CreateTagNetwork() {
            TagNetworkViewModel network = new TagNetworkViewModel(this._animeService, this._tagService);
            network.EditRequested += (sender, item) => this.EditAnime(item.Id);
            network.ShowInLibraryRequested += (sender, e) => this.ShowLibraryWithTags(e.Tags, e.Mode);
            return network;
        }

        private AnimeNetworkViewModel CreateAnimeNetwork() {
            AnimeNetworkViewModel network = new AnimeNetworkViewModel(this._animeService, this._tagService, this._settingsService);
            network.EditRequested += (sender, animeId) => this.EditAnime(animeId);
            network.SearchInLibraryRequested += (sender, searchText) => this.ShowLibraryWithSearch(searchText);
            return network;
        }

        private TasteProfileViewModel CreateTasteProfile() {
            TasteProfileViewModel profile = new TasteProfileViewModel(this._animeService, this._tagService);
            profile.EditRequested += (sender, item) => this.EditAnime(item.Id);
            profile.ShowInLibraryRequested += (sender, e) => this.ShowLibraryWithTags(e.Tags, e.Mode, e.Status);
            return profile;
        }

        private TimelineViewModel CreateTimeline() {
            TimelineViewModel timeline = new TimelineViewModel(this._animeService, this._tagService);
            timeline.EditRequested += (sender, animeId) => this.EditAnime(animeId);
            return timeline;
        }

        private void OpenEditor(Anime? anime) {
            this.DetachEditor();
            AnimeEditorViewModel editor = new AnimeEditorViewModel(
                this._animeService,
                this._tagService,
                this._dialogService,
                this._metadataClient,
                this._settingsService,
                anime
            );
            editor.CloseRequested += this.OnEditorCloseRequested;
            this._navigation = NavigationTarget.Library;
            this.CurrentPage = editor;
            this.OnPropertyChanged(nameof(this.Navigation));
        }

        private void OnEditorCloseRequested(object? sender, EventArgs e) {
            this.DetachEditor();
            this._navigation = NavigationTarget.Library;
            this.Library.Refresh();
            this.CurrentPage = this.Library;
            this.OnPropertyChanged(nameof(this.Navigation));
        }

        private void DetachEditor() {
            if (this.CurrentPage is AnimeEditorViewModel editor) {
                editor.CloseRequested -= this.OnEditorCloseRequested;
            }
        }

        /// <summary>Fragt bei ungespeicherten Änderungen im Editor nach, ob sie verworfen werden dürfen.</summary>
        private bool ConfirmLeaveEditor() {
            if (this.CurrentPage is AnimeEditorViewModel { IsModified: true }) {
                return this._dialogService.Confirm("Die Änderungen sind noch nicht gespeichert und gehen verloren.\nTrotzdem wechseln?", "Änderungen verwerfen");
            }
            return true;
        }

        private static string GetAssemblyTitle() {
            return Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? "Anicluster";
        }

        private static string GetVersionText() {
            Version? version = Assembly.GetExecutingAssembly().GetName().Version;
            return version is null ? string.Empty : $"Version {version.Major}.{version.Minor}.{version.Build}";
        }
    }
}
