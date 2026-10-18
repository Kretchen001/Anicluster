namespace Modells.Anime {

    /// <summary>
    /// Schlanke Darstellung eines Animes für die Listenansicht (ohne Bewertung, Staffeln usw.).
    /// </summary>
    public record AnimeListEntry(int Id, string Name, bool IsFavorite, Tier Tier, State State);
}
