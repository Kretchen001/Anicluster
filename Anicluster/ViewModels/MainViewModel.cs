using Anicluster.Database.Services;
using Anicluster.Dialogs;
using Modells.Anime;
using Serilog;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;

namespace Anicluster.ViewModels {

    /// <summary>
    /// ViewModel des Hauptfensters: zeigt die Anime-Liste oder (anstelle der Liste) den Editor
    /// zum Anlegen bzw. Bearbeiten eines Animes.
    /// </summary>
    public class MainViewModel : PropertyBaseChange {

        private readonly IAnimeService _animeService;
        private readonly ITagService _tagService;
        private readonly IDialogService _dialogService;
        private AnimeEditorViewModel? _editor;
        private AnimeSortField? _sortField;
        private bool _sortAscending = true;

        public string AppTitle { get; }

        public ObservableCollection<AnimeListItemViewModel> Animes { get; } = [];

        /// <summary>Der geöffnete Editor, <c>null</c> solange die Liste angezeigt wird.</summary>
        public AnimeEditorViewModel? Editor {
            get => this._editor;
            private set {
                if (this.SetProperty(ref this._editor, value)) {
                    this.OnPropertyChanged(nameof(this.IsListVisible));
                }
            }
        }

        public bool IsListVisible => this.Editor is null;

        public MainViewModel(IAnimeService animeService, ITagService tagService, IDialogService dialogService) {
            this._animeService = animeService;
            this._tagService = tagService;
            this._dialogService = dialogService;
            this.AppTitle = BuildAppTitle();
            this.RefreshAnimes();
        }

        /// <summary>Öffnet den Editor für einen neuen Anime.</summary>
        public void CreateNewAnime() {
            if (this.Editor is not null) {
                return;
            }
            this.OpenEditor(null);
        }

        /// <summary>Öffnet den Editor für einen vorhandenen Anime.</summary>
        public void EditAnime(int animeId) {
            Anime? anime = this._animeService.SelectAnimeByX(animeId);
            if (anime is null) {
                Log.Error($"EditAnime load with id: {animeId}");
                this._dialogService.ShowMessage("Fehler beim Laden eines Animes\nBitte Datenbank sichern!", string.Empty, MessageBoxImage.Error);
                return;
            }
            this.OpenEditor(anime);
        }

        /// <summary>Zeigt den Dialog zum schnellen Anlegen und lädt danach die Liste neu.</summary>
        public void CreateQuickAnime() {
            this._dialogService.ShowDialog(new QuickAnimeViewModel(this._animeService, this._dialogService));
            this.RefreshAnimes();
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
            this.RefreshAnimes();
        }

        public void OpenTagManager() {
            this._dialogService.ShowDialog(new TagManagerViewModel(this._tagService, this._dialogService));
        }

        /// <summary>
        /// Sortiert die Liste. Wird dasselbe Kriterium erneut gewählt, wechselt die Richtung.
        /// </summary>
        public void SortAnimes(AnimeSortField field) {
            this._sortAscending = this._sortField != field || !this._sortAscending;
            this._sortField = field;
            this.ApplySort();
        }

        /// <summary>Lädt die Anime-Liste neu und behält die zuletzt gewählte Sortierung bei.</summary>
        public void RefreshAnimes() {
            this.Animes.Clear();
            foreach (AnimeListEntry entry in this._animeService.SelectAnimeListEntries()) {
                AnimeListItemViewModel item = new AnimeListItemViewModel(entry);
                item.EditRequested += this.OnItemEditRequested;
                item.DeleteRequested += this.OnItemDeleteRequested;
                this.Animes.Add(item);
            }
            this.ApplySort();
        }

        private void OpenEditor(Anime? anime) {
            AnimeEditorViewModel editor = new AnimeEditorViewModel(this._animeService, this._tagService, this._dialogService, anime);
            editor.CloseRequested += this.OnEditorCloseRequested;
            this.Editor = editor;
        }

        private void OnEditorCloseRequested(object? sender, EventArgs e) {
            if (this.Editor is not null) {
                this.Editor.CloseRequested -= this.OnEditorCloseRequested;
            }
            this.Editor = null;
            this.RefreshAnimes();
        }

        private void OnItemEditRequested(object? sender, EventArgs e) {
            if (sender is AnimeListItemViewModel item) {
                this.EditAnime(item.Id);
            }
        }

        private void OnItemDeleteRequested(object? sender, EventArgs e) {
            if (sender is AnimeListItemViewModel item) {
                this.DeleteAnime(item.Id, item.Name);
            }
        }

        private void ApplySort() {
            if (this._sortField is null) {
                return;
            }
            IEnumerable<AnimeListItemViewModel> sorted = this._sortField switch {
                AnimeSortField.Id => this.Animes.OrderBy(x => x.Id),
                AnimeSortField.Name => this.Animes.OrderBy(x => x.Name, StringComparer.CurrentCultureIgnoreCase),
                AnimeSortField.State => this.Animes.OrderBy(x => x.State),
                AnimeSortField.Favorite => this.Animes.OrderBy(x => x.IsFavorite),
                AnimeSortField.Tier => this.Animes.OrderBy(x => x.Tier),
                _ => this.Animes
            };
            if (!this._sortAscending) {
                sorted = sorted.Reverse();
            }
            List<AnimeListItemViewModel> ordered = sorted.ToList();
            this.Animes.Clear();
            foreach (AnimeListItemViewModel item in ordered) {
                this.Animes.Add(item);
            }
        }

        private static string BuildAppTitle() {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string title = assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? "Anicluster";
            Version? version = assembly.GetName().Version;
            return version is null
                ? title
                : $"{title} - Version: {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }
    }
}
