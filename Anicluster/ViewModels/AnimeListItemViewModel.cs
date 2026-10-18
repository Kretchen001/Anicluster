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

        public int Id { get; }
        public string Name { get; }
        public bool IsFavorite { get; }
        public Tier Tier { get; }
        public State State { get; }

        /// <summary>Deutscher Anzeigetext des Status</summary>
        public string StateText => TranslationOfState[this.State];

        /// <summary>Wird ausgelöst, wenn der Anime zum Anzeigen/Bearbeiten geöffnet werden soll.</summary>
        public event EventHandler? EditRequested;

        /// <summary>Wird ausgelöst, wenn der Anime gelöscht werden soll.</summary>
        public event EventHandler? DeleteRequested;

        public AnimeListItemViewModel(AnimeListEntry entry) {
            this.Id = entry.Id;
            this.Name = entry.Name;
            this.IsFavorite = entry.IsFavorite;
            this.Tier = entry.Tier;
            this.State = entry.State;
        }

        public void RequestEdit() {
            this.EditRequested?.Invoke(this, EventArgs.Empty);
        }

        public void RequestDelete() {
            this.DeleteRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
