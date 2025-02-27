using Microsoft.Data.Sqlite;
using Modells.Anime;
using Serilog;
using System.Data;

namespace Anicluster.Database.Services {

    public class StatusService : IDatabaseService {

        private readonly DatabaseManager _databaseManager;

        public StatusService(DatabaseManager databaseManager) {
            _databaseManager = databaseManager;
        }

        public bool InitializeTable() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                string creationString = @"
                    CREATE TABLE IF NOT EXISTS Status (
                        Id INTEGER PRIMARY KEY,
                        State INTEGER NOT NULL,
                        Comment TEXT
                    );
                ";
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
                    connection.Close();
                    if (result is not null) {
                        Log.Information("Tabelle 'Status' existiert.");
                        return true;
                    }
                    else {
                        Log.Information("Tabelle 'Status' existiert NICHT!");
                        return false;
                    }
                }
            }
        }

        public int Insert(Status statusToInsert) {
            int statusId = 0;
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                connection.Open();
                try {
                    string insertCommand = "" +
                        "INSERT INTO Status (State, Comment) " +
                        "    VALUES (@state, @comment)";
                    using (SqliteCommand cmd = new SqliteCommand(insertCommand, connection)) {
                        cmd.Parameters.AddWithValue("@state", ((int)statusToInsert.State));
                        cmd.Parameters.AddWithValue("@comment", statusToInsert.Comment ?? "");
                        cmd.ExecuteNonQuery();
                    }
                    using (SqliteCommand cmd = new SqliteCommand("SELECT last_insert_rowid();", connection)) {
                        object? result = cmd.ExecuteScalar();
                        statusId = (int)((long)result!);
                    }
                }
                catch (Exception ex) {
                    Log.Error(ex.StackTrace ?? "Error while inserting statusToInsert data");
                    return -1;
                }
                connection.Close();
            }
            return statusId;
        }

        public Status GetStatusById(int id = -1) {
            string query = $"" +
                $"SELECT Id, State, Comment\n" +
                $"FROM Status\n" +
                $"WHERE Id = '{id}'";
            DataTable resDt = new DataTable();
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                using (SqliteCommand cmd = new SqliteCommand(query, connection)) {
                    connection.Open();
                    SqliteDataReader result = cmd.ExecuteReader();
                    resDt.Load(result);
                    connection.Close();
                }
            }
            return new Status(
                id: (int)((long)resDt.Rows[0]["Id"]),
                state: (State)Enum.Parse(typeof(State), resDt.Rows[0]["Status"].ToString() ?? "0"),
                comment: resDt.Rows[0]["Comment"].ToString()
            );
        }
    }
}
