using Microsoft.Data.Sqlite;
using Modells.Anime;
using Serilog;

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
                        //// 1. Rating einfügen und die generierte ID abrufen
                        //int ratingId;
                        //string insertRating = "INSERT INTO Rating (Story, Animation, SpecialEffects, Acoustic, IsRated) " +
                        //                      "VALUES (@Story, @Animation, @SpecialEffects, @Acoustic, @IsRated);";
                        //using (var cmd = new SqliteCommand(insertRating, connection, transaction)) {
                        //    cmd.Parameters.AddWithValue("@Story", animeToInsert.Rating.Story);
                        //    cmd.Parameters.AddWithValue("@Animation", animeToInsert.Rating.Animation);
                        //    cmd.Parameters.AddWithValue("@SpecialEffects", animeToInsert.Rating.SpecialEffects);
                        //    cmd.Parameters.AddWithValue("@Acoustic", JsonConvert.SerializeObject(animeToInsert.Rating.Acoustic)); // JSON-Format für Acoustic
                        //    cmd.Parameters.AddWithValue("@IsRated", animeToInsert.Rating.IsRated);
                        //    cmd.ExecuteNonQuery();
                        //}

                        //// Holen Sie sich die letzte eingefügte ID
                        //ratingId = (int)connection.LastInsertRowId();

                        //// 2. Anime einfügen
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

                        //// 3. MediaInfo einfügen
                        //string insertMediaInfo = "INSERT INTO MediaInfo (AnimeId, Author, Producer, Publisher) " +
                        //                          "VALUES (@AnimeId, @Author, @Producer, @Publisher);";
                        //using (var cmd = new SqliteCommand(insertMediaInfo, connection, transaction)) {
                        //    cmd.Parameters.AddWithValue("@AnimeId", animeToInsert.Id);
                        //    cmd.Parameters.AddWithValue("@Author", animeToInsert.MediaInfo.Author ?? (object)DBNull.Value);
                        //    cmd.Parameters.AddWithValue("@Producer", animeToInsert.MediaInfo.Producer ?? (object)DBNull.Value);
                        //    cmd.Parameters.AddWithValue("@Publisher", animeToInsert.MediaInfo.Publisher ?? (object)DBNull.Value);
                        //    cmd.ExecuteNonQuery();
                        //}

                        //// 4. Seasons einfügen
                        //foreach (var season in animeToInsert.Season) {
                        //    string insertSeason = "INSERT INTO Season (AnimeId, Number, Comment) " +
                        //                          "VALUES (@AnimeId, @Number, @Comment);";
                        //    using (var cmd = new SqliteCommand(insertSeason, connection, transaction)) {
                        //        cmd.Parameters.AddWithValue("@AnimeId", animeToInsert.Id);
                        //        cmd.Parameters.AddWithValue("@Number", season.Number);
                        //        cmd.Parameters.AddWithValue("@Comment", season.Comment ?? (object)DBNull.Value);
                        //        cmd.ExecuteNonQuery();

                        //        // 5. VideoAnimation für die Season einfügen
                        //        foreach (var videoAnimation in season.Episodes) {
                        //            string insertVideoAnimation = "INSERT INTO VideoAnimation (SeasonId, AnimeId, VideoType, Comment) " +
                        //                                          "VALUES (@SeasonId, @AnimeId, @VideoType, @Comment);";
                        //            using (var cmd2 = new SqliteCommand(insertVideoAnimation, connection, transaction)) {
                        //                cmd2.Parameters.AddWithValue("@SeasonId", season.Number); // Hier müsste die korrekte ID verwendet werden
                        //                cmd2.Parameters.AddWithValue("@AnimeId", animeToInsert.Id);
                        //                cmd2.Parameters.AddWithValue("@VideoType", (int)videoAnimation.VideoType);
                        //                cmd2.Parameters.AddWithValue("@Comment", videoAnimation.Comment ?? (object)DBNull.Value);
                        //                cmd2.ExecuteNonQuery();
                        //            }
                        //        }
                        //    }
                        //}

                        //// 6. Ovas einfügen
                        //foreach (var ova in animeToInsert.Ovas) {
                        //    string insertOva = "INSERT INTO VideoAnimation (AnimeId, VideoType, Comment) " +
                        //                       "VALUES (@AnimeId, @VideoType, @Comment);";
                        //    using (var cmd = new SqliteCommand(insertOva, connection, transaction)) {
                        //        cmd.Parameters.AddWithValue("@AnimeId", animeToInsert.Id);
                        //        cmd.Parameters.AddWithValue("@VideoType", (int)ova.VideoType);
                        //        cmd.Parameters.AddWithValue("@Comment", ova.Comment ?? (object)DBNull.Value);
                        //        cmd.ExecuteNonQuery();
                        //    }
                        //}

                        //// 7. Tags einfügen
                        //foreach (var tag in animeToInsert.Tags) {
                        //    string insertTag = "INSERT INTO Tag (Designation) VALUES (@Designation);";
                        //    using (var cmd = new SqliteCommand(insertTag, connection, transaction)) {
                        //        cmd.Parameters.AddWithValue("@Designation", tag.Designation);
                        //        cmd.ExecuteNonQuery();
                        //    }

                        //    // 8. Mapping von Anime und Tags einfügen
                        //    string insertAnimeTag = "INSERT INTO AnimeTag (AnimeId, TagId) VALUES (@AnimeId, @TagId);";
                        //    using (var cmd = new SqliteCommand(insertAnimeTag, connection, transaction)) {
                        //        cmd.Parameters.AddWithValue("@AnimeId", animeToInsert.Id);
                        //        // Hier müsste die korrekte ID des Tags verwendet werden, z.B. durch ein SELECT
                        //        cmd.Parameters.AddWithValue("@TagId", /* ID des Tags hier */);
                        //        cmd.ExecuteNonQuery();
                        //    }
                        //}

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
