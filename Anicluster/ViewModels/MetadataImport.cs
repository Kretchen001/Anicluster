using Anicluster.Metadata;
using System.Globalization;

namespace Anicluster.ViewModels {

    /// <summary>
    /// Was der Dialog "Metadaten laden" über den Anime wissen muss: womit gesucht wird, was schon vorhanden ist
    /// (damit nichts versehentlich überschrieben wird) und mit welchen Einträgen der Anime schon verknüpft ist.
    /// </summary>
    public record MetadataImportContext(
        string SearchText,
        bool HasImage,
        bool HasDescription,
        bool HasStartDate,
        bool HasEndDate,
        bool HasProducer,
        bool HasPublisher,
        bool HasAuthor,
        bool HasOriginalName,
        int? AniListId,
        int? MyAnimeListId,
        int? KitsuId
    ) {

        /// <summary>Gespeicherte Id des Animes bei der Quelle, <c>null</c> wenn er dort nicht verknüpft ist.</summary>
        public string? GetLinkedId(MetadataProviderKind provider) {
            int? id = provider switch {
                MetadataProviderKind.AniList => this.AniListId,
                MetadataProviderKind.Jikan => this.MyAnimeListId,
                MetadataProviderKind.Kitsu => this.KitsuId,
                _ => null
            };
            return id?.ToString(CultureInfo.InvariantCulture);
        }
    }

    /// <summary>
    /// Die vom Benutzer bestätigte Auswahl. Ein Feld ist <c>null</c>, wenn es nicht übernommen werden soll.
    /// Von den Daten des Zeitraums steht nur ein vollständiges Datum drin, unvollständige bleiben leer.
    /// </summary>
    /// <param name="Provider">Die Quelle, bei der der Eintrag gewählt wurde</param>
    /// <param name="ExternalId">Id des Eintrags bei dieser Quelle</param>
    /// <param name="MyAnimeListId">Id bei MyAnimeList, wenn die Quelle sie mitgeliefert hat</param>
    /// <param name="CoverData">Das heruntergeladene Cover (noch unverkleinert)</param>
    /// <param name="Description">Die Beschreibung</param>
    /// <param name="StartDate">Beginn der Veröffentlichung</param>
    /// <param name="EndDate">Ende der Veröffentlichung</param>
    /// <param name="Producer">Studios und Produzenten</param>
    /// <param name="Publisher">Lizenzgeber und Veröffentlicher</param>
    /// <param name="Author">Urheber der Vorlage</param>
    /// <param name="OriginalName">Titel in der Originalsprache</param>
    public record MetadataImportResult(
        MetadataProviderKind Provider,
        string ExternalId,
        int? MyAnimeListId,
        byte[]? CoverData,
        string? Description,
        DateTime? StartDate,
        DateTime? EndDate,
        string? Producer,
        string? Publisher,
        string? Author,
        string? OriginalName
    );
}
