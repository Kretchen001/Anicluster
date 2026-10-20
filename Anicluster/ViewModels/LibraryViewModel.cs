using Anicluster.Database.Services;
using Anicluster.Settings;
using Modells.Anime;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;

namespace Anicluster.ViewModels {

    /// <summary>
    /// Die Bibliothek: alle Animes mit Suche, Filter, Sortierung und umschaltbarer Darstellung (Karten, Tabelle oder Bilder).
    /// </summary>
    public class LibraryViewModel : PropertyBaseChange {

        private readonly IAnimeService _animeService;
        private readonly AppSettingsService _settingsService;
        private string _searchText = string.Empty;
        private LibraryStatusFilter _statusFilter = LibraryStatusFilter.All;
        private LibraryTierFilter _tierFilter = LibraryTierFilter.All;
        private TagMatchMode _tagMatchMode = TagMatchMode.All;
        private bool _onlyFavorites;
        private AnimeSortField _sortField = AnimeSortField.Name;
        private bool _sortAscending = true;
        private LibraryViewMode _viewMode;
        private int _visibleCount;

        public ObservableCollection<AnimeListItemViewModel> Animes { get; } = [];

        /// <summary>Tags, nach denen gefiltert wird (z. B. aus dem Tag-Netzwerk übernommen)</summary>
        public ObservableCollection<TagFilterItem> TagFilters { get; } = [];

        /// <summary>Gefilterte und sortierte Sicht auf <see cref="Animes"/>, daran binden die Views.</summary>
        public ICollectionView Items { get; }

        public IReadOnlyList<FilterOption<AnimeSortField>> SortOptions { get; } = [
            new FilterOption<AnimeSortField>(AnimeSortField.Name, "Name"),
            new FilterOption<AnimeSortField>(AnimeSortField.Tier, "Tier"),
            new FilterOption<AnimeSortField>(AnimeSortField.State, "Status"),
            new FilterOption<AnimeSortField>(AnimeSortField.Favorite, "Favorit"),
            new FilterOption<AnimeSortField>(AnimeSortField.Id, "Hinzugefügt")
        ];

        public IReadOnlyList<FilterOption<LibraryTierFilter>> TierOptions { get; } = [
            new FilterOption<LibraryTierFilter>(LibraryTierFilter.All, "Alle Tiers"),
            new FilterOption<LibraryTierFilter>(LibraryTierFilter.S, "Tier S"),
            new FilterOption<LibraryTierFilter>(LibraryTierFilter.A, "Tier A"),
            new FilterOption<LibraryTierFilter>(LibraryTierFilter.B, "Tier B"),
            new FilterOption<LibraryTierFilter>(LibraryTierFilter.C, "Tier C"),
            new FilterOption<LibraryTierFilter>(LibraryTierFilter.D, "Tier D"),
            new FilterOption<LibraryTierFilter>(LibraryTierFilter.E, "Tier E"),
            new FilterOption<LibraryTierFilter>(LibraryTierFilter.NotDefined, "Ohne Tier")
        ];

        /// <summary>Wird ausgelöst, wenn ein Anime zum Anzeigen/Bearbeiten geöffnet werden soll.</summary>
        public event EventHandler<AnimeListItemViewModel>? EditRequested;

        /// <summary>Wird ausgelöst, wenn ein Anime gelöscht werden soll.</summary>
        public event EventHandler<AnimeListItemViewModel>? DeleteRequested;

        #region Suche, Filter, Sortierung
        public string SearchText {
            get => this._searchText;
            set {
                if (this.SetProperty(ref this._searchText, value)) {
                    this.RefreshFilter();
                }
            }
        }

        public LibraryStatusFilter StatusFilter {
            get => this._statusFilter;
            set {
                if (this.SetProperty(ref this._statusFilter, value)) {
                    this.RefreshFilter();
                }
            }
        }

        public LibraryTierFilter TierFilter {
            get => this._tierFilter;
            set {
                if (this.SetProperty(ref this._tierFilter, value)) {
                    this.RefreshFilter();
                }
            }
        }

        public TagMatchMode TagMatchMode {
            get => this._tagMatchMode;
            set {
                if (this.SetProperty(ref this._tagMatchMode, value)) {
                    this.RefreshFilter();
                }
            }
        }

        public bool HasTagFilter => this.TagFilters.Count > 0;

        public bool HasMultipleTagFilters => this.TagFilters.Count > 1;

