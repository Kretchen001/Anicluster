namespace Modells.Anime {

    /// <summary>Ein Tag mit der Anzahl der Animes, denen er zugeordnet ist.</summary>
    public record TagUsage(int Id, string Designation, int AnimeCount);

    /// <summary>Wie viele Animes beide Tags gemeinsam haben (<see cref="TagIdA"/> ist immer kleiner als <see cref="TagIdB"/>).</summary>
    public record TagLink(int TagIdA, int TagIdB, int AnimeCount);
}
