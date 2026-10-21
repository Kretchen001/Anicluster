using Modells.Anime;

namespace Anicluster.TasteProfile {

    /// <summary>Einstellungen der Auswertung: Was zählt als Lieblings-Anime?</summary>
    /// <param name="LikedTiers">Animes mit einem dieser Tiers zählen als Lieblinge</param>
    /// <param name="IncludeFavorites">Zählen auch Animes mit Favoriten-Stern als Lieblinge?</param>
    /// <param name="SuggestionState">Status der Animes, die bewertet werden (<c>null</c> = alle Stati)</param>
    public record TasteOptions(IReadOnlySet<Tier> LikedTiers, bool IncludeFavorites, State? SuggestionState);

    /// <summary>
    /// Auswertung eines Tags: Wie oft kommt er bei den Lieblingen vor, wie oft in der ganzen Bibliothek?
    /// </summary>
    /// <param name="TagId">Id des Tags</param>
    /// <param name="Name">Bezeichnung des Tags</param>
    /// <param name="LikedCount">Anzahl der Lieblinge mit diesem Tag</param>
    /// <param name="TotalCount">Anzahl aller gesehenen Animes mit diesem Tag</param>
    /// <param name="LikedShare">Anteil der Lieblinge, die den Tag haben (0 bis 1)</param>
    /// <param name="LibraryShare">Anteil aller gesehenen Animes, die den Tag haben (0 bis 1)</param>
    /// <param name="Lift">Wie viel häufiger der Tag bei Lieblingen vorkommt als zu erwarten wäre (1 = durchschnittlich)</param>
    public record TagTaste(int TagId, string Name, int LikedCount, int TotalCount, double LikedShare, double LibraryShare, double Lift) {

        /// <summary>Logarithmus des Lifts: positiv = mag man eher, negativ = eher nicht, 0 = neutral</summary>
        public double Weight => Math.Log(this.Lift);
    }

    /// <summary>Ein Anime der Wunschliste mit seiner Passung zum Geschmacksprofil.</summary>
    /// <param name="Entry">Der Anime</param>
    /// <param name="Percent">Passung in Prozent, bezogen auf den am besten passenden Anime der Wunschliste (100)</param>
    /// <param name="MatchingTags">Die Tags, wegen derer der Anime passt (stärkste zuerst)</param>
    public record TasteSuggestion(AnimeListEntry Entry, int Percent, IReadOnlyList<TagTaste> MatchingTags);

    /// <summary>Ergebnis der Auswertung.</summary>
    /// <param name="LikedAnimeCount">Anzahl der Lieblinge, auf denen das Profil beruht</param>
    /// <param name="WatchedAnimeCount">Anzahl aller gesehenen Animes (alle außer der Wunschliste)</param>
    /// <param name="HasEnoughData">Gibt es genug Lieblinge für ein aussagekräftiges Profil?</param>
    /// <param name="Preferred">Tags, die bei Lieblingen auffällig häufig vorkommen</param>
    /// <param name="Avoided">Tags, die bei Lieblingen auffällig selten vorkommen</param>
    /// <param name="Suggestions">Passende Animes der Wunschliste, beste zuerst</param>
    public record TasteProfile(
        int LikedAnimeCount,
        int WatchedAnimeCount,
        bool HasEnoughData,
        IReadOnlyList<TagTaste> Preferred,
        IReadOnlyList<TagTaste> Avoided,
        IReadOnlyList<TasteSuggestion> Suggestions
    );
}