        public bool OnlyFavorites {
            get => this._onlyFavorites;
            set {
                if (this.SetProperty(ref this._onlyFavorites, value)) {
                    this.RefreshFilter();
                }
            }
        }

        public AnimeSortField SortField {
            get => this._sortField;
            set {
                if (this.SetProperty(ref this._sortField, value)) {
                    this.ApplySort();
                }
            }
        }

        /// <summary>Absteigend sortieren? (ToggleButton in der Kopfzeile)</summary>
        public bool SortDescending {
            get => !this._sortAscending;
            set {
                if (this._sortAscending == value) {
                    this._sortAscending = !value;
                    this.OnPropertyChanged();
                    this.OnPropertyChanged(nameof(this.SortDirectionText));
                    this.ApplySort();
                }
            }
        }

        public string SortDirectionText => this._sortAscending ? "↑ Aufsteigend" : "↓ Absteigend";
        #endregion

        #region Darstellung
        public LibraryViewMode ViewMode {
            get => this._viewMode;
            set {
                if (this.SetProperty(ref this._viewMode, value)) {
                    this.OnPropertyChanged(nameof(this.IsCardView));
                    this.OnPropertyChanged(nameof(this.IsTableView));
                    this.OnPropertyChanged(nameof(this.IsImageView));
                    this._settingsService.Current.LibraryViewMode = value;
                    this._settingsService.Save();
                }
            }
        }

        public bool IsCardView => this.ViewMode == LibraryViewMode.Cards;

        public bool IsTableView => this.ViewMode == LibraryViewMode.Table;

        public bool IsImageView => this.ViewMode == LibraryViewMode.Images;
        #endregion

        #region Anzahl und Leerzustand
        public int TotalCount => this.Animes.Count;

        public int VisibleCount {
            get => this._visibleCount;
            private set {
                if (this.SetProperty(ref this._visibleCount, value)) {
                    this.OnPropertyChanged(nameof(this.CountText));
                    this.OnPropertyChanged(nameof(this.IsEmpty));
                    this.OnPropertyChanged(nameof(this.HasItems));
                }
            }
        }

        public string CountText {
            get {
                string noun = this.TotalCount == 1 ? "Anime" : "Animes";
                return this.VisibleCount == this.TotalCount
                    ? $"{this.TotalCount} {noun}"
                    : $"{this.VisibleCount} von {this.TotalCount} {noun}";
            }
        }

        public bool IsEmpty => this.VisibleCount == 0;

        public bool HasItems => this.VisibleCount > 0;

        /// <summary>Hinweistext, wenn nichts angezeigt wird (leere Bibliothek oder nichts passt zum Filter)</summary>
        public string EmptyText => this.TotalCount == 0
            ? "Die Bibliothek ist noch leer. Lege mit „Neuer Anime“ den ersten Eintrag an."
            : "Kein Anime passt zu Suche und Filter.";
        #endregion

        public LibraryViewModel(IAnimeService animeService, AppSettingsService settingsService) {
            this._animeService = animeService;
            this._settingsService = settingsService;
            this._viewMode = settingsService.Current.LibraryViewMode;
            this.Items = CollectionViewSource.GetDefaultView(this.Animes);
            this.Items.Filter = this.Matches;
            this.ApplySort();
            this.Refresh();
        }

        /// <summary>Lädt die Animes neu, Suche, Filter und Sortierung bleiben erhalten.</summary>
        public void Refresh() {
            this.Animes.Clear();
            foreach (AnimeListEntry entry in this._animeService.SelectAnimeListEntries()) {
                AnimeListItemViewModel item = new AnimeListItemViewModel(entry, this._animeService);
                item.EditRequested += (sender, e) => this.EditRequested?.Invoke(this, item);
                item.DeleteRequested += (sender, e) => this.DeleteRequested?.Invoke(this, item);
                this.Animes.Add(item);
            }
            this.OnPropertyChanged(nameof(this.TotalCount));
            this.OnPropertyChanged(nameof(this.EmptyText));
            this.UpdateVisibleCount();
        }

        /// <summary>Ersetzt den Tag-Filter durch die übergebenen Tags.</summary>
        public void SetTagFilter(IEnumerable<TagFilterItem> tags, TagMatchMode mode) {
            this.TagFilters.Clear();
            foreach (TagFilterItem tag in tags) {
                this.TagFilters.Add(tag);
            }
            this._tagMatchMode = mode;
            this.OnPropertyChanged(nameof(this.TagMatchMode));
            this.OnTagFilterChanged();
        }

