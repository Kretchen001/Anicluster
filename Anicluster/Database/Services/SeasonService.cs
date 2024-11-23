using Anicluster.Database.Services.AssociativeEntities;
using Microsoft.Data.Sqlite;
using Modells.Anime;
using Serilog;

namespace Anicluster.Database.Services {

    public class SeasonService : IDatabaseService/*<Season>*/ {

        private readonly DatabaseManager _databaseManager;

        public SeasonService(DatabaseManager databaseManager) {
            _databaseManager = databaseManager;
        }

        public bool InitializeTable() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                string creationString = @"
                    CREATE TABLE IF NOT EXISTS Season (
                        Id INTEGER PRIMARY KEY,
                        Number INTEGER,
                        Comment TEXT,
                        PublishingTimeId INTEGER,
                        FOREIGN KEY(PublishingTimeId) REFERENCES PublishingTime(Id)
                    );
                ";
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
                    connection.Close();
                    if (result is not null) {
                        Log.Information("Tabelle 'Season' existiert.");
                        return true;
                    }
                    else {
                        Log.Information("Tabelle 'Season' existiert NICHT!");
                        return false;
                    }
                }
            }
        }

        public int Insert(Season seasonToInsert) {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                // check if season exist, then return only its id
                string checkQuery = "" +
                    "SELECT *\n" +
                    "FROM Season\n" +
                    "WHERE\n" +
                    $"    Number={seasonToInsert.Number}\n" +
                    $"    AND Comment LIKE '{seasonToInsert.Comment}';";
                connection.Open();
                using (SqliteCommand cmd = new SqliteCommand(checkQuery, connection)) {
                    SqliteDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows) {
                        reader.Read();
                        return (int)((long)reader["Id"]);
                    }
                }
                connection.Close();
                // not there, so insert the PublishingTime
                PublishingTimeService publishingTimeService = new PublishingTimeService(_databaseManager);
                int pubId = publishingTimeService.Insert(seasonToInsert.PublishingTime);
                // else insert and return inserted id
                int insertedSeasonId = -1;
                string querySeasonInsert = "" +
                    "INSERT INTO Season (Number, Comment, PublishingTimeId) \n" +
                    "    VALUES (@Number, @Comment, @PublishingTimeId);";
                using (SqliteCommand cmd = new SqliteCommand(querySeasonInsert, connection)) {
                    try {
                        cmd.Parameters.AddWithValue("@Number", seasonToInsert.Number);
                        cmd.Parameters.AddWithValue("@Comment", seasonToInsert.Comment);
                        cmd.Parameters.AddWithValue("@PublishingTimeId", pubId);
                        connection.Open();
                        cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                        cmd.CommandText = "SELECT last_insert_rowid();";
                        insertedSeasonId = (int)((long)cmd.ExecuteScalar()!);
                        connection.Close();
                    }
                    catch (Exception ex) {
                        Log.Error(ex.StackTrace ?? "Error without stacktrace... <- Season not inserted or queryable!");
                        return -1;
                    }
                }
                // insert the episodes
                List<int> videoAnimationIds = new List<int>(seasonToInsert.Episodes.Count);
                VideoAnimationService vas = new VideoAnimationService(_databaseManager);
                foreach (VideoAnimation x in seasonToInsert.Episodes) {
                    videoAnimationIds.Add(vas.Insert(x));
                }
                // map the episodes as VideoAnimation
                foreach (int x in videoAnimationIds) {
                    SeasonVideoAnimationAssociativeEntityService svaaes = new SeasonVideoAnimationAssociativeEntityService(_databaseManager);
                    svaaes.Insert(insertedSeasonId, x);
                }
                return insertedSeasonId;
            }
        }
    }
}
