namespace Anicluster.ViewModels {

    /// <summary>Eintrag einer Auswahlliste: technischer Wert und deutscher Anzeigetext.</summary>
    public record FilterOption<T>(T Value, string Label);

    /// <summary>Statusfilter der Bibliothek.</summary>
    public enum LibraryStatusFilter {
        All,
        Wishlist,
        Started,
        Finished,
        Interrupted,
        Canceled
    }

    /// <summary>Tierfilter der Bibliothek.</summary>
    public enum LibraryTierFilter {
        All,
        S,
        A,
        B,
        C,
        D,
        E,
        NotDefined
    }
}
