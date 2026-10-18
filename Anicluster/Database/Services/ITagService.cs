using Modells.Anime;

namespace Anicluster.Database.Services {

    /// <summary>Zugriff auf Tags in der Datenbank.</summary>
    public interface ITagService {

        List<Tag> SelectAllTags();

        /// <summary>Fügt den Tag ein und liefert die vergebene Id (-1 bei Fehler).</summary>
        int Insert(Tag tag);

        bool DeleteTagById(Tag tagToDelete);
    }
}
