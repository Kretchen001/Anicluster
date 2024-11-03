using Microsoft.Data.Sqlite;
using Serilog;

namespace Anicluster.Database.Services {
    
    public class RatingService {

        private readonly DatabaseManager _databaseManager;

        public RatingService(DatabaseManager databaseManager) {
            _databaseManager = databaseManager;
        }

        public bool InitializeRatingTable() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                string creationString = "" +
                    "CREATE TABLE IF NOT EXISTS Rating (" +
                    "    Id INTEGER PRIMARY KEY," +
                    "    Story INTEGER," +
                    "    Animation INTEGER," +
                    "    SpecialEffects INTEGER," +
                    "    AcousticId INTEGER," +
                    "    IsRated BOOLEAN," +
                    "    FOREIGN KEY(AcousticId) REFERENCES Acoustic(Id)" +
                    ");";
                using (SqliteCommand cmd = new SqliteCommand(creationString, connection)) {
                    try {
                        connection.Open();
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex) {
                        Log.Error(ex.StackTrace ?? "Error without stacktrace... <- Rating table not created!");
                        return false;
                    }
                }
            }
            Log.Information("Rating table created.");
            return true;
        }

        public bool TableExist() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                connection.Open();
                string query = "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName;";
                using (SqliteCommand cmd = new SqliteCommand(query, connection)) {
                    cmd.Parameters.AddWithValue("@tableName", "Rating");
                    object? result = cmd.ExecuteScalar();

                    return result != null;
                }
            }
        }
    }
}
