using Microsoft.Data.Sqlite;
using Modells.Anime;
using Serilog;
using System.Security.Cryptography;

namespace Anicluster.Database.Services {

    public class AnimeService {

        private readonly DatabaseManager _databaseManager;

        public AnimeService(DatabaseManager databaseManager) {
            _databaseManager = databaseManager;
        }

        public bool InitializeAnimeTable() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                string creationString = "" +
                    "CREATE TABLE IF NOT EXISTS Anime (" +
                    "    Id INTEGER PRIMARY KEY," +
                    "    Name TEXT NOT NULL," +
                    "    OriginalName TEXT," +
                    "    Url TEXT," +
                    "    Favorite BOOLEAN NOT NULL DEFAULT 0," +
                    "    Tier INTEGER NOT NULL," +
                    "    RecommendedFrom TEXT," +
                    "    Predecessor INTEGER," +
                    "    Successor INTEGER," +
                    "    Related INTEGER," +
                    "    Comment TEXT," +
                    "    RatingId INTEGER," +
                    "    MediaInfoId INTEGER," +
                    "    StatusId INTEGER, " +
                    "    FOREIGN KEY (RatingId) REFERENCES Rating(Id)," +
                    "    FOREIGN KEY (MediaInfoId) REFERENCES MediaInfo(Id)," +
                    "    FOREIGN KEY (StatusId) REFERENCES Status(Id)" +
                    ");";
                using (SqliteCommand cmd = new SqliteCommand(creationString, connection)) {
                    try {
                        connection.Open();
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex) {
                        Log.Error(ex.StackTrace ?? "Error without stacktrace... <- Anime table not created!");
                        return false;
                    }
                }
            }
            Log.Information("Anime table created.");
            return true;
        }

        public bool TableExist() {
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                connection.Open();
                string query = "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName;";
                using (SqliteCommand cmd = new SqliteCommand(query, connection)) {
                    cmd.Parameters.AddWithValue("@tableName", "Anime");
                    object? result = cmd.ExecuteScalar();

                    return result != null;
                }
            }
        }

        // Insert Merke
        public bool InsertAnime(Anime animeToInsert) {
            // First get the Foreign-Key's
            // Rating
            int ratingId = new RatingService(_databaseManager).InsertRating(animeToInsert.Rating);
            if (ratingId.Equals(-1)) {
                Log.Error("Break by inserting Rating...");
                return false;
            }
            int mediaInfoId = new MediaInfoService(_databaseManager).InsertMediaInfo(animeToInsert.MediaInfo);
            if (mediaInfoId.Equals(-1)) {
                Log.Error("Break by inserting MediaInfo...");
                return false;
            }
            int statusId = new StatusService(_databaseManager).InsertStatus(animeToInsert.Status);
            if (statusId.Equals(-1)) {
                Log.Error("Break by inserting Status...");
                return false;
            }

            // Insert the Anime and get the Id for the Other Key's
            using (SqliteConnection connection = _databaseManager.GetConnection()) {
                connection.Open();
                using (SqliteTransaction transaction = connection.BeginTransaction()) {
                    try {
                        // Anime einfügen und ID abfragen
                        //   -> FOREIGN KEYs von davor verwenden (da ist ja kein Mapping notwendig
                        // Seasons einzufügen
                        // AnimeId und SeasonIds mappen
                        // VideoAnimation einfügen
                        // AnimeId und VideoAnimationIds mappen
                        // Tags per Id auf die Animes mappen (die Tags sind ja schon da, also die nicht mehr einfügen!

                        #region Anime
                        //string insertAnime = "INSERT INTO Anime (Id, Name, OriginalName, Url, Favorite, RatingId, Tier, RecommendedFrom, Predecessor, Successor, Related, Comment) " +
                        //                     "VALUES (@Id, @Name, @OriginalName, @Url, @Favorite, @RatingId, @Tier, @RecommendedFrom, @Predecessor, @Successor, @Related, @Comment);";
                        //using (var cmd = new SqliteCommand(insertAnime, connection, transaction)) {
                        //    cmd.Parameters.AddWithValue("@Id", animeToInsert.Id);
                        //    cmd.Parameters.AddWithValue("@Name", animeToInsert.Name);
                        //    cmd.Parameters.AddWithValue("@OriginalName", animeToInsert.OriginalName ?? (object)DBNull.Value);
                        //    cmd.Parameters.AddWithValue("@Url", animeToInsert.Url ?? (object)DBNull.Value);
                        //    cmd.Parameters.AddWithValue("@Favorite", animeToInsert.Favorite);
                        //    cmd.Parameters.AddWithValue("@RatingId", ratingId); // RatingId setzen
                        //    cmd.Parameters.AddWithValue("@Tier", animeToInsert.Tier.ToString());
                        //    cmd.Parameters.AddWithValue("@RecommendedFrom", animeToInsert.RecommendedFrom ?? (object)DBNull.Value);
                        //    cmd.Parameters.AddWithValue("@Predecessor", animeToInsert.Predecessor);
                        //    cmd.Parameters.AddWithValue("@Successor", animeToInsert.Successor);
                        //    cmd.Parameters.AddWithValue("@Related", animeToInsert.Related);
                        //    cmd.Parameters.AddWithValue("@Comment", animeToInsert.Comment ?? (object)DBNull.Value);
                        //    cmd.ExecuteNonQuery();
                        //}
                        #endregion
                        #region "Mapping"
                        //    // Mapping von Anime und Tags einfügen
                        //    string insertAnimeTag = "INSERT INTO AnimeTag (AnimeId, TagId) VALUES (@AnimeId, @TagId);";
                        //    using (var cmd = new SqliteCommand(insertAnimeTag, connection, transaction)) {
                        //        cmd.Parameters.AddWithValue("@AnimeId", animeToInsert.Id);
                        //        // Hier müsste die korrekte ID des Tags verwendet werden, z.B. durch ein SELECT
                        //        cmd.Parameters.AddWithValue("@TagId", /* ID des Tags hier */);
                        //        cmd.ExecuteNonQuery();
                        //    }
                        //}
                        #endregion

                        // Commit der Transaktion
                        transaction.Commit();
                    }
                    catch (Exception ex) {
                        transaction.Rollback();
                        Log.Error(ex.StackTrace ?? "Error while inserting animeToInsert data");
                        return false;
                    }
                }
                connection.Close();
            }
            return true;
        }
    }
}
