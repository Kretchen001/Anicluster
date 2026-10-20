namespace Modells.Anime {

    /// <summary>
    /// Schlanke Darstellung eines Animes für die Listenansicht (ohne Bewertung, Staffeln usw.),
    /// inklusive der Ids seiner Tags für Filter und Netzwerk. <paramref name="HasImage"/> sagt, ob ein Cover vorhanden ist,
    /// die Bilddaten selbst werden erst bei Bedarf geladen.
    /// </summary>
    public record AnimeListEntry(int Id, string Name, bool IsFavorite, Tier Tier, State State, IReadOnlyList<int> TagIds, bool HasImage);
}
