namespace Anicluster.Metadata {

    /// <summary>Eine einzelne Online-Quelle hinter dem <see cref="MetadataClient"/>.</summary>
    internal interface IMetadataProvider {

        MetadataProviderKind Kind { get; }

        Task<MetadataResult<IReadOnlyList<MetadataCandidate>>> SearchAsync(string query, CancellationToken cancellationToken);

        Task<MetadataResult<MetadataCandidate>> GetByIdAsync(string externalId, CancellationToken cancellationToken);
    }
}
