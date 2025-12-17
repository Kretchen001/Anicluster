using Microsoft.Data.Sqlite;
using Modells.Anime;
using Modells.Anime.SoundRating;
using Serilog;
using System.Data;

namespace Anicluster.Database.Services {

    public class RatingService : IDatabaseService {

        private readonly DatabaseManager _databaseManager;

        public RatingService(DatabaseManager databaseManager) {
            this._databaseManager = databaseManager;
        }

        public bool InitializeTable() {
            using (SqliteConnection connection = this._databaseManager.GetConnection()) {
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
            using (SqliteConnection connection = this._databaseManager.GetConnection()) {
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
            int acousticId = new AcousticService(this._databaseManager).Insert(ratingToInsert.Acoustic);
            if (acousticId.Equals(-1)) {
                Log.Error("Break by inserting Rating...");
                return -1;
            }

            // Insert now the Rating
            int ratingId = 0;
            string insertQuery = "" +
                "INSERT INTO Rating (Story, Animation, SpecialEffects, AcousticId, IsRated)\n" +
                "    VALUES (@Story, @Animation, @SpecialEffects, @AcousticId, @IsRated)";
            using (SqliteConnection connection = this._databaseManager.GetConnection()) {
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

        public Rating GetRatingById(int id = -1) {
            string query = $"" +
                $"SELECT *\n" +
                $"FROM Rating\n" +
                $"WHERE Id = '{id}'";
            DataTable resDt = new DataTable();
            using (SqliteConnection connection = this._databaseManager.GetConnection()) {
                using (SqliteCommand cmd = new SqliteCommand(query, connection)) {
                    connection.Open();
                    SqliteDataReader result = cmd.ExecuteReader();
                    resDt.Load(result);
                    connection.Close();
                }
            }
            Acoustic acoustic = new AcousticService(this._databaseManager).GetAcousticById((int)resDt.Rows[0]["AcousticId"]);

            return new Rating(
                id: (int)((long)resDt.Rows[0]["Id"]),
                story: (int)((long)resDt.Rows[0]["Story"]),
                animation: (int)((long)resDt.Rows[0]["Animation"]),
                specialEffects: (int)((long)resDt.Rows[0]["SpecialEffects"]),
                acoustic: acoustic,
                isRated: (bool)resDt.Rows[0]["IsRated"]
            );
        }

        public bool UpdateRating(Rating ratingToUpdate) {
            if (!(new AcousticService(this._databaseManager).UpdateAcoustic(ratingToUpdate.Acoustic))) {
                return false;
            }

            string insertQuery = @"
                Update Rating 
                SET
                    Story = @Story,
                    Animation = @Animation,
                    SpecialEffects = @SpecialEffects,
                    AcousticId = @AcousticId,
                    IsRated = @IsRated
                WHERE Id = @id;
            ";
            using (SqliteConnection connection = this._databaseManager.GetConnection()) {
                connection.Open();
                try {
                    using (SqliteCommand cmd = new SqliteCommand(insertQuery, connection)) {
                        cmd.Parameters.AddWithValue("@Story", ratingToUpdate.Story);
                        cmd.Parameters.AddWithValue("@Animation", ratingToUpdate.Animation);
                        cmd.Parameters.AddWithValue("@SpecialEffects", ratingToUpdate.SpecialEffects);
                        cmd.Parameters.AddWithValue("@AcousticId", ratingToUpdate.Acoustic.Id);
                        cmd.Parameters.AddWithValue("@IsRated", ratingToUpdate.IsRated);
                        cmd.Parameters.AddWithValue("@Id", ratingToUpdate.IsRated);
                        cmd.ExecuteScalar();
                    }
                }
                catch (Exception ex) {
                    Log.Error(ex.StackTrace ?? "Error while inserting ratingToUpdate data");
                    return false;
                }
                connection.Close();
            }

            return true;
        }
    }
}
