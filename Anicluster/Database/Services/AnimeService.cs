using Anicluster.Database.Services.AssociativeEntities;
using Microsoft.Data.Sqlite;
using Modells.Anime;
using Serilog;
using System.Data;

namespace Anicluster.Database.Services {

    public class AnimeService : IDatabaseService {

        private readonly DatabaseManager _databaseManager;

        public AnimeService(DatabaseManager databaseManager) {
            this._databaseManager = databaseManager;
        }

        /// <summary>
        /// Initialize (create) table. NO MORE
        /// </summary>
        /// <returns></returns>
        public bool InitializeTable() {
            using (SqliteConnection connection = this._databaseManager.GetConnection()) {
                string creationString = @"
                    CREATE TABLE IF NOT EXISTS Anime (
                        Id INTEGER PRIMARY KEY,
                        Name TEXT NOT NULL,
                        OriginalName TEXT,
                        Url TEXT,
                        Favorite BOOLEAN NOT NULL DEFAULT 0,
                        Tier INTEGER NOT NULL,
                        RecommendedFrom TEXT,
                        Predecessor INTEGER DEFAULT -1,
                        Successor INTEGER DEFAULT -1,
                        Related INTEGER DEFAULT -1,
                        Comment TEXT,
                        RatingId INTEGER,
                        MediaInfoId INTEGER,
                        StatusId INTEGER, 
                        FOREIGN KEY (RatingId) REFERENCES Rating(Id),
                        FOREIGN KEY (MediaInfoId) REFERENCES MediaInfo(Id),
                        FOREIGN KEY (StatusId) REFERENCES Status(Id)
                    );
                ";
                using (SqliteCommand cmd = new SqliteCommand(creationString, connection)) {
                    try {
                        connection.Open();
                        cmd.ExecuteNonQuery();
                        connection.Close();
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
            using (SqliteConnection connection = this._databaseManager.GetConnection()) {
                connection.Open();
                string query = "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName;";
                using (SqliteCommand cmd = new SqliteCommand(query, connection)) {
                    cmd.Parameters.AddWithValue("@tableName", "Anime");
                    object? result = cmd.ExecuteScalar();
                    connection.Close();
                    if (result is not null) {
                        Log.Information("Tabelle 'Anime' existiert.");
                        return true;
                    }
                    else {
                        Log.Information("Tabelle 'Anime' existiert NICHT!");
                        return false;
                    }
                }
            }
        }

        public bool Insert(Anime animeToInsert) {
            // First get the Foreign-Key's
            int ratingId = new RatingService(this._databaseManager).Insert(animeToInsert.Rating);
            if (ratingId.Equals(-1)) {
                Log.Error("Break by inserting Rating...");
                return false;
            }
            int mediaInfoId = new MediaInfoService(this._databaseManager).Insert(animeToInsert.MediaInfo);
            if (mediaInfoId.Equals(-1)) {
                Log.Error("Break by inserting MediaInfo...");
                return false;
            }
            int statusId = new StatusService(this._databaseManager).Insert(animeToInsert.Status);
            if (statusId.Equals(-1)) {
                Log.Error("Break by inserting Status...");
                return false;
            }

            // Insert the Anime and get the Id for the Other Key's
            using (SqliteConnection connection = this._databaseManager.GetConnection()) {
                connection.Open();
                try {
                    #region Anime
                    // Anime einfügen und ID abfragen
                    //   -> FOREIGN KEYs von davor verwenden (da ist ja kein Mapping notwendig)
                    string insertAnimeQuery = @"
                        INSERT INTO Anime (Name, OriginalName, Url, Favorite, Tier, RecommendedFrom, Predecessor, Successor, Related, Comment, RatingId, MediaInfoId, StatusId)
                            VALUES (@Name,
                                    @OriginalName,
                                    @Url,
                                    @Favorite,
                                    @Tier,
                                    @RecommendedFrom,
                                    @Predecessor,
                                    @Successor,
                                    @Related,
                                    @Comment,
                                    @RatingId,
                                    @MediaInfoId,
                                    @StatusId
                        );
                    ";
                    using (SqliteCommand cmd = new SqliteCommand(insertAnimeQuery, connection)) {
                        cmd.Parameters.AddWithValue("@Name", animeToInsert.Name);
                        cmd.Parameters.AddWithValue("@OriginalName", animeToInsert.OriginalName ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Url", animeToInsert.Url ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Favorite", animeToInsert.Favorite);
                        cmd.Parameters.AddWithValue("@Tier", animeToInsert.Tier);
                        cmd.Parameters.AddWithValue("@RecommendedFrom", animeToInsert.RecommendedFrom ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Predecessor", animeToInsert.Predecessor);
                        cmd.Parameters.AddWithValue("@Successor", animeToInsert.Successor);
                        cmd.Parameters.AddWithValue("@Related", animeToInsert.Related);
                        cmd.Parameters.AddWithValue("@Comment", animeToInsert.Comment ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@RatingId", ratingId);
                        cmd.Parameters.AddWithValue("@MediaInfoId", mediaInfoId);
                        cmd.Parameters.AddWithValue("@StatusId", statusId);
                        cmd.ExecuteNonQuery();
                        // request the id
                        cmd.Parameters.Clear();
                        cmd.CommandText = "SELECT last_insert_rowid();";
                        object? result = cmd.ExecuteScalar();
                        connection.Close();
                        animeToInsert.Id = (int)((long)result!);
                    }
                    #endregion
                    #region "Mapping"
                    // Seasons einfügen
                    List<int> seasonIds = [];
                    SeasonService seasonService = new SeasonService(this._databaseManager);
                    foreach (Season x in animeToInsert.Seasons) {
                        seasonIds.Add(seasonService.Insert(x));
                    }
                    // AnimeId und SeasonIds mappen
                    AnimeSeasonAssociativeEntityService asaes = new AnimeSeasonAssociativeEntityService(this._databaseManager);
                    foreach (int x in seasonIds) {
                        asaes.Insert(animeToInsert.Id, x);
                    }
                    // Movies, OVAs, etc. einfügen
                    List<int> insertedVAIds = new List<int>(animeToInsert.Ovas.Count);
                    VideoAnimationService vas = new VideoAnimationService(this._databaseManager);
                    foreach (VideoAnimation x in animeToInsert.Ovas) {
                        insertedVAIds.Add(vas.Insert(x));
                    }
                    // AnimeId und VideoAnimationIds mappen
                    AnimeOvaAssociativeEntityService aoaes = new AnimeOvaAssociativeEntityService(this._databaseManager);
                    foreach (int x in insertedVAIds) {
                        aoaes.Insert(animeToInsert.Id, x);
                    }
                    // Tags per Id auf die Animes mappen (die Tags sind ja schon da, also die nicht mehr einfügen!)
                    AnimeTagAssociativeEntityService ataes = new AnimeTagAssociativeEntityService(this._databaseManager);
                    foreach (Tag x in animeToInsert.Tags) {
                        ataes.Insert(animeToInsert.Id, x.Id);
                    }
                    #endregion
                }
                catch (Exception ex) {
                    Log.Error(ex.StackTrace ?? "Error while inserting animeToInsert data");
                    return false;
                }
                connection.Close();
            }
            return true;
        }

        public bool InsertFast(string animeName, bool animeFavorite, Tier animeTier) {
            using (SqliteConnection connection = this._databaseManager.GetConnection()) {
                connection.Open();
                try {
                    string insertAnimeQuery = @"
                        INSERT INTO Anime (Name, Favorite, Tier)
                            VALUES (@Name,
                                    @Favorite,
                                    @Tier
                        );
                    ";
                    using (SqliteCommand cmd = new SqliteCommand(insertAnimeQuery, connection)) {
                        cmd.Parameters.AddWithValue("@Name", animeName);
                        cmd.Parameters.AddWithValue("@Favorite", animeFavorite);
                        cmd.Parameters.AddWithValue("@Tier", animeTier);
                        cmd.ExecuteNonQuery();
                        // request the id
                        cmd.Parameters.Clear();
                        cmd.CommandText = "SELECT last_insert_rowid();";
                        object? result = cmd.ExecuteScalar();
                        connection.Close();
                    }
                }
                catch (Exception ex) {
                    Log.Error(ex.StackTrace ?? "Error while inserting a small Anime data");
                    return false;
                }
                connection.Close();
            }
            return true;
        }

        public Anime? SelectAnimeByX(int id = -1, string designation = "") {
            if (id.Equals(-1) && designation.Equals("")) {
                return null;
            }

            Anime selectedAnime = new Anime();

            string query = id == -1 ? @"SELECT * FROM Anime WHERE Name = @A;" : @"SELECT * FROM Anime WHERE Id = @A;";

            DataTable resDt = new DataTable();
            using (SqliteConnection connection = this._databaseManager.GetConnection()) {
                using (SqliteCommand cmd = new SqliteCommand(query, connection)) {
                    cmd.Parameters.AddWithValue("@A", id == -1 ? designation : id);
                    connection.Open();
                    SqliteDataReader result = cmd.ExecuteReader();
                    resDt.Load(result);
                    connection.Close();
                }
            }
            if (resDt.Rows.Count.Equals(0)) {
                return null;
            }
            selectedAnime.Id = (int)((long)resDt.Rows[0]["Id"]);
            selectedAnime.Name = resDt.Rows[0]["Name"].ToString() ?? "";
            selectedAnime.OriginalName = resDt.Rows[0]["OriginalName"].ToString();
            selectedAnime.Url = resDt.Rows[0]["Url"].ToString() ?? "";
            selectedAnime.Favorite = Convert.ToBoolean(resDt.Rows[0]["Favorite"]);
            selectedAnime.Tier = (Tier)Enum.Parse(typeof(Tier), resDt.Rows[0]["Tier"].ToString() ?? "NotDefinied");
            selectedAnime.RecommendedFrom = resDt.Rows[0]["RecommendedFrom"].ToString();
            selectedAnime.Predecessor = (int)((long)resDt.Rows[0]["Predecessor"]);
            selectedAnime.Successor = (int)((long)resDt.Rows[0]["Successor"]);
            selectedAnime.Related = (int)((long)resDt.Rows[0]["Related"]);
            selectedAnime.Comment = resDt.Rows[0]["Comment"].ToString();

            selectedAnime.Rating = new RatingService(this._databaseManager).GetRatingById((int)((long)resDt.Rows[0]["RatingId"]));
            selectedAnime.MediaInfo = new MediaInfoService(this._databaseManager).GetMediaInfoById((int)((long)resDt.Rows[0]["MediaInfoId"]));
            selectedAnime.Status = new StatusService(this._databaseManager).GetStatusById((int)((long)resDt.Rows[0]["StatusId"]));

            List<int> ovaIds = new AnimeOvaAssociativeEntityService(this._databaseManager).GetOvaIdsByAnimeId(selectedAnime.Id);
            selectedAnime.Ovas = new VideoAnimationService(this._databaseManager).GetOvas(ovaIds);

            List<int> seasonIds = new AnimeSeasonAssociativeEntityService(this._databaseManager).GetSeasonIdsByAnimeId(selectedAnime.Id);
            selectedAnime.Seasons = new SeasonService(this._databaseManager).GetSeasonsByIds(seasonIds);

            List<int> tagIds = new AnimeTagAssociativeEntityService(this._databaseManager).GetTagIdsForAnimeId(selectedAnime.Id);
            selectedAnime.Tags = new TagService(this._databaseManager).GetTagsByIds(tagIds);

            return selectedAnime;
        }

        public bool UpdateAnime(Anime animeToUpdate) {
            DataTable resDt = new DataTable();
            using (SqliteConnection connection = this._databaseManager.GetConnection()) {
                using (SqliteCommand cmd = new SqliteCommand("SELECT * FROM Anime WHERE Id = @A;", connection)) {
                    cmd.Parameters.AddWithValue("@A", animeToUpdate.Id);
                    connection.Open();
                    SqliteDataReader result = cmd.ExecuteReader();
                    resDt.Load(result);
                    connection.Close();
                }
            }
            if (resDt.Rows.Count.Equals(0)) {
                return false; // anime not found, reason unclear
            }

            string updateAnimeBase = @"
                UPDATE Anime
                SET
                    Name = @Name,
                    OriginalName = @OriginalName,
                    Url = @Url,
                    Favorite = @Favorite,
                    Tier = @Tier,
                    RecommendedFrom = @RecommendedFrom,
                    Predecessor = @Predecessor,
                    Successor = @Successor,
                    Related = @Related,
                    Comment = @Comment
                WHERE Id = @id;
            ";
            using (SqliteConnection connection = this._databaseManager.GetConnection()) {
                using (SqliteCommand cmd = new SqliteCommand(updateAnimeBase, connection)) {
                    cmd.Parameters.AddWithValue("@id", animeToUpdate.Id);
                    cmd.Parameters.AddWithValue("@Name", animeToUpdate.Name);
                    cmd.Parameters.AddWithValue("@OriginalName", animeToUpdate.OriginalName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Url", animeToUpdate.Url ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Favorite", animeToUpdate.Favorite);
                    cmd.Parameters.AddWithValue("@Tier", animeToUpdate.Tier);
                    cmd.Parameters.AddWithValue("@RecommendedFrom", animeToUpdate.RecommendedFrom ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Predecessor", animeToUpdate.Predecessor);
                    cmd.Parameters.AddWithValue("@Successor", animeToUpdate.Successor);
                    cmd.Parameters.AddWithValue("@Related", animeToUpdate.Related);
                    cmd.Parameters.AddWithValue("@Comment", animeToUpdate.Comment ?? (object)DBNull.Value);
                    cmd.ExecuteNonQuery();
                    connection.Close();
                }
            }

            if (!(new RatingService(this._databaseManager).UpdateRating(animeToUpdate.Rating))) {
                return false;
            }
            if (!(new MediaInfoService(this._databaseManager).UpdateMediaInfo(animeToUpdate.MediaInfo))) {
                return false;
            }
            if (!(new StatusService(this._databaseManager).UpdateStatus(animeToUpdate.Status))) {
                return false;
            }

            return true;
        }

        #region View(s)
        /// <summary>
        /// Complete not include the Lists, only the id's that are neccassery for the Lists are included like pubId.
        /// </summary>
        /// <returns></returns>
        public bool CreateViewAnimeComplete() {
            using (SqliteConnection connection = this._databaseManager.GetConnection()) {
                string animeView = $"SELECT name FROM sqlite_master WHERE type='view' AND name='vw_Anime';";
                using (SqliteCommand cmd = new SqliteCommand(animeView, connection)) {
                    connection.Open();
                    object? result = cmd.ExecuteScalar();
                    if (result is null) {
                        string createViewQuery = @"
                            CREATE VIEW IF NOT EXISTS vw_Anime AS
                            SELECT
                                a.Id AS AnimeId,
                                a.Name AS AnimeName,
                                a.OriginalName,
                                a.Url,
                                a.Favorite,
                                a.Tier,
                                a.RecommendedFrom,
                                a.Predecessor,
                                a.Successor,
                                a.Related,
                                a.Comment,
                                r.Story AS RatingStory,
                                r.Animation AS RatingAnimation,
                                r.SpecialEffects AS RatingSpecialEffects,
                                r.IsRated AS RatingIsRated,
                                ac.Id AS AcousticId,
                                ac.Soundtrack AS AcousticSoundtrack,
                                ac.Comment AS AcousticComment,
                                m.Author AS MediaInfoAuthor,
                                m.Producer AS MediaInfoProducer,
                                m.Publisher AS MediaInfoPublisher,
                                pt.Id AS PublishingTimeId,
                                pt.StartDate AS PublishingStartDate,
                                pt.EndDate AS PublishingEndDate,
                                s.Id AS StatusId,
                                s.State AS StatusState,
                                s.Comment AS StatusComment
                            FROM Anime a
                            LEFT JOIN Rating r ON a.RatingId = r.Id
                            LEFT JOIN Acoustic ac ON r.AcousticId = ac.Id
                            LEFT JOIN MediaInfo m ON a.MediaInfoId = m.Id
                            LEFT JOIN PublishingTime pt ON m.PublishingTimeId = pt.Id
                            LEFT JOIN Status s ON a.StatusId = s.Id;
                        ";
                        try {
                            using (SqliteCommand createCmd = new SqliteCommand(createViewQuery, connection)) {
                                createCmd.ExecuteNonQuery();
                                Log.Information("View vw_Anime created.");
                            }
                        }
                        catch (Exception ex) {
                            Log.Error(ex.StackTrace ?? "Error while create view vw_Anime");
                            return false;
                        }
                    }
                    connection.Close();
                }
            }
            return true;
        }
        
        public Anime? SelectAnimeByXPerView(int id = -1, string designation = "") {
            string query = id == -1 ? @"SELECT * FROM vw_Anime WHERE AnimeName = @AnimeName;" : @"SELECT * FROM vw_Anime WHERE AnimeId = @AnimeName;";

            return null;
        }
        #endregion
    }
}