        public void RemoveTagFilter(TagFilterItem tag) {
            this.TagFilters.Remove(tag);
            this.OnTagFilterChanged();
        }

        public void ClearTagFilter() {
            this.TagFilters.Clear();
            this.OnTagFilterChanged();
        }

        private void OnTagFilterChanged() {
            this.OnPropertyChanged(nameof(this.HasTagFilter));
            this.OnPropertyChanged(nameof(this.HasMultipleTagFilters));
            this.RefreshFilter();
        }

        /// <summary>Sortiert nach der Spalte, wiederholtes Wählen derselben Spalte kehrt die Richtung um (Tabellenkopf).</summary>
        public void SortBy(AnimeSortField field) {
            if (this.SortField == field) {
                this.SortDescending = this._sortAscending;
            }
            else {
                this._sortAscending = true;
                this.OnPropertyChanged(nameof(this.SortDescending));
                this.OnPropertyChanged(nameof(this.SortDirectionText));
                this.SortField = field;
            }
        }

        private void RefreshFilter() {
            this.Items.Refresh();
            this.UpdateVisibleCount();
        }

        private void UpdateVisibleCount() {
            this.VisibleCount = this.Items.Cast<object>().Count();
        }

        private bool Matches(object item) {
            if (item is not AnimeListItemViewModel anime) {
                return false;
            }
            if (this.OnlyFavorites && !anime.IsFavorite) {
                return false;
            }
            if (this.StatusFilter != LibraryStatusFilter.All && !this.MatchesStatus(anime.State)) {
                return false;
            }
            if (this.TierFilter != LibraryTierFilter.All && !this.MatchesTier(anime.Tier)) {
                return false;
            }
            if (this.TagFilters.Count > 0 && !this.MatchesTags(anime)) {
                return false;
            }
            return string.IsNullOrWhiteSpace(this.SearchText)
                || anime.Name.Contains(this.SearchText.Trim(), StringComparison.CurrentCultureIgnoreCase);
        }

        private bool MatchesTags(AnimeListItemViewModel anime) {
            return this.TagMatchMode == TagMatchMode.All
                ? this.TagFilters.All(tag => anime.TagIds.Contains(tag.Id))
                : this.TagFilters.Any(tag => anime.TagIds.Contains(tag.Id));
        }

        private bool MatchesStatus(State state) {
            return this.StatusFilter switch {
                LibraryStatusFilter.Wishlist => state == State.Wishlist,
                LibraryStatusFilter.Started => state == State.Started,
                LibraryStatusFilter.Finished => state == State.Finished,
                LibraryStatusFilter.Interrupted => state == State.Interrupted,
                LibraryStatusFilter.Canceled => state == State.Canceled,
                _ => true
            };
        }

        private bool MatchesTier(Tier tier) {
            return this.TierFilter switch {
                LibraryTierFilter.S => tier == Tier.S,
                LibraryTierFilter.A => tier == Tier.A,
                LibraryTierFilter.B => tier == Tier.B,
                LibraryTierFilter.C => tier == Tier.C,
                LibraryTierFilter.D => tier == Tier.D,
                LibraryTierFilter.E => tier == Tier.E,
                LibraryTierFilter.NotDefined => tier == Tier.NotDefined,
                _ => true
            };
        }

        /// <summary>Setzt die Sortierung der Sicht: erst nach dem gewählten Kriterium, dann nach Name.</summary>
        private void ApplySort() {
            string property = this.SortField switch {
                AnimeSortField.Id => nameof(AnimeListItemViewModel.Id),
                AnimeSortField.State => nameof(AnimeListItemViewModel.State),
                AnimeSortField.Favorite => nameof(AnimeListItemViewModel.IsFavorite),
                AnimeSortField.Tier => nameof(AnimeListItemViewModel.Tier),
                _ => nameof(AnimeListItemViewModel.Name)
            };
            ListSortDirection direction = this._sortAscending ? ListSortDirection.Ascending : ListSortDirection.Descending;
            using (this.Items.DeferRefresh()) {
                this.Items.SortDescriptions.Clear();
                this.Items.SortDescriptions.Add(new SortDescription(property, direction));
                if (this.SortField != AnimeSortField.Name) {
                    this.Items.SortDescriptions.Add(new SortDescription(nameof(AnimeListItemViewModel.Name), ListSortDirection.Ascending));
                }
            }
        }
    }
}
