namespace Anicluster.Metadata {

    /// <summary>
    /// Ein Treffer einer Online-Quelle. Enthält alles, was die Anwendung übernehmen kann,
    /// ein fehlender Wert ist <c>null</c>. Die Beschreibung ist bereits von HTML befreit.
    /// </summary>
    /// <param name="Provider">Die Quelle des Treffers</param>
    /// <param name="ExternalId">Id des Eintrags bei der Quelle (immer Text, für die URL der Abfrage)</param>
    /// <param name="Title">Haupttitel der Quelle (meist Romaji)</param>
    /// <param name="AlternativeTitle">Englischer oder japanischer Titel, falls er sich vom Haupttitel unterscheidet</param>
    /// <param name="Format">Art des Eintrags, zum Beispiel "TV-Serie" oder "Film"</param>
    /// <param name="Year">Jahr der Erstveröffentlichung</param>
    /// <param name="EpisodeCount">Anzahl der Episoden</param>
    /// <param name="Description">Beschreibung (meist Englisch)</param>
    /// <param name="CoverUrl">Adresse des Covers (https)</param>
    /// <param name="Start">Beginn der Veröffentlichung, eventuell unvollständig</param>
    /// <param name="End">Ende der Veröffentlichung, eventuell unvollständig oder unbekannt</param>
    /// <param name="MyAnimeListId">Id bei MyAnimeList, wenn die Quelle sie mitliefert</param>
    public record MetadataCandidate(
        MetadataProviderKind Provider,
        string ExternalId,
        string Title,
        string? AlternativeTitle,
        string? Format,
        int? Year,
        int? EpisodeCount,
        string? Description,
        string? CoverUrl,
        PartialDate? Start,
        PartialDate? End,
        int? MyAnimeListId
    );
}
