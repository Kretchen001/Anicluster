namespace Modells.Anime {

    /// <summary>
    /// Die Verknüpfungen eines Animes zu anderen Animes (Vorgänger, Nachfolger, Verwandt) für das Anime-Netzwerk.
    /// Der Wert -1 bedeutet, dass keine Verknüpfung eingetragen ist.
    /// </summary>
    public record AnimeRelationEntry(int Id, int Predecessor, int Successor, int Related);
}
