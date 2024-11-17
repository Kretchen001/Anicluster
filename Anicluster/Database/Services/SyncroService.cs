using Microsoft.Data.Sqlite;
using Serilog;

namespace Anicluster.Database.Services {

    public class SyncroService : IDatabaseService {

        private readonly DatabaseManager _databaseManager;

        public SyncroService(DatabaseManager databaseManager) {
            _databaseManager = databaseManager;
        }

        public bool InitializeTable() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                string creationString = "" +
                    "CREATE TABLE IF NOT EXISTS Syncro (" +
                    "    Id INTEGER PRIMARY KEY," +
                    "    General INTEGER," +
                    "    Language INTEGER," +
                    "    IsAssessed BOOLEAN," +
                    "    Comment TEXT," +
                    "    AcousticId INTEGER" +
                    ");";
                using (SqliteCommand cmd = new SqliteCommand(creationString, connection)) {
                    try {
                        connection.Open();
                        cmd.ExecuteNonQuery();
                        connection.Close();
                    }
                    catch (Exception ex) {
                        Log.Error(ex.StackTrace ?? "Error without stacktrace... <- Syncro table not created!");
                        return false;
                    }
                }
            }
            Log.Information("Syncro table created.");
            return true;
        }

        public bool TableExist() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                connection.Open();
                string query = "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName;";
                using (SqliteCommand cmd = new SqliteCommand(query, connection)) {
                    cmd.Parameters.AddWithValue("@tableName", "Syncro");
                    object? result = cmd.ExecuteScalar();

                    return result != null;
                }
            }
        }
    }
}
