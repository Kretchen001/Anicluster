using Microsoft.Data.Sqlite;
using Serilog;

namespace Anicluster.Database.Services {

    public class TagService {

        private readonly DatabaseManager _databaseManager;

        public TagService(DatabaseManager databaseManager) {
            _databaseManager = databaseManager;
        }

        public bool InitializeTagTable() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                string creationString = "" +
                    "CREATE TABLE IF NOT EXISTS Tag (" +
                    "    Id INTEGER PRIMARY KEY," +
                    "    Designation TEXT NOT NULL" +
                    ");";
                using (SqliteCommand cmd = new SqliteCommand(creationString, connection)) {
                    try {
                        connection.Open();
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex) {
                        Log.Error(ex.StackTrace ?? "Error without stacktrace... <- Tag table not created!");
                        return false;
                    }
                }
            }
            Log.Information("Tag table created.");
            return true;
        }

        public bool TableExist() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                connection.Open();
                string query = "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName;";
                using (SqliteCommand cmd = new SqliteCommand(query, connection)) {
                    cmd.Parameters.AddWithValue("@tableName", "Tag");
                    object? result = cmd.ExecuteScalar();

                    return result != null;
                }
            }
        }
    }
}
