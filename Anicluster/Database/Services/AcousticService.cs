using Microsoft.Data.Sqlite;
using Serilog;

namespace Anicluster.Database.Services {

    public class AcousticService {

        private readonly DatabaseManager _databaseManager;

        public AcousticService(DatabaseManager databaseManager) {
            _databaseManager = databaseManager;
        }

        public bool InitializeAcousticTable() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                string creationString = "" +
                    "CREATE TABLE IF NOT EXISTS Acoustic (" +
                    "    Id INTEGER PRIMARY KEY," +
                    "    Soundtrack INTEGER," +
                    "    Comment TEXT" +
                    ");";
                using (SqliteCommand cmd = new SqliteCommand(creationString, connection)) {
                    try {
                        connection.Open();
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex) {
                        Log.Error(ex.StackTrace ?? "Error without stacktrace... <- Acoustic table not created!");
                        return false;
                    }
                }
            }
            Log.Information("Acoustic table created.");
            return true;
        }

        public bool TableExist() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                connection.Open();
                string query = "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName;";
                using (SqliteCommand cmd = new SqliteCommand(query, connection)) {
                    cmd.Parameters.AddWithValue("@tableName", "Acoustic");
                    object? result = cmd.ExecuteScalar();

                    return result != null;
                }
            }
        }
    }
}
