namespace Anicluster.Metadata {

    /// <summary>Die Online-Quellen, aus denen Metadaten (Cover, Beschreibung, Zeitraum) geladen werden können.</summary>
    public enum MetadataProviderKind {
        AniList,
        Jikan,
        Kitsu
    }

    /// <summary>Anzeigetexte zu den Quellen.</summary>
    public static class MetadataProviderKindExtensions {

        public static string GetDisplayName(this MetadataProviderKind kind) {
            return kind switch {
                MetadataProviderKind.AniList => "AniList",
                MetadataProviderKind.Jikan => "Jikan (MyAnimeList)",
                MetadataProviderKind.Kitsu => "Kitsu",
                _ => kind.ToString()
            };
        }
    }
}
