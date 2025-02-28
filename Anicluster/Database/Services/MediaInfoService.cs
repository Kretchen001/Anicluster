using Microsoft.Data.Sqlite;
using Modells.Anime;
using Serilog;
using System.Data;

namespace Anicluster.Database.Services {

    public class MediaInfoService : IDatabaseService {

        private readonly DatabaseManager _databaseManager;

        public MediaInfoService(DatabaseManager databaseManager) {
            _databaseManager = databaseManager;
        }

        public bool InitializeTable() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                string creationString = @"
                    CREATE TABLE IF NOT EXISTS MediaInfo (
                        Id INTEGER PRIMARY KEY,
                        Author TEXT,
                        Producer TEXT,
                        Publisher TEXT,
                        PublishingTimeId INTEGER,
                        FOREIGN KEY (PublishingTimeId) REFERENCES PublishingTime(Id)
                    );
                ";
                using (SqliteCommand cmd = new SqliteCommand(creationString, connection)) {
                    try {
                        connection.Open();
                        cmd.ExecuteNonQuery();
                        connection.Close();
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
                    connection.Close();
                    if (result is not null) {
                        Log.Information("Tabelle 'MediaInfo' existiert.");
                        return true;
                    }
                    else {
                        Log.Information("Tabelle 'MediaInfo' existiert NICHT!");
                        return false;
                    }
                }
            }
        }

        public int Insert(MediaInfo mediaInfoToInsert) {
            int mediaInfoId = 0;
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                connection.Open();
                try {
                    PublishingTimeService publishingTimeService = new PublishingTimeService(_databaseManager);
                    int pubId = publishingTimeService.Insert(mediaInfoToInsert.PublishingTime);
                    string insertQuery = "" +
                        "INSERT INTO MediaInfo (Author, Producer, Publisher, PublishingTimeId)\n" +
                        "    VALUES (@Author, @Producer, @Publisher, @PublishingTimeId)";
                    using (SqliteCommand cmd = new SqliteCommand(insertQuery, connection)) {
                        cmd.Parameters.AddWithValue("@Author", mediaInfoToInsert.Author ?? DBNull.Value.ToString());
                        cmd.Parameters.AddWithValue("@Producer", mediaInfoToInsert.Producer ?? DBNull.Value.ToString());
                        cmd.Parameters.AddWithValue("@Publisher", mediaInfoToInsert.Publisher ?? DBNull.Value.ToString());
                        cmd.Parameters.AddWithValue("@PublishingTimeId", pubId);
                        cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                        cmd.CommandText = "SELECT last_insert_rowid();";
                        object? result = cmd.ExecuteScalar();
                        connection.Close();
                        mediaInfoId = (int)((long)result!);
                    }
                }
                catch (Exception ex) {
                    Log.Error(ex.StackTrace ?? "Error while inserting mediaInfoToInsert data");
                    return -1;
                }
            }
            return mediaInfoId;
        }

        public MediaInfo GetMediaInfoById(int id = -1) {
            string query = $"" +
                $"SELECT Id, Author, Producer, Publisher, PublishingTimeId\n" +
                $"FROM MediaInfo\n" +
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
            PublishingTime tempPubTime = new PublishingTimeService(_databaseManager).SelectById(
                (int)((long)resDt.Rows[0]["PublishingTimeId"])
            );
            return new MediaInfo(
                id: (int)((long)resDt.Rows[0]["Id"]),
                author: resDt.Rows[0]["Author"].ToString(),
                producer: resDt.Rows[0]["Producer"].ToString(),
                publisher: resDt.Rows[0]["Publisher"].ToString(),
                pubTime: tempPubTime
            );
        }
    }
}
