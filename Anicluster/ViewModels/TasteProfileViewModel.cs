using Anicluster.Database.Services;
using Anicluster.TasteProfile;
using Modells.Anime;
using System.Collections.ObjectModel;

namespace Anicluster.ViewModels {

    /// <summary>
    /// Das Geschmacksprofil: Welche Tags kommen bei deinen Lieblings-Animes auffällig oft oder selten vor,
    /// und welche Animes deiner Wunschliste passen deshalb besonders gut?
    /// Die Berechnung übernimmt der <see cref="TasteProfileCalculator"/>, dieses ViewModel hält die Einstellungen und bereitet das Ergebnis auf.
    /// </summary>
    public class TasteProfileViewModel : PropertyBaseChange {

        /// <summary>Anzahl der stärksten Tags, die der Knopf „In Bibliothek filtern“ als Filter übernimmt</summary>
        private const int FilterTagCount = 5;

        private const int MinThreshold = 5;
        private const int MaxThreshold = 100;

        private readonly IAnimeService _animeService;
        private readonly List<AnimeListEntry> _entries;
        private readonly Dictionary<int, string> _tagNames;
        private bool _includeTierS = true;
        private bool _includeTierA = true;
        private bool _includeTierB;
        private bool _includeTierC;
        private bool _includeTierD;
        private bool _includeFavorites = true;
        private LibraryStatusFilter _suggestionStatus = LibraryStatusFilter.Wishlist;
        private int _goodThreshold = 60;
        private int _fairThreshold = 30;
        private TasteProfile.TasteProfile _profile;

        /// <summary>Tags, die bei Lieblingen auffällig häufig vorkommen</summary>
        public ObservableCollection<TasteTagItemViewModel> PreferredTags { get; } = [];

        /// <summary>Tags, die bei Lieblingen auffällig selten vorkommen</summary>
        public ObservableCollection<TasteTagItemViewModel> AvoidedTags { get; } = [];

        /// <summary>Animes mit dem gewählten Status, die zu dir passen (Passung ab dem oberen Schwellwert), die beste zuerst</summary>
        public ObservableCollection<TasteSuggestionViewModel> Suggestions { get; } = [];

        /// <summary>Animes mit dem gewählten Status, die leidlich zu dir passen (zwischen den beiden Schwellwerten)</summary>
        public ObservableCollection<TasteSuggestionViewModel> FairSuggestions { get; } = [];

        /// <summary>Wird ausgelöst, wenn ein Anime zum Anzeigen/Bearbeiten geöffnet werden soll.</summary>
        public event EventHandler<AnimeListItemViewModel>? EditRequested;

        /// <summary>Wird ausgelöst, wenn Tags als Filter in der Bibliothek angezeigt werden sollen.</summary>
        public event EventHandler<ShowInLibraryEventArgs>? ShowInLibraryRequested;

        #region Was zählt als Liebling?
        public bool IncludeTierS {
            get => this._includeTierS;
            set {
                if (this.SetProperty(ref this._includeTierS, value)) {
                    this.Recalculate();
                }
            }
        }

        public bool IncludeTierA {
            get => this._includeTierA;
            set {
                if (this.SetProperty(ref this._includeTierA, value)) {
                    this.Recalculate();
                }
            }
        }

        public bool IncludeTierB {
            get => this._includeTierB;
            set {
                if (this.SetProperty(ref this._includeTierB, value)) {
                    this.Recalculate();
                }
            }
        }

        public bool IncludeTierC {
            get => this._includeTierC;
            set {
                if (this.SetProperty(ref this._includeTierC, value)) {
                    this.Recalculate();
                }
            }
        }

        public bool IncludeTierD {
            get => this._includeTierD;
            set {
                if (this.SetProperty(ref this._includeTierD, value)) {
                    this.Recalculate();
                }
            }
        }

        public bool IncludeFavorites {
            get => this._includeFavorites;
            set {
                if (this.SetProperty(ref this._includeFavorites, value)) {
                    this.Recalculate();
                }
            }
        }
        #endregion

        #region Welche Animes werden bewertet?
        /// <summary>Auswahl des Status, dessen Animes bewertet werden (zuletzt „Alle Stati“)</summary>
        public IReadOnlyList<FilterOption<LibraryStatusFilter>> StatusOptions { get; } = [
            new(LibraryStatusFilter.Wishlist, "Wunschliste"),
            new(LibraryStatusFilter.Started, "Begonnen"),
            new(LibraryStatusFilter.Finished, "Beendet"),
            new(LibraryStatusFilter.Interrupted, "Unterbrochen"),
            new(LibraryStatusFilter.Canceled, "Abgebrochen"),
            new(LibraryStatusFilter.All, "Alle Stati")
        ];

