using Microsoft.Data.Sqlite;
using Serilog;

namespace Anicluster.Database.Services.AssociativeEntities {
    internal class AnimeOvaAssociativeEntityService : IDatabaseService {

        private readonly DatabaseManager _databaseManager;

        public AnimeOvaAssociativeEntityService(DatabaseManager databaseManager) {
            _databaseManager = databaseManager;
        }

        public bool InitializeTable() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                string creationString = "" +
                    "CREATE TABLE IF NOT EXISTS AnimeOvaAssociativeEntityService (\n" +
                    "    AnimeId INTEGER,\n" +
                    "    VideoAnimationId INTEGER,\n" +
                    "    PRIMARY KEY(AnimeId, VideoAnimationId),\n" +
                    "    FOREIGN KEY(AnimeId) REFERENCES Anime(Id),\n" +
                    "    FOREIGN KEY(VideoAnimationId) REFERENCES VideoAnimation(Id)\n" +
                    ");";
                using (SqliteCommand cmd = new SqliteCommand(creationString, connection)) {
                    try {
                        connection.Open();
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex) {
                        Log.Error(ex.StackTrace ?? "Error without stacktrace... <- AnimeOvaAssociativeEntityService table not created!");
                        return false;
                    }
                }
            }
            Log.Information("AnimeOvaAssociativeEntityService table created.");
            return true;
        }

        public bool TableExist() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                connection.Open();
                string query = "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName;";
                using (SqliteCommand cmd = new SqliteCommand(query, connection)) {
                    cmd.Parameters.AddWithValue("@tableName", "AnimeOvaAssociativeEntityService");
                    object? result = cmd.ExecuteScalar();

                    return result != null;
                }
            }
        }

        public void Insert(int animeId, int vaId) {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                connection.Open();
                string query = "INSERT INTO AnimeOvaAssociativeEntityService (AnimeId, VideoAnimationId)\n" +
                    "VALUES (@AnimeId, @VideoAnimationId);";

                using (SqliteCommand cmd = new SqliteCommand(query, connection)) {
                    cmd.Parameters.AddWithValue("@AnimeId", animeId);
                    cmd.Parameters.AddWithValue("@VideoAnimationId", vaId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
