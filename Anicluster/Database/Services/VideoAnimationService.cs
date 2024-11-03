using Microsoft.Data.Sqlite;
using Serilog;

namespace Anicluster.Database.Services {
    internal class VideoAnimationService {

        private readonly DatabaseManager _databaseManager;

        public VideoAnimationService(DatabaseManager databaseManager) {
            _databaseManager = databaseManager;
        }

        public bool InitializeVideoAnimationTable() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                string creationString = "" +
                    "CREATE TABLE IF NOT EXISTS VideoAnimation (" +
                    "    Id INTEGER PRIMARY KEY," +
                    "    SeasonId INTEGER," +
                    "    AnimeId INTEGER," +
                    "    VideoType INTEGER," +
                    "    Comment TEXT," +
                    "    FOREIGN KEY(SeasonId) REFERENCES Season(Id)" +
                    ");";
                using (SqliteCommand cmd = new SqliteCommand(creationString, connection)) {
                    try {
                        connection.Open();
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex) {
                        Log.Error(ex.StackTrace ?? "Error without stacktrace... <- VideoAnimation table not created!");
                        return false;
                    }
                }
            }
            Log.Information("VideoAnimation table created.");
            return true;
        }

        public bool TableExist() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                connection.Open();
                string query = "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName;";
                using (SqliteCommand cmd = new SqliteCommand(query, connection)) {
                    cmd.Parameters.AddWithValue("@tableName", "VideoAnimation");
                    object? result = cmd.ExecuteScalar();

                    return result != null;
                }
            }
        }
    }
}