        public LibraryStatusFilter SuggestionStatus {
            get => this._suggestionStatus;
            set {
                if (this.SetProperty(ref this._suggestionStatus, value)) {
                    this.Recalculate();
                }
            }
        }
        #endregion

        #region Schwellwerte
        /// <summary>Ab dieser Passung (in Prozent) „passt“ ein Anime zu dir</summary>
        public int GoodThreshold {
            get => this._goodThreshold;
            set {
                int clamped = Math.Clamp(value, MinThreshold + 1, MaxThreshold);
                if (this.SetProperty(ref this._goodThreshold, clamped)) {
                    // Der untere Schwellwert muss immer unter dem oberen liegen
                    if (this._fairThreshold >= clamped) {
                        this._fairThreshold = clamped - 1;
                        this.OnPropertyChanged(nameof(this.FairThreshold));
                    }
                    this.ApplyThresholds();
                }
            }
        }

        /// <summary>Ab dieser Passung (in Prozent) „passt“ ein Anime leidlich zu dir</summary>
        public int FairThreshold {
            get => this._fairThreshold;
            set {
                int clamped = Math.Clamp(value, MinThreshold, MaxThreshold - 1);
                if (this.SetProperty(ref this._fairThreshold, clamped)) {
                    if (this._goodThreshold <= clamped) {
                        this._goodThreshold = clamped + 1;
                        this.OnPropertyChanged(nameof(this.GoodThreshold));
                    }
                    this.ApplyThresholds();
                }
            }
        }

        public string GoodThresholdText => $"ab {this._goodThreshold} %";

        public string FairThresholdText => $"ab {this._fairThreshold} %";

        public string FairSuggestionsTitle => $"Passt leidlich zu dir ({this._fairThreshold} bis {this._goodThreshold - 1} %)";

        public string GoodSuggestionsTitle => $"Passt zu dir (ab {this._goodThreshold} %)";
        #endregion

        #region Ergebnis
        /// <summary>Gibt es genug Lieblinge für ein Profil?</summary>
        public bool HasProfile => this._profile.HasEnoughData;

        public bool HasNoProfile => !this.HasProfile;

        public bool HasPreferredTags => this.PreferredTags.Count > 0;

        public bool HasAvoidedTags => this.AvoidedTags.Count > 0;

        public bool HasSuggestions => this.Suggestions.Count > 0;

        public bool HasFairSuggestions => this.FairSuggestions.Count > 0;

        /// <summary>Weder „passt“ noch „passt leidlich“ hat einen Treffer</summary>
        public bool HasNoAnySuggestions => !this.HasSuggestions && !this.HasFairSuggestions;

        /// <summary>Die Basis der Auswertung, z. B. „12 Lieblinge von 48 gesehenen Animes“</summary>
        public string BasisText {
            get {
                string watchedNoun = this._profile.WatchedAnimeCount == 1 ? "gesehenen Anime" : "gesehenen Animes";
                string likedNoun = this._profile.LikedAnimeCount == 1 ? "Liebling" : "Lieblinge";
                return $"{this._profile.LikedAnimeCount} {likedNoun} von {this._profile.WatchedAnimeCount} {watchedNoun}";
            }
        }

        /// <summary>Hinweis, wenn zu wenige Lieblinge für ein Profil vorhanden sind</summary>
        public string NoProfileText =>
            $"Für ein Profil werden mindestens {TasteProfileCalculator.MinLikedAnimes} Lieblinge gebraucht. "
            + "Wähle oben weitere Tiers aus oder vergib im Editor Tiers und Favoriten-Sterne "
            + "(Animes auf der Wunschliste zählen nicht als Lieblinge mit).";

        /// <summary>Hinweistext, wenn keine Vorschläge angezeigt werden</summary>
        public string NoSuggestionsText => this._entries.Any(x => this.MatchesSuggestionStatus(x.State))
            ? "Kein Anime mit diesem Status erreicht den Schwellwert. Senke ihn oder erweitere, was als Liebling zählt."
            : "Es gibt keine Animes mit diesem Status.";

        /// <summary>Kann die Bibliothek mit den stärksten Tags gefiltert werden?</summary>
        public bool CanShowInLibrary => this.HasPreferredTags;
        #endregion

        public TasteProfileViewModel(IAnimeService animeService, ITagService tagService) {
            this._animeService = animeService;
            this._entries = animeService.SelectAnimeListEntries();
            this._tagNames = tagService.SelectAllTags().ToDictionary(x => x.Id, x => x.Designation);
            this._profile = this.CalculateProfile();
            this.FillResults();
        }

        /// <summary>Wechselt zur Bibliothek und zeigt dort die Animes mit dem gewählten Status gefiltert nach den stärksten Tags des Profils (ein Tag genügt).</summary>
        public void ShowSuggestionsInLibrary() {
            List<TagFilterItem> tags = this.PreferredTags
                .Take(FilterTagCount)
                .Select(x => new TagFilterItem(x.TagId, x.Name))
                .ToList();
            if (tags.Count == 0) {
                return;
            }
            this.ShowInLibraryRequested?.Invoke(this, new ShowInLibraryEventArgs() {
                Tags = tags,
                Mode = TagMatchMode.Any,
                Status = this.SuggestionStatus
            });
        }

