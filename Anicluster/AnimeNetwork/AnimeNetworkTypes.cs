namespace Anicluster.AnimeNetwork {

    /// <summary>Die drei Netzwerkarten des Anime-Netzwerks.</summary>
    public enum AnimeNetworkMode {
        /// <summary>Vorgänger, Nachfolger und Verwandt, wie bei den Animes eingetragen</summary>
        Relations,

        /// <summary>Cluster nach ähnlichen Tags</summary>
        Tags,

        /// <summary>Cluster nach ähnlicher Bewertung</summary>
        Ratings
    }

    /// <summary>Art einer Verbindung zwischen zwei Animes.</summary>
    public enum AnimeEdgeKind {
        /// <summary>Ähnlichkeit (Tags oder Bewertung), ohne Richtung</summary>
        Similarity,

        /// <summary>Vorgänger → Nachfolger, gerichtet</summary>
        Successor,

        /// <summary>Sonst zusammenhängend, ohne Richtung</summary>
        Related
    }

    /// <summary>
    /// Eine Verbindung zwischen zwei Animes. Bei <see cref="AnimeEdgeKind.Successor"/> ist <paramref name="IdA"/> der Vorgänger
    /// und <paramref name="IdB"/> der Nachfolger. <paramref name="Weight"/> liegt zwischen 0 und 1 (Stärke der Verbindung).
    /// </summary>
    public record AnimeNetworkEdge(int IdA, int IdB, double Weight, AnimeEdgeKind Kind);

    /// <summary>
    /// Ergebnis der Netzwerkberechnung: die Verbindungen, die Cluster-Nummer jedes verbundenen Animes
    /// (0 ist der größte Cluster) und die Ids aller Animes, für die in dieser Netzwerkart überhaupt Daten vorliegen
    /// (zum Beispiel Animes mit Tags oder mit Bewertung).
    /// </summary>
    public record AnimeNetworkResult(
        IReadOnlyList<AnimeNetworkEdge> Edges,
        IReadOnlyDictionary<int, int> Clusters,
        IReadOnlyCollection<int> EligibleIds);
}
