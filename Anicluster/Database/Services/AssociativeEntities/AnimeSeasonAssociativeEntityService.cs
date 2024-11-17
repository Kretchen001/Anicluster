using Microsoft.Data.Sqlite;
using Serilog;

namespace Anicluster.Database.Services.AssociativeEntities {
    public class AnimeSeasonAssociativeEntityService : IDatabaseService {

        private readonly DatabaseManager _databaseManager;

        public AnimeSeasonAssociativeEntityService(DatabaseManager databaseManager) {
            _databaseManager = databaseManager;
        }

        public bool InitializeTable() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                string creationString = "" +
                    "CREATE TABLE IF NOT EXISTS AnimeSeasonAssociativeEntity (" +
                    "    AnimeId INTEGER," +
                    "    SeasonId INTEGER," +
                    "    PRIMARY KEY(AnimeId, SeasonId),  --Kombinierter Primärschlüssel" +
                    "    FOREIGN KEY(AnimeId) REFERENCES Anime(Id)," +
                    "    FOREIGN KEY(SeasonId) REFERENCES Season(Id)" +
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

                    return result != null;
                }
            }
        }

        public void AddSeasonToAnime(int animeId, int seasonId) {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                connection.Open();
                string query = "INSERT INTO AnimeSeasonAssociativeEntity (AnimeId, SeasonId) VALUES (@AnimeId, @SeasonId);";

                using (SqliteCommand cmd = new SqliteCommand(query, connection)) {
                    cmd.Parameters.AddWithValue("@AnimeId", animeId);
                    cmd.Parameters.AddWithValue("@SeasonId", seasonId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
