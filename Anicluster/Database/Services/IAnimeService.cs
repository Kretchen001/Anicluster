using Modells.Anime;

namespace Anicluster.Database.Services {

    /// <summary>Zugriff auf Animes in der Datenbank.</summary>
    public interface IAnimeService {

        /// <summary>Fügt einen kompletten Anime inklusive Bewertung, Staffeln, Tags usw. ein.</summary>
        bool Insert(Anime animeToInsert);

        /// <summary>Fügt einen Anime nur mit Name, Favorit und Tier ein.</summary>
        bool InsertFast(string animeName, bool animeFavorite, Tier animeTier);

        /// <summary>Lädt einen kompletten Anime per Id oder Name. <c>null</c>, wenn er nicht gefunden wurde.</summary>
        Anime? SelectAnimeByX(int id = -1, string designation = "");

        /// <summary>Aktualisiert die Stammdaten sowie Bewertung, Medieninfo und Status eines vorhandenen Animes.</summary>
        bool UpdateAnime(Anime animeToUpdate);

        /// <summary>Lädt alle Animes in der schlanken Form für die Listenansicht.</summary>
        List<AnimeListEntry> SelectAnimeListEntries();
    }
}
