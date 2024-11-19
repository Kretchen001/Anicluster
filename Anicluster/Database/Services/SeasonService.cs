using Microsoft.Data.Sqlite;
using Modells.Anime;
using Serilog;

namespace Anicluster.Database.Services {

    public class SeasonService : IDatabaseService<Season> {

        private readonly DatabaseManager _databaseManager;

        public SeasonService(DatabaseManager databaseManager) {
            _databaseManager = databaseManager;
        }

        public bool InitializeTable() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                string creationString = "" +
                    "CREATE TABLE IF NOT EXISTS Season (" +
                    "    Id INTEGER PRIMARY KEY," +
                    "    AnimeId INTEGER," +
                    "    Number INTEGER," +
                    "    Comment TEXT," +
                    "    PublishingTimeId INTEGER," +
                    "    FOREIGN KEY(AnimeId) REFERENCES Anime(Id)," +
                    "    FOREIGN KEY(PublishingTimeId) REFERENCES PublishingTime(Id)" +
                    ");";
                using (SqliteCommand cmd = new SqliteCommand(creationString, connection)) {
                    try {
                        connection.Open();
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex) {
                        Log.Error(ex.StackTrace ?? "Error without stacktrace... <- Season table not created!");
                        return false;
                    }
                }
            }
            Log.Information("Season table created.");
            return true;
        }

        public bool TableExist() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                connection.Open();
                string query = "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName;";
                using (SqliteCommand cmd = new SqliteCommand(query, connection)) {
                    cmd.Parameters.AddWithValue("@tableName", "Season");
                    object? result = cmd.ExecuteScalar();

                    return result != null;
                }
            }
        }

        public int Insert(Season seasonToInsert, int animeId) {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                // check if season exist, then return only its id
                string checkQuery = "" +
                    "SELECT *\n" +
                    "FROM Season\n" +
                    $"WHERE AnimeId LIKE {animeId}\n" +
                    $"Number LIKE {seasonToInsert.Number}\n" +
                    $"Comment {seasonToInsert.Comment}";
                using (SqliteCommand cmd = new SqliteCommand(checkQuery, connection)) {
                    SqliteDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows) {
                        return (int)((long)reader["Id"]);
                    }
                }
                // not there, so insert the PublishingTime
                PublishingTimeService publishingTimeService = new PublishingTimeService(_databaseManager);
                int pubId = publishingTimeService.Insert(seasonToInsert.PublishingTime);
                // else insert and return inserted id
                int insertedSeasonId = -1;
                string querySeasonInsert = "" +
                    "INSERT INTO PublishingTime (AnimeId, Number, Comment, PublishingTimeId) \n" +
                    "VALUES (@AnimeId, @Number, @Comment, @PublishingTimeId);";
                using (SqliteCommand cmd = new SqliteCommand(querySeasonInsert, connection)) {
                    try {
                        cmd.Parameters.AddWithValue("@AnimeId", animeId);
                        cmd.Parameters.AddWithValue("@Number", seasonToInsert.Number);
                        connection.Open();
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "SELECT last_insert_rowid();";
                        object? insertedId = cmd.ExecuteScalar();
                        connection.Close();
                        insertedSeasonId = (int)((long)insertedId!);
                    }
                    catch (Exception ex) {
                        Log.Error(ex.StackTrace ?? "Error without stacktrace... <- Season not inserted or queryable!");
                        return -1;
                    }
                }
                // insert the episodes

                // map the episodes

                return -1;
            }
        }
    }
}
