namespace Modells.Anime {

    /// <summary>
    /// Die Teilnoten eines bewerteten Animes (je 0 bis 100) und seine Gesamtnote für das Anime-Netzwerk.
    /// <paramref name="Acoustic"/> ist die zusammengefasste Akustik (Soundtrack, Openings, Endings, Syncros).
    /// </summary>
    public record AnimeRatingEntry(int Id, int Story, int Animation, int SpecialEffects, int Acoustic, int General);
}
