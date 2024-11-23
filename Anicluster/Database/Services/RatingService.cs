using Microsoft.Data.Sqlite;
using Modells.Anime;
using Serilog;

namespace Anicluster.Database.Services {

    public class RatingService : IDatabaseService {

        private readonly DatabaseManager _databaseManager;

        public RatingService(DatabaseManager databaseManager) {
            _databaseManager = databaseManager;
        }

        public bool InitializeTable() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                string creationString = @"
                    CREATE TABLE IF NOT EXISTS Rating (
                        Id INTEGER PRIMARY KEY,
                        Story INTEGER,
                        Animation INTEGER,
                        SpecialEffects INTEGER,
                        AcousticId INTEGER,
                        IsRated BOOLEAN,
                        FOREIGN KEY(AcousticId) REFERENCES Acoustic(Id)
                    );
                    ";
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
                    connection.Close();
                    if (result is not null) {
                        Log.Information("Tabelle 'Rating' existiert.");
                        return true;
                    }
                    else {
                        Log.Information("Tabelle 'Rating' existiert NICHT!");
                        return false;
                    }
                }
            }
        }

        public int Insert(Rating ratingToInsert) {
            // First Insert the Acoustic and get this Id
            int acousticId = new AcousticService(_databaseManager).Insert(ratingToInsert.Acoustic);
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
                try {
                    using (SqliteCommand cmd = new SqliteCommand(insertQuery, connection)) {
                        cmd.Parameters.AddWithValue("@Story", ratingToInsert.Story);
                        cmd.Parameters.AddWithValue("@Animation", ratingToInsert.Animation);
                        cmd.Parameters.AddWithValue("@SpecialEffects", ratingToInsert.SpecialEffects);
                        cmd.Parameters.AddWithValue("@AcousticId", acousticId);
                        cmd.Parameters.AddWithValue("@IsRated", ratingToInsert.IsRated);
                        cmd.ExecuteScalar();
                    }
                    using (SqliteCommand cmd = new SqliteCommand("SELECT last_insert_rowid();", connection)) {
                        object? result = cmd.ExecuteScalar();
                        ratingId = (int)((long)result!);
                    }
                }
                catch (Exception ex) {
                    Log.Error(ex.StackTrace ?? "Error while inserting ratingToInsert data");
                    return -1;
                }
                connection.Close();
            }
            return ratingId;
        }
    }
}
