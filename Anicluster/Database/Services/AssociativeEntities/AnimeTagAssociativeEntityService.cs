using Microsoft.Data.Sqlite;
using Serilog;
using System.Data;

namespace Anicluster.Database.Services.AssociativeEntities {

    public class AnimeTagAssociativeEntityService : IDatabaseService {

        private readonly DatabaseManager _databaseManager;

        public AnimeTagAssociativeEntityService(DatabaseManager databaseManager) {
            this._databaseManager = databaseManager;
        }

        public bool InitializeTable() {
            using (SqliteConnection connection = this._databaseManager.GetConnection()) {
                string creationString = "" +
                    "CREATE TABLE IF NOT EXISTS AnimeTagAssociativeEntity (\n" +
                    "    AnimeId INTEGER,\n" +
                    "    TagId INTEGER,\n" +
                    "    PRIMARY KEY(AnimeId, TagId),\n" +
                    "    FOREIGN KEY(AnimeId) REFERENCES Anime(Id),\n" +
                    "    FOREIGN KEY(TagId) REFERENCES Tag(Id)\n" +
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
            using (SqliteConnection connection = this._databaseManager.GetConnection()) {
                connection.Open();
                string query = "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName;";
                using (SqliteCommand cmd = new SqliteCommand(query, connection)) {
                    cmd.Parameters.AddWithValue("@tableName", "AnimeTagAssociativeEntity");
                    object? result = cmd.ExecuteScalar();
                    connection.Close();
                    if (result is not null) {
                        Log.Information("Tabelle 'AnimeTagAssociativeEntity' existiert.");
                        return true;
                    }
                    else {
                        Log.Information("Tabelle 'AnimetagAssociativeEntity' existiert NICHT!");
                        return false;
                    }
                }
            }
        }

        public void Insert(int animeId, int tagId) {
            using (SqliteConnection connection = this._databaseManager.GetConnection()) {
                connection.Open();
                string query = "" +
                    "INSERT INTO AnimeTagAssociativeEntity (AnimeId, TagId)\n" +
                    "    VALUES (@AnimeId, @TagId);";
                using (SqliteCommand cmd = new SqliteCommand(query, connection)) {
                    cmd.Parameters.AddWithValue("@AnimeId", animeId);
                    cmd.Parameters.AddWithValue("@TagId", tagId);
                    cmd.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        public List<int> GetTagIdsForAnimeId(int id = -1) {
            string query = $"" +
                $"SELECT TagId\n" +
                $"FROM AnimeTagAssociativeEntity\n" +
                $"WHERE\n" +
                $"    AnimeId={id}";

            DataTable resDt = new DataTable();
            using (SqliteConnection connection = this._databaseManager.GetConnection()) {
                using (SqliteCommand cmd = new SqliteCommand(query, connection)) {
                    connection.Open();
                    SqliteDataReader result = cmd.ExecuteReader();
                    resDt.Load(result);
                    connection.Close();
                }
            }
            return resDt.AsEnumerable().Select(row => Convert.ToInt32(row.Field<long>("TagId"))).ToList();
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

        //public List<Tag> GetTagsForAnime(int animeId) {
        //    
        //    }

        //    return tags;
        //}
    }
}
