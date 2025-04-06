using Microsoft.Data.Sqlite;
using Serilog;
using System.Reflection;

namespace Anicluster.Database.Services {

    class SysService {

        private readonly DatabaseManager _databaseManager;

        public SysService(DatabaseManager databaseManager) {
            _databaseManager = databaseManager;
        }

        public bool InitializeTable() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                string creationString = @"
                    CREATE TABLE IF NOT EXISTS _Sys (
                        Id INTEGER PRIMARY KEY,
                        Version TEXT
                    );
                    INSERT INTO _Sys (Version) VALUES ('0.0.0');
                ";
                using (SqliteCommand cmd = new SqliteCommand(creationString, connection)) {
                    try {
                        connection.Open();
                        cmd.ExecuteNonQuery();
                        connection.Close();
                    }
                    catch (Exception ex) {
                        Log.Error(ex.StackTrace ?? "Error without stacktrace... <- _Sys table not created!");
                        return false;
                    }
                }
            }
            Log.Information("_Sys table created.");
            return true;
        }

        public bool TableExist() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                connection.Open();
                string query = "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName;";
                using (SqliteCommand cmd = new SqliteCommand(query, connection)) {
                    cmd.Parameters.AddWithValue("@tableName", "_Sys");
                    object? result = cmd.ExecuteScalar();
                    connection.Close();
                    if (result is not null) {
                        Log.Information("Tabelle '_Sys' existiert.");
                        return true;
                    }
                    else {
                        Log.Information("Tabelle '_Sys' existiert NICHT!");
                        return false;
                    }
                }
            }
        }

        public void UpdateTables() {
            Version version = Assembly.GetExecutingAssembly().GetName().Version!;
            string strVersion = $"Version: {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                string creationString = @"
                    SELECT Version
                    FROM _Sys;
                ";
                using (SqliteCommand cmd = new SqliteCommand(creationString, connection)) {
                    try {
                        connection.Open();
                        string dbVersion = cmd.ExecuteScalar()?.ToString() ?? "";
                        connection.Close();
                        if (dbVersion.Equals(strVersion)) {
                            return;
                        }
                        else { // perform update routines
                            cmd.CommandText = $"UPDATE _Sys SET Version = '{strVersion}'";
                            connection.Open();
                            cmd.ExecuteNonQuery();
                            connection.Close();
                        }
                    }
                    catch (Exception ex) {
                        Log.Error(ex.StackTrace ?? "Error without stacktrace...");
                    }
                }
            }
        }
    }
}
