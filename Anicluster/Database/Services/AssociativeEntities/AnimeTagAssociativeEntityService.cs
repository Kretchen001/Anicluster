using Microsoft.Data.Sqlite;
using Serilog;

namespace Anicluster.Database.Services.AssociativeEntities {

    public class AnimeTagAssociativeEntityService : IDatabaseService {

        private readonly DatabaseManager _databaseManager;

        public AnimeTagAssociativeEntityService(DatabaseManager databaseManager) {
            _databaseManager = databaseManager;
        }

        public bool InitializeTable() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                string creationString = "" +
                    "CREATE TABLE IF NOT EXISTS AnimeTagAssociativeEntity (" +
                    "    AnimeId INTEGER," +
                    "    TagId INTEGER," +
                    "    PRIMARY KEY(AnimeId, TagId),  --Kombinierter Primärschlüssel" +
                    "    FOREIGN KEY(AnimeId) REFERENCES Anime(Id)," +
                    "    FOREIGN KEY(TagId) REFERENCES Tag(Id)" +
                    ");";
                using (SqliteCommand cmd = new SqliteCommand(creationString, connection)) {
                    try {
                        connection.Open();
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex) {
                        Log.Error(ex.StackTrace ?? "Error without stacktrace... <- AnimeTagAssociativeEntity table not created!");
                        return false;
                    }
                }
            }
            Log.Information("AnimeTagAssociativeEntity table created.");
            return true;
        }

        public bool TableExist() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                connection.Open();
                string query = "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName;";
                using (SqliteCommand cmd = new SqliteCommand(query, connection)) {
                    cmd.Parameters.AddWithValue("@tableName", "AnimeTagAssociativeEntity");
                    object? result = cmd.ExecuteScalar();

                    return result != null;
                }
            }
        }

        public void AddTagToAnime(int animeId, int tagId) {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                connection.Open();
                string query = "INSERT INTO AnimeTagAssociativeEntity (AnimeId, TagId) VALUES (@AnimeId, @TagId);";

                using (SqliteCommand cmd = new SqliteCommand(query, connection)) {
                    cmd.Parameters.AddWithValue("@AnimeId", animeId);
                    cmd.Parameters.AddWithValue("@TagId", tagId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        //public void RemoveTagFromAnime(int animeId, int tagId) {
        //    using (var connection = _databaseManager.GetConnection()) {
        //        connection.Open();
        //        string query = "DELETE FROM AnimeTag WHERE AnimeId = @AnimeId AND TagId = @TagId;";

        //        using (var cmd = new SqliteCommand(query, connection)) {
        //            cmd.Parameters.AddWithValue("@AnimeId", animeId);
        //            cmd.Parameters.AddWithValue("@TagId", tagId);
        //            cmd.ExecuteNonQuery();
        //        }
        //    }
        //}

        //public List<int> GetTagsForAnime(int animeId) {
        //    var tagIds = new List<int>();

        //    using (var connection = _databaseManager.GetConnection()) {
        //        connection.Open();
        //        string query = "SELECT TagId FROM AnimeTag WHERE AnimeId = @AnimeId;";

        //        using (var cmd = new SqliteCommand(query, connection)) {
        //            cmd.Parameters.AddWithValue("@AnimeId", animeId);
        //            using (var reader = cmd.ExecuteReader()) {
        //                while (reader.Read()) {
        //                    tagIds.Add(reader.GetInt32(0));
        //                }
        //            }
        //        }
        //    }

        //    return tagIds;
        //}
    }
}
