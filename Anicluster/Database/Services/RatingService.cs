using Microsoft.Data.Sqlite;
using Modells.Anime;
using Serilog;

namespace Anicluster.Database.Services {

    public class RatingService {

        private readonly DatabaseManager _databaseManager;

        public RatingService(DatabaseManager databaseManager) {
            _databaseManager = databaseManager;
        }

        public bool InitializeRatingTable() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                string creationString = "" +
                    "CREATE TABLE IF NOT EXISTS Rating (" +
                    "    Id INTEGER PRIMARY KEY," +
                    "    Story INTEGER," +
                    "    Animation INTEGER," +
                    "    SpecialEffects INTEGER," +
                    "    AcousticId INTEGER," +
                    "    IsRated BOOLEAN," +
                    "    FOREIGN KEY(AcousticId) REFERENCES Acoustic(Id)" +
                    ");";
                using (SqliteCommand cmd = new SqliteCommand(creationString, connection)) {
                    try {
                        connection.Open();
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex) {
                        Log.Error(ex.StackTrace ?? "Error without stacktrace... <- Rating table not created!");
                        return false;
                    }
                }
            }
            Log.Information("Rating table created.");
            return true;
        }

        public bool TableExist() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                connection.Open();
                string query = "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName;";
                using (SqliteCommand cmd = new SqliteCommand(query, connection)) {
                    cmd.Parameters.AddWithValue("@tableName", "Rating");
                    object? result = cmd.ExecuteScalar();

                    return result != null;
                }
            }
        }

        public int InsertRating(Rating ratingToInsert) {
            // First Insert the Acoustic and get this Id
            int acousticId = new AcousticService(_databaseManager).InsertAcoustic(ratingToInsert.Acoustic);
            if (acousticId.Equals(-1)) {
                Log.Error("Break by inserting Rating...");
                return -1;
            }

            // Insert now the Rating
            int ratingId = 0;
            string insertQuery = "" +
                "INSERT INTO Rating (Story, Animation, SpecialEffects, AcousticId, IsRated)\n" +
                "    VALUES (@Story, @Animation, @SpecialEffects, @AcousticId, @IsRated)";
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                connection.Open();
                using (SqliteTransaction transaction = connection.BeginTransaction()) {
                    try {
                        using (SqliteCommand cmd = new SqliteCommand(insertQuery, connection)) {
                            cmd.Parameters.AddWithValue("@Story", ratingToInsert.Story);
                            cmd.Parameters.AddWithValue("@Animation", ratingToInsert.Animation);
                            cmd.Parameters.AddWithValue("@SpecialEffects", ratingToInsert.SpecialEffects);
                            cmd.Parameters.AddWithValue("@AcousticId", acousticId);
                            cmd.Parameters.AddWithValue("@IsRated", ratingToInsert.IsRated);
                        }
                        using (SqliteCommand cmd = new SqliteCommand("SELECT last_insert_rowid();", connection)) {
                            object? result = cmd.ExecuteScalar();
                            ratingId = (int)((long)result!);
                        }                        
                        transaction.Commit();
                    }
                    catch (Exception ex) {
                        transaction.Rollback();
                        Log.Error(ex.StackTrace ?? "Error while inserting ratingToInsert data");
                        return -1;
                    }
                }
                connection.Close();
            }
            return ratingId;
        }
    }
}
