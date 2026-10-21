namespace Modells.Anime {

    /// <summary>
    /// Die Merkmale eines Animes, an denen er wiedererkannt wird (zum Beispiel beim Zusammenführen einer Datensicherung):
    /// die feste Kennung, die Ids bei den Online-Quellen und der Name.
    /// </summary>
    public record AnimeIdentity(
        int Id,
        string Uid,
        string Name,
        string? OriginalName,
        int? AniListId,
        int? MyAnimeListId,
        int? KitsuId
    );
}
