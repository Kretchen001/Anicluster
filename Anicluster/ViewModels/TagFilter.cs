namespace Anicluster.ViewModels {

    /// <summary>Wie mehrere gewählte Tags zusammenwirken.</summary>
    public enum TagMatchMode {
        /// <summary>Der Anime muss alle gewählten Tags haben (UND).</summary>
        All,

        /// <summary>Der Anime muss mindestens einen der gewählten Tags haben (ODER).</summary>
        Any
    }

    /// <summary>Ein Tag, nach dem gefiltert wird.</summary>
    public record TagFilterItem(int Id, string Name);

    /// <summary>Wunsch des Netzwerks, die gewählten Tags als Filter in der Bibliothek anzuzeigen.</summary>
    public class ShowInLibraryEventArgs : EventArgs {

        public required IReadOnlyList<TagFilterItem> Tags { get; init; }

        public TagMatchMode Mode { get; init; }

        /// <summary>Optional: Status, auf den die Bibliothek zusätzlich eingeschränkt wird (<c>null</c> = nicht ändern)</summary>
        public LibraryStatusFilter? Status { get; init; }
    }
}
