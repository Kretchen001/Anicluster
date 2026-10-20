namespace Modells.Anime {

    /// <summary>
    /// Ein Zeitraum auf dem Zeitstrahl. Ist das Ende unbekannt, ist <paramref name="End"/> <c>null</c>.
    /// </summary>
    public record TimelineSpan(DateTime Start, DateTime? End);

    /// <summary>Der Veröffentlichungszeitraum einer Staffel.</summary>
    public record TimelineSeason(int Number, TimelineSpan Span);

    /// <summary>Eine Pause in der Veröffentlichung.</summary>
    public record TimelineInterruption(DateTime Start, DateTime End);

    /// <summary>
    /// Schlanke Darstellung eines Animes für den Zeitstrahl: seine Veröffentlichungszeiträume (gesamt und je Staffel)
    /// und die Unterbrechungen. <paramref name="Run"/> ist <c>null</c>, wenn kein Zeitraum für den Anime eingetragen ist.
    /// </summary>
    public record TimelineEntry(
        int Id,
        string Name,
        bool IsFavorite,
        Tier Tier,
        State State,
        TimelineSpan? Run,
        IReadOnlyList<TimelineSeason> Seasons,
        IReadOnlyList<TimelineInterruption> Interruptions);
}
