using Anicluster.Database.Services;
using Anicluster.Timeline;
using Modells.Anime;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;

namespace Anicluster.ViewModels {

    /// <summary>
    /// Der Zeitstrahl: alle Animes mit eingetragenem Veröffentlichungszeitraum als Balken auf einer gemeinsamen Zeitachse.
    /// Die Pixelpositionen berechnet <see cref="TimelineLayout"/>, dieses ViewModel hält Zoom, Suche und Farbwahl.
    /// </summary>
    public class TimelineViewModel : PropertyBaseChange {

        /// <summary>Höhe einer Zeile in Pixeln (muss zur View passen)</summary>
        public const double RowHeight = 32.0;

        /// <summary>Höhe der Zeitachse oben in Pixeln (muss zur View passen)</summary>
        public const double HeaderHeight = 30.0;

        public const double MinPixelsPerYear = 20.0;
        public const double MaxPixelsPerYear = 400.0;

        private readonly List<TimelineEntry> _entries;
        private double _pixelsPerYear = 120.0;
        private string _searchText = string.Empty;
        private bool _colorByTier = true;
        private string _tagSearchText = string.Empty;
        private TagMatchMode _tagMatchMode = TagMatchMode.All;
        private bool _isUpdatingTags;
        private double _width;
        private double _todayLeft;
        private int _withoutPeriodCount;

        /// <summary>Die sichtbaren Zeilen, früheste Veröffentlichung zuerst</summary>
        public ObservableCollection<TimelineRowViewModel> Rows { get; } = [];

        /// <summary>Alle Tags mit Auswahlzustand für die Tag-Auswahl</summary>
        public ObservableCollection<TagItemViewModel> AllTags { get; } = [];

        /// <summary>Gefilterte Sicht auf <see cref="AllTags"/> (nach <see cref="TagSearchText"/>)</summary>
        public ICollectionView FilteredTags { get; }

        /// <summary>Die gewählten Tags, nach denen der Zeitstrahl gefiltert wird</summary>
        public ObservableCollection<TagFilterItem> SelectedTags { get; } = [];

        /// <summary>Die Jahresmarken der Zeitachse</summary>
        public ObservableCollection<TimelineTick> Ticks { get; } = [];

        /// <summary>Wird ausgelöst, wenn ein Anime (Id) zum Anzeigen/Bearbeiten geöffnet werden soll.</summary>
        public event EventHandler<int>? EditRequested;

        /// <summary>Wird nach einer Änderung des Zooms ausgelöst; der Wert ist das Verhältnis neuer zu alter Zoom.</summary>
        public event EventHandler<double>? ZoomChanged;

        public double PixelsPerYear {
            get => this._pixelsPerYear;
            set {
                double clamped = Math.Clamp(value, MinPixelsPerYear, MaxPixelsPerYear);
                double old = this._pixelsPerYear;
                if (this.SetProperty(ref this._pixelsPerYear, clamped)) {
                    this.Rebuild();
                    this.ZoomChanged?.Invoke(this, clamped / old);
                }
            }
        }

        public string SearchText {
            get => this._searchText;
            set {
                if (this.SetProperty(ref this._searchText, value)) {
                    this.Rebuild();
                }
            }
        }

        public string TagSearchText {
            get => this._tagSearchText;
            set {
                if (this.SetProperty(ref this._tagSearchText, value)) {
                    this.FilteredTags.Refresh();
                }
            }
        }

        public TagMatchMode TagMatchMode {
            get => this._tagMatchMode;
            set {
                if (this.SetProperty(ref this._tagMatchMode, value)) {
                    this.Rebuild();
                }
            }
        }

        public bool HasTagFilter => this.SelectedTags.Count > 0;

        public bool HasMultipleTagFilters => this.SelectedTags.Count > 1;

        /// <summary>Beschriftung des Auswahlknopfes, z. B. „Tags (3) ▾“</summary>
        public string TagButtonText => this.SelectedTags.Count == 0 ? "Tags ▾" : $"Tags ({this.SelectedTags.Count}) ▾";

        public bool ColorByTier {
            get => this._colorByTier;
            set {
                if (this.SetProperty(ref this._colorByTier, value)) {
                    this.OnPropertyChanged(nameof(this.ColorByState));
                    this.Rebuild();
                }
            }
        }

        public bool ColorByState {
            get => !this._colorByTier;
            set => this.ColorByTier = !value;
        }

        /// <summary>Gesamtbreite der Zeitachse in Pixeln</summary>
        public double Width {
            get => this._width;
            private set => this.SetProperty(ref this._width, value);
        }

        /// <summary>Gesamthöhe aller Zeilen in Pixeln</summary>
        public double BodyHeight => this.Rows.Count * RowHeight;

