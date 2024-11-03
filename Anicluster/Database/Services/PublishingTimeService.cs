using Microsoft.Data.Sqlite;
using Serilog;

namespace Anicluster.Database.Services {

    public class PublishingTimeService {

        private readonly DatabaseManager _databaseManager;

        public PublishingTimeService(DatabaseManager databaseManager) {
            _databaseManager = databaseManager;
        }

        public bool InitializePublishingTimeTable() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                string creationString = "" +
                    "CREATE TABLE IF NOT EXISTS PublishingTime (" +
                    "    Id INTEGER PRIMARY KEY," +
                    "    MediaInfoId INTEGER," +
                    "    StartDate DATETIME," +
                    "    EndDate DATETIME," +
                    "    DisplayStartYear TEXT," +
                    "    DisplayEndYear TEXT" +
                    ");";
                using (SqliteCommand cmd = new SqliteCommand(creationString, connection)) {
                    try {
                        connection.Open();
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex) {
                        Log.Error(ex.StackTrace ?? "Error without stacktrace... <- PublishingTime table not created!");
                        return false;
                    }
                }
            }
            Log.Information("PublishingTime table created.");
            return true;
        }

        public bool TableExist() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                connection.Open();
                string query = "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName;";
                using (SqliteCommand cmd = new SqliteCommand(query, connection)) {
                    cmd.Parameters.AddWithValue("@tableName", "PublishingTime");
                    object? result = cmd.ExecuteScalar();

                    return result != null;
                }
            }
        }
    }
}
