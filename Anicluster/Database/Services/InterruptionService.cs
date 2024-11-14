using Microsoft.Data.Sqlite;
using Modells.Anime;
using Serilog;

namespace Anicluster.Database.Services {

    public class InterruptionService {

        private readonly DatabaseManager _databaseManager;

        public InterruptionService(DatabaseManager databaseManager) {
            _databaseManager = databaseManager;
        }

        public bool InitializeInterruptionTable() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                string creationString = "" +
                    "CREATE TABLE IF NOT EXISTS Interruption (\n" +
                    "    Id INTEGER PRIMARY KEY,\n" +
                    "    StartDate DATETIME,\n" +
                    "    EndDate DATETIME,\n" +
                    "    Comment TEXT\n" +
                    ");";
                using (SqliteCommand cmd = new SqliteCommand(creationString, connection)) {
                    try {
                        connection.Open();
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex) {
                        Log.Error(ex.StackTrace ?? "Error without stacktrace... <- Interruption table not created!");
                        return false;
                    }
                }
            }
            Log.Information("Interruption table created.");
            return true;
        }

        public bool TableExist() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                connection.Open();
                string query = "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName;";
                using (SqliteCommand cmd = new SqliteCommand(query, connection)) {
                    cmd.Parameters.AddWithValue("@tableName", "Interruption");
                    object? result = cmd.ExecuteScalar();

                    return result != null;
                }
            }
        }

        public int Insert(Interruption interruption) {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                string queryInsert = "" +
                    "INSERT INTO Tag (StartDate, EndDate, Comment) \n" +
                    "    VALUES (@StartDate, @EndDate, @Comment);";
                using (SqliteCommand cmd = new SqliteCommand(queryInsert, connection)) {
                    try {
                        cmd.Parameters.AddWithValue("@StartDate", interruption.Start.ToString("G"));
                        cmd.Parameters.AddWithValue("@EndDate", interruption.End.ToString("G"));
                        cmd.Parameters.AddWithValue("@Comment", interruption.Comment ?? "");
                        connection.Open();
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = $"SELECT Id FROM Tag WHERE StartDate Like '{interruption.Start:G}' AND EndDate Like '{interruption.End:G}'";
                        object? insertedId = cmd.ExecuteScalar();
                        connection.Close();
                        return insertedId is not null ? (int)((long)insertedId) : -1;
                    }
                    catch (Exception ex) {
                        Log.Error(ex.StackTrace ?? "Error without stacktrace... <- Interruption not inserted or queryable!");
                        return -1;
                    }
                }
            }
        }
    }
}
