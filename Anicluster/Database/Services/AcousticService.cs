using Microsoft.Data.Sqlite;
using Modells.Anime.SoundRating;
using Serilog;

namespace Anicluster.Database.Services {

    public class AcousticService : IDatabaseService {

        private readonly DatabaseManager _databaseManager;

        public AcousticService(DatabaseManager databaseManager) {
            _databaseManager = databaseManager;
        }

        public bool InitializeTable() {
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
                        connection.Close();
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

                    connection.Close();
                    return result != null;
                }
            }
        }

        public int InsertAcoustic(Acoustic acousticToInsert) {
            int acousticId = 0;
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                connection.Open();
                using (SqliteTransaction transaction = connection.BeginTransaction()) {
                    try {
                        string insertAcoustic = "" +
                            "INSERT INTO Acoustic (Soundtrack, Comment)\n" +
                            "    VALUES (@soundtrack, @comment)";
                        using (SqliteCommand cmd = new SqliteCommand(insertAcoustic, connection)) {
                            cmd.ExecuteScalar();
                        }
                        using (SqliteCommand cmd = new SqliteCommand("SELECT last_insert_rowid();", connection)) {
                            object? result = cmd.ExecuteScalar();
                            _ = int.TryParse(result as string, out acousticId);
                        }
                        transaction.Commit();
                    }
                    catch (Exception ex) {
                        transaction.Rollback();
                        Log.Error(ex.StackTrace ?? "Error while inserting acousticToInsert data");
                        return -1;
                    }
                }
                connection.Close();
            }
            return acousticId;
        }
    }
}
