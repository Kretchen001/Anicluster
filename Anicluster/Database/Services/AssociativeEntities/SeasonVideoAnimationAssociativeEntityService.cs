using Microsoft.Data.Sqlite;
using Serilog;
using System.Data;

namespace Anicluster.Database.Services.AssociativeEntities {

    public class SeasonVideoAnimationAssociativeEntityService : IDatabaseService {

        private readonly DatabaseManager _databaseManager;

        public SeasonVideoAnimationAssociativeEntityService(DatabaseManager databaseManager) {
            this._databaseManager = databaseManager;
        }

        public bool InitializeTable() {
            using (SqliteConnection connection = this._databaseManager.GetConnection()) {
                string creationString = @"
                    CREATE TABLE IF NOT EXISTS SeasonVideoAnimationAssociativeEntityService (
                        SeasonId INTEGER,
                        VideoAnimationId INTEGER,
                        FOREIGN KEY(SeasonId) REFERENCES Season(Id),
                        FOREIGN KEY(VideoAnimationId) REFERENCES VideoAnimation(Id)
                    );";
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
            using (SqliteConnection connection = this._databaseManager.GetConnection()) {
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
            using (SqliteConnection connection = this._databaseManager.GetConnection()) {
                connection.Open();
                string query = "INSERT INTO SeasonVideoAnimationAssociativeEntityService (SeasonId, VideoAnimationId)\n" +
                    "    VALUES (@SeasonId, @VideoAnimationId);";
                using (SqliteCommand cmd = new SqliteCommand(query, connection)) {
                    cmd.Parameters.AddWithValue("@SeasonId", seasonId);
                    cmd.Parameters.AddWithValue("@VideoAnimationId", videoAnimationId);
                    cmd.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        public List<int> GetVideoAnimationById(int id = -1) {
            string query = $"" +
                $"SELECT VideoAnimationId\n" +
                $"FROM SeasonVideoAnimationAssociativeEntityService\n" +
                $"WHERE\n" +
                $"    SeasonId={id}";

            DataTable resDt = new DataTable();
            using (SqliteConnection connection = this._databaseManager.GetConnection()) {
                using (SqliteCommand cmd = new SqliteCommand(query, connection)) {
                    connection.Open();
                    SqliteDataReader result = cmd.ExecuteReader();
                    resDt.Load(result);
                    connection.Close();
                }
            }
            return resDt.AsEnumerable().Select(row => Convert.ToInt32(row.Field<long>("VideoAnimationId"))).ToList();
        }
    }
}
