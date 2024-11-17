using Microsoft.Data.Sqlite;
using Modells.Anime;
using Serilog;

namespace Anicluster.Database.Services {

    public class StatusService : IDatabaseService {

        private readonly DatabaseManager _databaseManager;

        public StatusService(DatabaseManager databaseManager) {
            _databaseManager = databaseManager;
        }

        public bool InitializeTable() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                string creationString = "" +
                    "CREATE TABLE IF NOT EXISTS Status (" +
                    "    Id INTEGER PRIMARY KEY," +
                    "    State INTEGER NOT NULL," +
                    "    Comment TEXT" +
                    ");";
                using (SqliteCommand cmd = new SqliteCommand(creationString, connection)) {
                    try {
                        connection.Open();
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex) {
                        Log.Error(ex.StackTrace ?? "Error without stacktrace... <- Status table not created!");
                        return false;
                    }
                }
            }
            Log.Information("Status table created.");
            return true;
        }

        public bool TableExist() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                connection.Open();
                string query = "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName;";
                using (SqliteCommand cmd = new SqliteCommand(query, connection)) {
                    cmd.Parameters.AddWithValue("@tableName", "Status");
                    object? result = cmd.ExecuteScalar();

                    return result != null;
                }
            }
        }

        public int InsertStatus(Status statusToInsert) {
            int statusId = 0;
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                connection.Open();
                using (SqliteTransaction transaction = connection.BeginTransaction()) {
                    try {
                        string insertCommand = "" +
                            "INSERT INTO Status (State, Comment) " +
                            "    VALUES (@state, @comment)";
                        using (SqliteCommand command = new SqliteCommand(insertCommand, connection)) {
                            command.Parameters.AddWithValue("@state", ((int)statusToInsert.State));
                            command.Parameters.AddWithValue("@comment", statusToInsert.Comment ?? "");
                            
                            command.ExecuteNonQuery();
                        }
                        using (SqliteCommand cmd = new SqliteCommand("SELECT last_insert_rowid();", connection)) {
                            object? result = cmd.ExecuteScalar();
                            _ = int.TryParse(result as string, out statusId);
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex) {
                        transaction.Rollback();
                        Log.Error(ex.StackTrace ?? "Error while inserting statusToInsert data");
                        return -1;
                    }
                }
                connection.Close();
            }
            return statusId;
        }
    }
}
