using Microsoft.Data.Sqlite;
using Modells.Anime;
using Serilog;
using System.Data;

namespace Anicluster.Database.Services.AssociativeEntities {
    internal class AnimeOvaAssociativeEntityService : IDatabaseService {

        private readonly DatabaseManager _databaseManager;

        public AnimeOvaAssociativeEntityService(DatabaseManager databaseManager) {
            _databaseManager = databaseManager;
        }

        public bool InitializeTable() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                string creationString = "" +
                    "CREATE TABLE IF NOT EXISTS AnimeOvaAssociativeEntityService (\n" +
                    "    AnimeId INTEGER,\n" +
                    "    VideoAnimationId INTEGER,\n" +
                    "    PRIMARY KEY(AnimeId, VideoAnimationId),\n" +
                    "    FOREIGN KEY(AnimeId) REFERENCES Anime(Id),\n" +
                    "    FOREIGN KEY(VideoAnimationId) REFERENCES VideoAnimation(Id)\n" +
                    ");";
                using (SqliteCommand cmd = new SqliteCommand(creationString, connection)) {
                    try {
                        connection.Open();
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex) {
                        Log.Error(ex.StackTrace ?? "Error without stacktrace... <- AnimeOvaAssociativeEntityService table not created!");
                        return false;
                    }
                }
            }
            Log.Information("AnimeOvaAssociativeEntityService table created.");
            return true;
        }

        public bool TableExist() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                connection.Open();
                string query = "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName;";
                using (SqliteCommand cmd = new SqliteCommand(query, connection)) {
                    cmd.Parameters.AddWithValue("@tableName", "AnimeOvaAssociativeEntityService");
                    object? result = cmd.ExecuteScalar();
                    connection.Close();
                    if (result is not null) {
                        Log.Information("Tabelle 'AnimeOvaAssociativeEntity' existiert.");
                        return true;
                    }
                    else {
                        Log.Information("Tabelle 'AnimeOvaAssociativeEntity' existiert NICHT!");
                        return false;
                    }
                }
            }
        }

        public void Insert(int animeId, int vaId) {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                string checkIfExist = $"" +
                    $"SELECT *\n" +
                    $"FROM AnimeOvaAssociativeEntityService\n" +
                    $"WHERE\n" +
                    $"    AnimeId={animeId}\n" +
                    $"    AND VideoAnimationId={vaId}";
                connection.Open();
                using (SqliteCommand cmd = new SqliteCommand(checkIfExist, connection)) {
                    SqliteDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows) {
                        return;
                    }
                }
                string query = "INSERT INTO AnimeOvaAssociativeEntityService (AnimeId, VideoAnimationId)\n" +
                    "    VALUES (@AnimeId, @VideoAnimationId);";
                using (SqliteCommand cmd = new SqliteCommand(query, connection)) {
                    cmd.Parameters.AddWithValue("@AnimeId", animeId);
                    cmd.Parameters.AddWithValue("@VideoAnimationId", vaId);
                    cmd.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        public List<int> GetOvaIdsByAnimeId(int id = -1) {
            string query = $"" +
                $"SELECT VideoAnimationId\n" +
                $"FROM AnimeOvaAssociativeEntityService\n" +
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
            return resDt.AsEnumerable().Select(row => row.Field<int>("VideoAnimationId")).ToList();
        }
    }
}
