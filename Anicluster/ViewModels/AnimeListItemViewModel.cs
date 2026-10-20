using Anicluster.Database.Services;
using Modells.Anime;

namespace Anicluster.ViewModels {

    /// <summary>
    /// Ein Eintrag in der Anime-Liste des Hauptfensters.
    /// </summary>
    public class AnimeListItemViewModel : PropertyBaseChange {

        private static readonly Dictionary<State, string> TranslationOfState = new Dictionary<State, string>() {
            { State.Wishlist, "Wunschliste" },
            { State.Started, "Begonnen" },
            { State.Finished, "Beendet" },
            { State.Interrupted, "Unterbrochen" },
            { State.Canceled, "Abgebrochen" },
        };

        private readonly IAnimeService _animeService;
        private byte[]? _thumbnail;
        private bool _isThumbnailLoaded;

        public int Id { get; }
        public string Name { get; }
        public bool IsFavorite { get; }
        public Tier Tier { get; }
        public State State { get; }

        /// <summary>Hat der Anime ein Bild?</summary>
        public bool HasImage { get; }

        /// <summary>
        /// Die kleine Vorschau des Bildes (JPEG-Daten), <c>null</c> ohne Bild. Sie wird erst beim ersten Zugriff geladen,
        /// damit die Karten- und Tabellenansicht keine Bilddaten lesen müssen.
        /// </summary>
        public byte[]? Thumbnail {
            get {
                if (!this._isThumbnailLoaded) {
                    this._isThumbnailLoaded = true;
                    this._thumbnail = this.HasImage ? this._animeService.SelectThumbnail(this.Id) : null;
                }
                return this._thumbnail;
            }
        }

        /// <summary>Ids der Tags dieses Animes (für den Tag-Filter)</summary>
        public HashSet<int> TagIds { get; }

        /// <summary>Kurztext des Tiers für die Tabelle ("-", wenn keins gesetzt ist)</summary>
        public string TierText => this.Tier == Tier.NotDefined ? "–" : this.Tier.ToString();

        /// <summary>Deutscher Anzeigetext des Status</summary>
        public string StateText => TranslationOfState[this.State];

        /// <summary>Wird ausgelöst, wenn der Anime zum Anzeigen/Bearbeiten geöffnet werden soll.</summary>
        public event EventHandler? EditRequested;

        /// <summary>Wird ausgelöst, wenn der Anime gelöscht werden soll.</summary>
        public event EventHandler? DeleteRequested;

        public AnimeListItemViewModel(AnimeListEntry entry, IAnimeService animeService) {
            this._animeService = animeService;
            this.Id = entry.Id;
            this.Name = entry.Name;
            this.IsFavorite = entry.IsFavorite;
            this.Tier = entry.Tier;
            this.State = entry.State;
            this.HasImage = entry.HasImage;
            this.TagIds = entry.TagIds.ToHashSet();
        }

        public void RequestEdit() {
            this.EditRequested?.Invoke(this, EventArgs.Empty);
        }

        public void RequestDelete() {
            this.DeleteRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
