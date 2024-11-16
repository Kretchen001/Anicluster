using Microsoft.Data.Sqlite;
using Modells.Anime;
using Serilog;

namespace Anicluster.Database.Services {

    public class MediaInfoService {

        private readonly DatabaseManager _databaseManager;

        public MediaInfoService(DatabaseManager databaseManager) {
            _databaseManager = databaseManager;
        }

        public bool InitializeMediaInfoTable() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                string creationString = "" +
                    "CREATE TABLE IF NOT EXISTS MediaInfo (" +
                    "    Id INTEGER PRIMARY KEY," +
                    "    Author TEXT," +
                    "    Producer TEXT," +
                    "    Publisher TEXT," +
                    "    PublishingTimeId INTEGER" +
                    "    FOREIGN KEY (PublishingTimeId) REFERENCES PublishingTime(Id)" +
                    ");";
                using (SqliteCommand cmd = new SqliteCommand(creationString, connection)) {
                    try {
                        connection.Open();
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex) {
                        Log.Error(ex.StackTrace ?? "Error without stacktrace... <- MediaInfo table not created!");
                        return false;
                    }
                }
            }
            Log.Information("MediaInfo table created.");
            return true;
        }

        public bool TableExist() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                connection.Open();
                string query = "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName;";
                using (SqliteCommand cmd = new SqliteCommand(query, connection)) {
                    cmd.Parameters.AddWithValue("@tableName", "MediaInfo");
                    object? result = cmd.ExecuteScalar();

                    return result != null;
                }
            }
        }

        public int InsertMediaInfo(MediaInfo mediaInfoToInsert) {
            int mediaInfoId = 0;
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                using (SqliteTransaction transaction = connection.BeginTransaction()) {
                    try {
                        PublishingTimeService publishingTimeService = new PublishingTimeService(_databaseManager);
                        int pubId = publishingTimeService.Insert(mediaInfoToInsert.PublishingTime);
                        string insertQuery = "" +
                            "INSERT INTO MediaInfo (Author, Producer, Publisher, PublishingTimeId)\n" +
                            "VALUES (@Author, @Producer, @Publisher, @PublishingTimeId)";
                        using (SqliteCommand cmd = new SqliteCommand(insertQuery, connection)) {
                            cmd.Parameters.AddWithValue("@Author", mediaInfoToInsert.Author);
                            cmd.Parameters.AddWithValue("@Producer", mediaInfoToInsert.Producer);
                            cmd.Parameters.AddWithValue("@Publisher", mediaInfoToInsert.Publisher);
                            cmd.Parameters.AddWithValue("@AutPublishingTimeIdhor", mediaInfoId);
                            connection.Open();
                            cmd.ExecuteNonQuery();
                            cmd.Parameters.Clear();
                            cmd.CommandText = "SELECT last_insert_rowid();";
                            object? result = cmd.ExecuteScalar();
                            connection.Close();
                            mediaInfoId = (int)((long)result!);
                        }
                        transaction.Commit();
                    }
                    catch (Exception ex) {
                        transaction.Rollback();
                        Log.Error(ex.StackTrace ?? "Error while inserting mediaInfoToInsert data");
                        return -1;
                    }
                }
            }
            return mediaInfoId;
        }
    }
}
