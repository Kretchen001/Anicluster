using Modells.Anime;
using Serilog;

namespace Anicluster.Database.Services {

    /// <summary>
    /// Liest und schreibt Tags und deren Zuordnung zu Animes.
    /// </summary>
    public class TagService : ITagService {

        private readonly DatabaseManager _databaseManager;

        public TagService(DatabaseManager databaseManager) {
            this._databaseManager = databaseManager;
        }

        public List<Tag> SelectAllTags() {
            try {
                using (DatabaseSession session = this._databaseManager.BeginSession()) {
                    return session.Query(
                        "SELECT Id, Designation FROM Tag ORDER BY Designation COLLATE NOCASE;",
                        reader => new Tag(reader.GetString(1)) { Id = reader.GetInt(0) }
                    );
                }
            }
            catch (Exception ex) {
                Log.Error(ex, "Tags konnten nicht geladen werden.");
                return [];
            }
        }

        public int Insert(Tag tag) {
            try {
                using (DatabaseSession session = this._databaseManager.BeginSession()) {
                    tag.Id = session.Insert("INSERT INTO Tag (Designation) VALUES (@Designation);", ("@Designation", tag.Designation));
                    session.Commit();
                    return tag.Id;
                }
            }
            catch (Exception ex) {
                Log.Error(ex, "Tag '{Designation}' konnte nicht eingefügt werden.", tag.Designation);
                return -1;
            }
        }

        public bool DeleteTagById(Tag tagToDelete) {
            try {
                using (DatabaseSession session = this._databaseManager.BeginSession()) {
                    int affectedRows = session.Execute("DELETE FROM Tag WHERE Id = @Id;", ("@Id", tagToDelete.Id));
                    session.Commit();
                    return affectedRows > 0;
                }
            }
            catch (Exception ex) {
                Log.Error(ex, "Tag '{Designation}' konnte nicht gelöscht werden.", tagToDelete.Designation);
                return false;
            }
        }

        /// <summary>Lädt die Tags eines Animes alphabetisch sortiert.</summary>
        internal List<Tag> SelectByAnimeId(DatabaseSession session, int animeId) {
            return session.Query(
                "SELECT t.Id, t.Designation FROM Tag t JOIN AnimeTagAssociativeEntity m ON m.TagId = t.Id WHERE m.AnimeId = @AnimeId ORDER BY t.Designation COLLATE NOCASE;",
                reader => new Tag(reader.GetString(1)) { Id = reader.GetInt(0) },
                ("@AnimeId", animeId)
            );
        }

        /// <summary>Ersetzt die Tag-Zuordnung des Animes. Die Tags selbst müssen bereits existieren.</summary>
        internal void ReplaceForAnime(DatabaseSession session, int animeId, List<Tag> tags) {
            session.Execute("DELETE FROM AnimeTagAssociativeEntity WHERE AnimeId = @AnimeId;", ("@AnimeId", animeId));
            foreach (Tag tag in tags) {
                session.Execute(
                    "INSERT OR IGNORE INTO AnimeTagAssociativeEntity (AnimeId, TagId) VALUES (@AnimeId, @TagId);",
                    ("@AnimeId", animeId), ("@TagId", tag.Id)
                );
            }
        }
    }
}
