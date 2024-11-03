using Microsoft.Data.Sqlite;
using Serilog;

namespace Anicluster.Database.Services {

    public class SeasonService {

        private readonly DatabaseManager _databaseManager;

        public SeasonService(DatabaseManager databaseManager) {
            _databaseManager = databaseManager;
        }

        public bool InitializeSeasonTable() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                string creationString = "" +
                    "CREATE TABLE IF NOT EXISTS Season (" +
                    "    Id INTEGER PRIMARY KEY," +
                    "    AnimeId INTEGER," +
                    "    Number INTEGER," +
                    "    Comment TEXT," +
                    "    PublishingTimeId INTEGER," +
                    "    FOREIGN KEY(AnimeId) REFERENCES Anime(Id)," +
                    "    FOREIGN KEY(PublishingTimeId) REFERENCES PublishingTime(Id)" +
                    ");";
                using (SqliteCommand cmd = new SqliteCommand(creationString, connection)) {
                    try {
                        connection.Open();
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex) {
                        Log.Error(ex.StackTrace ?? "Error without stacktrace... <- Season table not created!");
                        return false;
                    }
                }
            }
            Log.Information("Season table created.");
            return true;
        }

        public bool TableExist() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                connection.Open();
                string query = "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName;";
                using (SqliteCommand cmd = new SqliteCommand(query, connection)) {
                    cmd.Parameters.AddWithValue("@tableName", "Season");
                    object? result = cmd.ExecuteScalar();

                    return result != null;
                }
            }
        }
    }
}
