using Modells.Anime;

namespace Anicluster.Database.Services {

    /// <summary>Zugriff auf Tags in der Datenbank.</summary>
    public interface ITagService {

        /// <summary>Lädt alle Tags alphabetisch sortiert.</summary>
        List<Tag> SelectAllTags();

        /// <summary>Fügt den Tag ein, setzt seine Id und liefert sie (-1 bei Fehler).</summary>
        int Insert(Tag tag);

        /// <summary>Löscht den Tag, die Zuordnungen zu Animes verschwinden automatisch.</summary>
        bool DeleteTagById(Tag tagToDelete);
    }
}
