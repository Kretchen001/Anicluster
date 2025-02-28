using Microsoft.Data.Sqlite;
using Serilog;
using System.Data;

namespace Anicluster.Database.Services.AssociativeEntities {
    public class AnimeSeasonAssociativeEntityService : IDatabaseService {

        private readonly DatabaseManager _databaseManager;

        public AnimeSeasonAssociativeEntityService(DatabaseManager databaseManager) {
            _databaseManager = databaseManager;
        }

        public bool InitializeTable() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                string creationString = "" +
                    "CREATE TABLE IF NOT EXISTS AnimeSeasonAssociativeEntity (\n" +
                    "    AnimeId INTEGER,\n" +
                    "    SeasonId INTEGER,\n" +
                    "    PRIMARY KEY(AnimeId, SeasonId),\n" +
                    "    FOREIGN KEY(AnimeId) REFERENCES Anime(Id),\n" +
                    "    FOREIGN KEY(SeasonId) REFERENCES Season(Id)\n" +
                    ");";
                using (SqliteCommand cmd = new SqliteCommand(creationString, connection)) {
                    try {
                        connection.Open();
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex) {
                        Log.Error(ex.StackTrace ?? "Error without stacktrace... <- AnimeSeasonAssociativeEntity table not created!");
                        return false;
                    }
                }
            }
            Log.Information("AnimeSeasonAssociativeEntity table created.");
            return true;
        }

        public bool TableExist() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                connection.Open();
                string query = "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName;";
                using (SqliteCommand cmd = new SqliteCommand(query, connection)) {
                    cmd.Parameters.AddWithValue("@tableName", "AnimeSeasonAssociativeEntity");
                    object? result = cmd.ExecuteScalar();
                    connection.Close();
                    if (result is not null) {
                        Log.Information("Tabelle 'AnimeSeasonAssociativeEntity' existiert.");
                        return true;
                    }
                    else {
                        Log.Information("Tabelle 'AnimeSeasonAssociativeEntity' existiert NICHT!");
                        return false;
                    }
                }
            }
        }

        public void Insert(int animeId, int seasonId) {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                string query = "INSERT INTO AnimeSeasonAssociativeEntity (AnimeId, SeasonId)\n" +
                "    VALUES (@AnimeId, @SeasonId);";
                connection.Open();
                using (SqliteCommand cmd = new SqliteCommand(query, connection)) {
                    cmd.Parameters.AddWithValue("@AnimeId", animeId);
                    cmd.Parameters.AddWithValue("@SeasonId", seasonId);
                    cmd.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        public List<int> GetSeasonIdsByAnimeId(int id = -1) {
            string query = $"" +
                $"SELECT SeasonId\n" +
                $"FROM AnimeSeasonAssociativeEntity\n" +
                $"WHERE\n" +
                $"    AnimeId={id}";

            DataTable resDt = new DataTable();
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                using (SqliteCommand cmd = new SqliteCommand(query, connection)) {
                    connection.Open();
                    SqliteDataReader result = cmd.ExecuteReader();
                    resDt.Load(result);
                    connection.Close();
                }
            }
            return resDt.AsEnumerable().Select(row => Convert.ToInt32(row.Field<long>("SeasonId"))).ToList();
        }
    }
}
