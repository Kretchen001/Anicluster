using Microsoft.Data.Sqlite;
using Modells.Anime.SoundRating;
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
                    "    Comment TEXT" +
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
                    connection.Close();
                    if (result is not null) {
                        Log.Information("Tabelle 'Syncro' existiert.");
                        return true;
                    }
                    else {
                        Log.Information("Tabelle 'Syncro' existiert NICHT!");
                        return false;
                    }
                }
            }
        }

        public int Insert(Syncro syncroToInsert) {
            int syncroId = -1;
            string insertQuery = "" +
                "INSERT INTO Syncro (General, Language, IsAssessed, Comment)\n" +
                "    VALUES (@General, @Language, @IsAssessed, @Comment)";
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                connection.Open();
                try {
                    using (SqliteCommand cmd = new SqliteCommand(insertQuery, connection)) {
                        cmd.Parameters.AddWithValue("@General", syncroToInsert.General);
                        cmd.Parameters.AddWithValue("@Language", syncroToInsert.Language);
                        cmd.Parameters.AddWithValue("@IsAssessed", syncroToInsert.IsAssessed);
                        cmd.Parameters.AddWithValue("@Comment", syncroToInsert.Comment);
                    }
                    using (SqliteCommand cmd = new SqliteCommand("SELECT last_insert_rowid();", connection)) {
                        object? result = cmd.ExecuteScalar();
                        syncroId = (int)((long)result!);
                    }
                }
                catch (Exception ex) {
                    Log.Error(ex.StackTrace ?? "Error while inserting syncroToInsert data");
                    return -1;
                }
                connection.Close();
            }
            return syncroId;
        }
    }
}
