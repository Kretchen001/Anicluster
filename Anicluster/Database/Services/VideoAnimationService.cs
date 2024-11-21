using Microsoft.Data.Sqlite;
using Modells.Anime;
using Serilog;

namespace Anicluster.Database.Services {

    internal class VideoAnimationService : IDatabaseService {

        private readonly DatabaseManager _databaseManager;

        public VideoAnimationService(DatabaseManager databaseManager) {
            _databaseManager = databaseManager;
        }

        public bool InitializeTable() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                string creationString = "" +
                    "CREATE TABLE IF NOT EXISTS VideoAnimation (\n" +
                    "    Id INTEGER PRIMARY KEY,\n" +
                    "    VideoType INTEGER,\n" +
                    "    Comment TEXT\n" +
                    ");";
                using (SqliteCommand cmd = new SqliteCommand(creationString, connection)) {
                    try {
                        connection.Open();
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex) {
                        Log.Error(ex.StackTrace ?? "Error without stacktrace... <- VideoAnimation table not created!");
                        return false;
                    }
                }
            }
            Log.Information("VideoAnimation table created.");
            return true;
        }

        public bool TableExist() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                connection.Open();
                string query = "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName;";
                using (SqliteCommand cmd = new SqliteCommand(query, connection)) {
                    cmd.Parameters.AddWithValue("@tableName", "VideoAnimation");
                    object? result = cmd.ExecuteScalar();

                    return result != null;
                }
            }
        }

        public int Insert(VideoAnimation videoAnimationToInsert) {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                // check if season exist, then return only its id
                string checkQuery = "" +
                    "SELECT *\n" +
                    "FROM VideoAnimation\n" +
                    $"WHERE VideoType LIKE {videoAnimationToInsert.VideoType}" +
                    $"    Comment LIKE '{videoAnimationToInsert.Comment}'";
                using (SqliteCommand cmd = new SqliteCommand(checkQuery, connection)) {
                    SqliteDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows) {
                        return (int)((long)reader["Id"]);
                    }
                }
                string queryInsert = "" +
                    "INSERT INTO VideoAnimation (VideoType, Comment) \n" +
                    "VALUES (@VideoType, @Comment);";
                using (SqliteCommand cmd = new SqliteCommand(queryInsert, connection)) {
                    try {
                        cmd.Parameters.AddWithValue("@VideoType", videoAnimationToInsert.VideoType);
                        cmd.Parameters.AddWithValue("@Comment", videoAnimationToInsert.Comment);
                        connection.Open();
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "SELECT last_insert_rowid();";
                        object? insertedId = cmd.ExecuteScalar();
                        connection.Close();
                        return (int)((long)insertedId!);
                    }
                    catch (Exception ex) {
                        Log.Error(ex.StackTrace ?? "Error without stacktrace... <- Season not inserted or queryable!");
                        return -1;
                    }
                }
            }
        }
    }
}
