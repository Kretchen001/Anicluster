namespace Anicluster.Metadata {

    /// <summary>
    /// Zugriff auf die Online-Quellen für Metadaten. Jede Methode macht höchstens eine Anfrage und nur, wenn sie aufgerufen wird.
    /// Fehler kommen als <see cref="MetadataResult{T}"/> zurück, es werden keine Ausnahmen geworfen.
    /// </summary>
    public interface IMetadataClient {

        /// <summary>Sucht Animes nach Name (höchstens wenige Treffer, nach Passung sortiert).</summary>
        Task<MetadataResult<IReadOnlyList<MetadataCandidate>>> SearchAsync(
            MetadataProviderKind provider,
            string query,
            CancellationToken cancellationToken
        );

        /// <summary>Lädt einen Eintrag über seine Id bei der Quelle.</summary>
        Task<MetadataResult<MetadataCandidate>> GetByIdAsync(
            MetadataProviderKind provider,
            string externalId,
            CancellationToken cancellationToken
        );

        /// <summary>Lädt ein Cover (nur https, höchstens 10 MB).</summary>
        Task<MetadataResult<byte[]>> DownloadCoverAsync(string url, CancellationToken cancellationToken);
    }
}