        /// <summary>Position der Heute-Linie in Pixeln</summary>
        public double TodayLeft {
            get => this._todayLeft;
            private set => this.SetProperty(ref this._todayLeft, value);
        }

        public bool HasRows => this.Rows.Count > 0;

        public bool HasNoRows => !this.HasRows;

        /// <summary>Kurzinfo neben dem Titel, z. B. „24 Animes · 3 ohne Zeitraum“</summary>
        public string InfoText {
            get {
                string text = $"{this.Rows.Count} {(this.Rows.Count == 1 ? "Anime" : "Animes")}";
                return this._withoutPeriodCount > 0 ? $"{text} · {this._withoutPeriodCount} ohne Zeitraum" : text;
            }
        }

        /// <summary>Hinweis, wenn nichts angezeigt werden kann</summary>
        public string EmptyText => this._searchText.Trim().Length > 0 || this.HasTagFilter
            ? "Kein Anime mit Veröffentlichungszeitraum passt zu den Filtern."
            : "Noch kein Anime hat einen Veröffentlichungszeitraum. Trage ihn im Editor bei den Medieninfos oder den Staffeln ein.";

        public TimelineViewModel(IAnimeService animeService, ITagService tagService) {
            this._entries = animeService.SelectTimelineEntries();
            foreach (Tag tag in tagService.SelectAllTags()) {
                TagItemViewModel item = new TagItemViewModel(tag);
                item.PropertyChanged += this.OnTagItemChanged;
                this.AllTags.Add(item);
            }
            this.FilteredTags = CollectionViewSource.GetDefaultView(this.AllTags);
            this.FilteredTags.Filter = this.MatchesTagSearch;
            this.Rebuild();
        }

        /// <summary>Entfernt einen Tag aus dem Filter.</summary>
        public void RemoveTag(TagFilterItem tag) {
            TagItemViewModel? item = this.AllTags.FirstOrDefault(x => x.Tag.Id == tag.Id);
            if (item is not null) {
                item.IsSelected = false;
            }
        }

        /// <summary>Wählt alle Tags ab.</summary>
        public void ClearTags() {
            this._isUpdatingTags = true;
            foreach (TagItemViewModel item in this.AllTags) {
                item.IsSelected = false;
            }
            this._isUpdatingTags = false;
            this.UpdateSelectedTags();
        }

        private void OnTagItemChanged(object? sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(TagItemViewModel.IsSelected) && !this._isUpdatingTags) {
                this.UpdateSelectedTags();
            }
        }

        private void UpdateSelectedTags() {
            this.SelectedTags.Clear();
            foreach (TagItemViewModel item in this.AllTags.Where(x => x.IsSelected)) {
                this.SelectedTags.Add(new TagFilterItem(item.Tag.Id, item.Tag.Designation));
            }
            this.OnPropertyChanged(nameof(this.HasTagFilter));
            this.OnPropertyChanged(nameof(this.HasMultipleTagFilters));
            this.OnPropertyChanged(nameof(this.TagButtonText));
            this.Rebuild();
        }

        private bool MatchesTagSearch(object item) {
            return item is TagItemViewModel tagItem
                && (this._tagSearchText.Length == 0
                    || tagItem.Tag.Designation.Contains(this._tagSearchText, StringComparison.CurrentCultureIgnoreCase));
        }

        /// <summary>Vergrößert (Faktor über 1) oder verkleinert (unter 1) den Zoom, z. B. per Strg + Mausrad.</summary>
        public void ZoomBy(double factor) {
            this.PixelsPerYear *= factor;
        }

        private void Rebuild() {
            List<int> tagIds = this.SelectedTags.Select(x => x.Id).ToList();
            TimelineLayoutResult layout = TimelineLayout.Calculate(
                this._entries, this._pixelsPerYear, DateTime.Today, this._searchText, tagIds, this._tagMatchMode == TagMatchMode.All);
            this.Rows.Clear();
            foreach (TimelineRowLayout rowLayout in layout.Rows) {
                TimelineRowViewModel row = new TimelineRowViewModel(rowLayout, this._colorByTier);
                row.EditRequested += (sender, e) => this.EditRequested?.Invoke(this, row.Id);
                this.Rows.Add(row);
            }
            this.Ticks.Clear();
            foreach (TimelineTick tick in layout.Ticks) {
                this.Ticks.Add(tick);
            }
            this._withoutPeriodCount = layout.WithoutPeriodCount;
            this.Width = layout.Width;
            this.TodayLeft = layout.TodayLeft;
            this.OnPropertyChanged(nameof(this.BodyHeight));
            this.OnPropertyChanged(nameof(this.HasRows));
            this.OnPropertyChanged(nameof(this.HasNoRows));
            this.OnPropertyChanged(nameof(this.InfoText));
            this.OnPropertyChanged(nameof(this.EmptyText));
        }
    }
}