        /// <summary>Wechselt zur Bibliothek und zeigt dort alle Animes mit diesem Tag.</summary>
        public void ShowInLibrary(TasteTagItemViewModel tag) {
            this.ShowInLibraryRequested?.Invoke(this, new ShowInLibraryEventArgs() {
                Tags = [new TagFilterItem(tag.TagId, tag.Name)],
                Mode = TagMatchMode.All
            });
        }

        private void Recalculate() {
            this._profile = this.CalculateProfile();
            this.FillResults();
        }

        private TasteProfile.TasteProfile CalculateProfile() {
            HashSet<Tier> tiers = [];
            if (this.IncludeTierS) {
                tiers.Add(Tier.S);
            }
            if (this.IncludeTierA) {
                tiers.Add(Tier.A);
            }
            if (this.IncludeTierB) {
                tiers.Add(Tier.B);
            }
            if (this.IncludeTierC) {
                tiers.Add(Tier.C);
            }
            if (this.IncludeTierD) {
                tiers.Add(Tier.D);
            }
            TasteOptions options = new TasteOptions(tiers, this.IncludeFavorites, this.GetSuggestionState());
            return TasteProfileCalculator.Calculate(this._entries, this._tagNames, options);
        }

        /// <summary>Der gewählte Status als <see cref="State"/>, <c>null</c> bei „Alle Stati“.</summary>
        private State? GetSuggestionState() {
            return this._suggestionStatus switch {
                LibraryStatusFilter.Wishlist => State.Wishlist,
                LibraryStatusFilter.Started => State.Started,
                LibraryStatusFilter.Finished => State.Finished,
                LibraryStatusFilter.Interrupted => State.Interrupted,
                LibraryStatusFilter.Canceled => State.Canceled,
                _ => null
            };
        }

        private bool MatchesSuggestionStatus(State state) {
            State? selected = this.GetSuggestionState();
            return selected is null || selected == state;
        }

        /// <summary>Überträgt das Ergebnis in die Listen und meldet der View alle abgeleiteten Werte.</summary>
        private void FillResults() {
            this.PreferredTags.Clear();
            foreach (TagTaste taste in this._profile.Preferred) {
                this.PreferredTags.Add(new TasteTagItemViewModel(taste, this._profile.LikedAnimeCount, false));
            }
            this.AvoidedTags.Clear();
            foreach (TagTaste taste in this._profile.Avoided) {
                this.AvoidedTags.Add(new TasteTagItemViewModel(taste, this._profile.LikedAnimeCount, true));
            }
            this.ApplyThresholds();
            this.OnPropertyChanged(nameof(this.HasProfile));
            this.OnPropertyChanged(nameof(this.HasNoProfile));
            this.OnPropertyChanged(nameof(this.HasPreferredTags));
            this.OnPropertyChanged(nameof(this.HasAvoidedTags));
            this.OnPropertyChanged(nameof(this.BasisText));
            this.OnPropertyChanged(nameof(this.NoSuggestionsText));
            this.OnPropertyChanged(nameof(this.CanShowInLibrary));
        }

        /// <summary>Verteilt die berechneten Vorschläge anhand der Schwellwerte auf „passt“ und „passt leidlich“.</summary>
        private void ApplyThresholds() {
            this.Suggestions.Clear();
            this.FairSuggestions.Clear();
            foreach (TasteSuggestion suggestion in this._profile.Suggestions) {
                if (suggestion.Percent < this._fairThreshold) {
                    continue;
                }
                AnimeListItemViewModel item = new AnimeListItemViewModel(suggestion.Entry, this._animeService);
                item.EditRequested += (sender, e) => this.EditRequested?.Invoke(this, item);
                TasteSuggestionViewModel viewModel = new TasteSuggestionViewModel(suggestion, item);
                if (suggestion.Percent >= this._goodThreshold) {
                    this.Suggestions.Add(viewModel);
                }
                else {
                    this.FairSuggestions.Add(viewModel);
                }
            }
            this.OnPropertyChanged(nameof(this.HasSuggestions));
            this.OnPropertyChanged(nameof(this.HasFairSuggestions));
            this.OnPropertyChanged(nameof(this.HasNoAnySuggestions));
            this.OnPropertyChanged(nameof(this.GoodThresholdText));
            this.OnPropertyChanged(nameof(this.FairThresholdText));
            this.OnPropertyChanged(nameof(this.GoodSuggestionsTitle));
            this.OnPropertyChanged(nameof(this.FairSuggestionsTitle));
        }
    }
}
