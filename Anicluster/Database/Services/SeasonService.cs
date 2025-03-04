using Anicluster.Database.Services.AssociativeEntities;
using Microsoft.Data.Sqlite;
using Modells.Anime;
using Serilog;
using System.Data;
using System.Xml.Linq;

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
                        cmd.Parameters.AddWithValue("@Comment", seasonToInsert.Comment ?? DBNull.Value.ToString());
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
                SeasonVideoAnimationAssociativeEntityService svaaes = new SeasonVideoAnimationAssociativeEntityService(_databaseManager);
                foreach (int x in videoAnimationIds) {
                    svaaes.Insert(insertedSeasonId, x);
                }
                return insertedSeasonId;
            }
        }

        public List<Season> GetSeasonsByIds(List<int> ids) {
            List<Season> seasons = [];
            DataTable resDt = new DataTable();
            string query = $"SELECT Id, Number, Comment, PublishingTimeId FROM Season WHERE Id IN ({String.Join(", ", ids)})";
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                using (SqliteCommand cmd = new SqliteCommand(query, connection)) {
                    connection.Open();
                    SqliteDataReader result = cmd.ExecuteReader();
                    resDt.Load(result);
                    connection.Close();
                }
            }
            PublishingTimeService pts = new PublishingTimeService(_databaseManager);
            SeasonVideoAnimationAssociativeEntityService svaaes = new SeasonVideoAnimationAssociativeEntityService(_databaseManager);
            VideoAnimationService vas = new VideoAnimationService(_databaseManager);
            foreach (DataRow row in resDt.Rows) {
                Season tempSeason = new Season(
                    (int)((long)row["Id"]),
                    (int)((long)row["Number"]),
                    row["Comment"].ToString()
                );
                tempSeason.PublishingTime = pts.SelectById((int)((long)row["PublishingTimeId"]));
                List<int> VideoAnimationIds = svaaes.GetVideoAnimationById(tempSeason.Id);
                tempSeason.Episodes.AddRange(vas.GetVideoAnimation(VideoAnimationIds));
                seasons.Add(tempSeason);
            }
            return seasons;
        }
    }
}
