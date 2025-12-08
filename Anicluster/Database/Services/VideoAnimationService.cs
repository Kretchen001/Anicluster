using Microsoft.Data.Sqlite;
using Modells.Anime;
using Serilog;
using System.Data;

namespace Anicluster.Database.Services {

    internal class VideoAnimationService : IDatabaseService {

        private readonly DatabaseManager _databaseManager;

        public VideoAnimationService(DatabaseManager databaseManager) {
            this._databaseManager = databaseManager;
        }

        public bool InitializeTable() {
            using (SqliteConnection connection = this._databaseManager.GetConnection()) {
                string creationString = @"
                    CREATE TABLE IF NOT EXISTS VideoAnimation (
                        Id INTEGER PRIMARY KEY,
                        VideoType INTEGER,
                        Comment TEXT
                    );
                ";
                using (SqliteCommand cmd = new SqliteCommand(creationString, connection)) {
                    try {
                        connection.Open();
                        cmd.ExecuteNonQuery();
                        connection.Close();
                    }
                    catch (Exception ex) {
                        Log.Error(ex.StackTrace ?? "Error without stacktrace... <- VideoAnimation table not created!");
                        return false;
                    }
                }
            }
            if (this.TableExist()) {
                Log.Information("VideoAnimation table created.");
                return true;
            }
            else {
                Log.Information("VideoAnimation table NOT created.");
                return false;
            }
        }

        public bool TableExist() {
            using (SqliteConnection connection = this._databaseManager.GetConnection()) {
                connection.Open();
                string query = "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName;";
                using (SqliteCommand cmd = new SqliteCommand(query, connection)) {
                    cmd.Parameters.AddWithValue("@tableName", "VideoAnimation");
                    object? result = cmd.ExecuteScalar();
                    connection.Close();
                    if (result is not null) {
                        Log.Information("Tabelle 'VideoAnimation' existiert.");
                        return true;
                    }
                    else {
                        Log.Information("Tabelle 'VideoAnimation' existiert NICHT!");
                        return false;
                    }
                }
            }
        }

        public int Insert(VideoAnimation videoAnimationToInsert) {
            using (SqliteConnection connection = this._databaseManager.GetConnection()) {
                string queryInsert = @"
                    INSERT INTO VideoAnimation (VideoType, Comment)
                        VALUES (@VideoType, @Comment);
                ";
                using (SqliteCommand cmd = new SqliteCommand(queryInsert, connection)) {
                    try {
                        cmd.Parameters.AddWithValue("@VideoType", videoAnimationToInsert.VideoType);
                        cmd.Parameters.AddWithValue("@Comment", videoAnimationToInsert.Comment ?? DBNull.Value.ToString());
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

        public List<VideoAnimation> GetOvas(List<int> ids) {
            List<VideoAnimation> vas = [];
            DataTable resDt = new DataTable();
            string query = $"SELECT Id, Comment FROM VideoAnimation WHERE Id IN ({String.Join(", ", ids)})";
            using (SqliteConnection connection = this._databaseManager.GetConnection()) {
                using (SqliteCommand cmd = new SqliteCommand(query, connection)) {
                    connection.Open();
                    SqliteDataReader result = cmd.ExecuteReader();
                    resDt.Load(result);
                    connection.Close();
                }
            }
            foreach (DataRow row in resDt.Rows) {
                vas.Add(new VideoAnimation(
                    (int)((long)row["Id"]),
                    VideoType.Ova,
                    row["Comment"].ToString()
                ));
            }
            return vas;
        }

        public List<VideoAnimation> GetVideoAnimation(List<int> ids) {
            List<VideoAnimation> vas = [];
            DataTable resDt = new DataTable();
            string query = $"SELECT Id, Comment, VideoType FROM VideoAnimation WHERE Id IN ({String.Join(", ", ids)})";
            using (SqliteConnection connection = this._databaseManager.GetConnection()) {
                using (SqliteCommand cmd = new SqliteCommand(query, connection)) {
                    connection.Open();
                    SqliteDataReader result = cmd.ExecuteReader();
                    resDt.Load(result);
                    connection.Close();
                }
            }
            foreach (DataRow row in resDt.Rows) {
                vas.Add(new VideoAnimation(
                    id:(int)((long)row["Id"]),
                    videoType:(VideoType)Enum.Parse(typeof(VideoType), resDt.Rows[0]["VideoType"].ToString() ?? VideoType.Ova.ToString()),
                    comment:row["Comment"].ToString()
                ));
            }
            return vas;
        }
    }
}
