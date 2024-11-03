using Microsoft.Data.Sqlite;
using Serilog;

namespace Anicluster.Database.Services {

    public class MusicPieceService {

        private readonly DatabaseManager _databaseManager;

        public MusicPieceService(DatabaseManager databaseManager) {
            _databaseManager = databaseManager;
        }

        public bool InitializeMusicPieceTable() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                string creationString = "" +
                    "CREATE TABLE IF NOT EXISTS MusicPiece (" +
                    "    Id INTEGER PRIMARY KEY," +
                    "    Type INTEGER," +
                    "    Name TEXT," +
                    "    Comment TEXT," +
                    "    General INTEGER" +
                    ");";
                using (SqliteCommand cmd = new SqliteCommand(creationString, connection)) {
                    try {
                        connection.Open();
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex) {
                        Log.Error(ex.StackTrace ?? "Error without stacktrace... <- MusicPiece table not created!");
                        return false;
                    }
                }
            }
            Log.Information("MusicPiece table created.");
            return true;
        }

        public bool TableExist() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                connection.Open();
                string query = "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName;";
                using (SqliteCommand cmd = new SqliteCommand(query, connection)) {
                    cmd.Parameters.AddWithValue("@tableName", "MusicPiece");
                    object? result = cmd.ExecuteScalar();

                    return result != null;
                }
            }
        }
    }
}
