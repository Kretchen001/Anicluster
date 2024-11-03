using Microsoft.Data.Sqlite;
using Serilog;

namespace Anicluster.Database.Services {
    public class AnimeService {

        private readonly DatabaseManager _databaseManager;

        public AnimeService(DatabaseManager databaseManager) {
            _databaseManager = databaseManager;
        }

        public bool InitializeAnimeTable() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                string creationString = "" +
                    "CREATE TABLE IF NOT EXISTS Anime (" +
                    "    Id INTEGER PRIMARY KEY," +
                    "    Name TEXT NOT NULL," +
                    "    OriginalName TEXT," +
                    "    Url TEXT," +
                    "    Favorite BOOLEAN NOT NULL DEFAULT 0," +
                    "    RatingId INTEGER," +
                    "    Tier TEXT NOT NULL," +
                    "    RecommendedFrom TEXT," +
                    "    Predecessor INTEGER," +
                    "    Successor INTEGER," +
                    "    Related INTEGER," +
                    "    Comment TEXT," +
                    "    FOREIGN KEY (RatingId) REFERENCES Rating(Id)" +
                    ");";
                using (SqliteCommand cmd = new SqliteCommand(creationString, connection)) {
                    try {
                    connection.Open();
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex) {
                        Log.Error(ex.StackTrace ?? "Error without stacktrace... <- Anime table not created!");
                        return false;
                    }
                }
            }
            Log.Information("Anime table created.");
            return true;
        }

        public bool TableExists() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                connection.Open();
                string query = "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName;";
                using (SqliteCommand cmd = new SqliteCommand(query, connection)) {
                    cmd.Parameters.AddWithValue("@tableName", "Anime");
                    object? result = cmd.ExecuteScalar();

                    return result != null;
                }
            }
        }
    }
}
