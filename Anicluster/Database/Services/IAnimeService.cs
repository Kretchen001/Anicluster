using Modells.Anime;

namespace Anicluster.Database.Services {

    /// <summary>Zugriff auf Animes in der Datenbank. Jede Methode läuft in einer Transaktion: ganz oder gar nicht.</summary>
    public interface IAnimeService {

        /// <summary>Fügt einen kompletten Anime inklusive Status, Bewertung, Staffeln, Tags usw. ein und setzt die Ids im Modell.</summary>
        bool Insert(Anime animeToInsert);

        /// <summary>Fügt einen Anime nur mit Name, Favorit und Tier ein (alle übrigen Daten mit Standardwerten).</summary>
        bool InsertFast(string animeName, bool animeFavorite, Tier animeTier);

        /// <summary>Lädt einen kompletten Anime per Id oder Name. <c>null</c>, wenn er nicht gefunden wurde oder das Laden fehlschlug.</summary>
        Anime? SelectAnimeByX(int id = -1, string designation = "");

        /// <summary>Speichert alle Daten eines vorhandenen Animes (auch Staffeln, OVAs, Tags und Bewertung).</summary>
        bool UpdateAnime(Anime animeToUpdate);

        /// <summary>Löscht den Anime mit allen zugehörigen Daten. Verweise anderer Animes (Vorgänger, Nachfolger, Verwandt) werden zurückgesetzt.</summary>
        bool DeleteAnime(int animeId);

        /// <summary>Lädt alle Animes in der schlanken Form für die Listenansicht.</summary>
        List<AnimeListEntry> SelectAnimeListEntries();
    }
}
