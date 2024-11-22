using Microsoft.Data.Sqlite;
using Serilog;

namespace Anicluster.Database.Services.AssociativeEntities {

    public class SeasonVideoAnimationAssociativeEntityService : IDatabaseService {

        private readonly DatabaseManager _databaseManager;

        public SeasonVideoAnimationAssociativeEntityService(DatabaseManager databaseManager) {
            _databaseManager = databaseManager;
        }

        public bool InitializeTable() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                string creationString = "" +
                    "CREATE TABLE IF NOT EXISTS SeasonVideoAnimationAssociativeEntityService (\n" +
                    "    SeasonId INTEGER,\n" +
                    "    VideoAnimationId INTEGER,\n" +
                    "    PRIMARY KEY(SeasonId, VideoAnimationId),\n" +
                    "    FOREIGN KEY(SeasonId) REFERENCES Season(Id),\n" +
                    "    FOREIGN KEY(VideoAnimationId) REFERENCES VideoAnimation(Id)\n" +
                    ");";
                using (SqliteCommand cmd = new SqliteCommand(creationString, connection)) {
                    try {
                        connection.Open();
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex) {
                        Log.Error(ex.StackTrace ?? "Error without stacktrace... <- SeasonVideoAnimationAssociativeEntityService table not created!");
                        return false;
                    }
                }
            }
            Log.Information("SeasonVideoAnimationAssociativeEntityService table created.");
            return true;
        }

        public bool TableExist() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                connection.Open();
                string query = "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName;";
                using (SqliteCommand cmd = new SqliteCommand(query, connection)) {
                    cmd.Parameters.AddWithValue("@tableName", "SeasonVideoAnimationAssociativeEntityService");
                    object? result = cmd.ExecuteScalar();
                    connection.Close();
                    if (result is not null) {
                        Log.Information("Tabelle 'SeasonVideoAnimationAssociativeEntityService' existiert.");
                        return true;
                    }
                    else {
                        Log.Information("Tabelle 'SeasonVideoAnimationAssociativeEntityService' existiert NICHT!");
                        return false;
                    }
                }
            }
        }

        public void Insert(int seasonId, int videoAnimationId) {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                connection.Open();
                string query = "INSERT INTO SeasonVideoAnimationAssociativeEntityService (SeasonId, VideoAnimationId) VALUES (@SeasonId, @VideoAnimationId);";

                using (SqliteCommand cmd = new SqliteCommand(query, connection)) {
                    cmd.Parameters.AddWithValue("@SeasonId", seasonId);
                    cmd.Parameters.AddWithValue("@VideoAnimation", videoAnimationId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
