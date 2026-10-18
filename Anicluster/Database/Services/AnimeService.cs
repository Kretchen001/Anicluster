using Microsoft.Data.Sqlite;
using Modells.Anime;
using Serilog;

namespace Anicluster.Database.Services {

    /// <summary>
    /// Liest und schreibt Animes. Ein Anime wird immer als Ganzes in einer Transaktion gespeichert:
    /// Stammdaten per INSERT/UPDATE, 1:1-Daten (Status, Bewertung, Medieninfo) per Anlegen oder Aktualisieren,
    /// Listen (Staffeln, OVAs, Tags, Musikstücke, ...) werden beim Speichern ersetzt.
    /// </summary>
    public class AnimeService : IAnimeService {

        private const string SelectAnimeColumns = "Id, Name, OriginalName, Url, Favorite, Tier, RecommendedFrom, Predecessor, Successor, Related, Comment";

        private readonly DatabaseManager _databaseManager;
        private readonly StatusService _statusService = new StatusService();
        private readonly RatingService _ratingService = new RatingService();
        private readonly MediaInfoService _mediaInfoService = new MediaInfoService();
        private readonly SeasonService _seasonService = new SeasonService();
        private readonly VideoAnimationService _videoAnimationService = new VideoAnimationService();
        private readonly TagService _tagService;

        public AnimeService(DatabaseManager databaseManager) {
            this._databaseManager = databaseManager;
            this._tagService = new TagService(databaseManager);
        }

        public bool Insert(Anime animeToInsert) {
            try {
                using (DatabaseSession session = this._databaseManager.BeginSession()) {
                    animeToInsert.Id = session.Insert(
                        @"INSERT INTO Anime (Name, OriginalName, Url, Favorite, Tier, RecommendedFrom, Predecessor, Successor, Related, Comment)
                            VALUES (@Name, @OriginalName, @Url, @Favorite, @Tier, @RecommendedFrom, @Predecessor, @Successor, @Related, @Comment);",
                        ("@Name", animeToInsert.Name), ("@OriginalName", animeToInsert.OriginalName), ("@Url", animeToInsert.Url),
                        ("@Favorite", animeToInsert.Favorite), ("@Tier", animeToInsert.Tier), ("@RecommendedFrom", animeToInsert.RecommendedFrom),
                        ("@Predecessor", animeToInsert.Predecessor), ("@Successor", animeToInsert.Successor), ("@Related", animeToInsert.Related),
                        ("@Comment", animeToInsert.Comment)
                    );
                    this.SaveRelatedData(session, animeToInsert);
                    session.Commit();
                }
                return true;
            }
            catch (Exception ex) {
                Log.Error(ex, "Anime '{AnimeName}' konnte nicht eingefügt werden.", animeToInsert.Name);
                animeToInsert.Id = 0;
                return false;
            }
        }

        public bool InsertFast(string animeName, bool animeFavorite, Tier animeTier) {
            return this.Insert(new Anime() { Name = animeName, Favorite = animeFavorite, Tier = animeTier });
        }

        public Anime? SelectAnimeByX(int id = -1, string designation = "") {
            if (id == -1 && designation == "") {
                return null;
            }
            try {
                using (DatabaseSession session = this._databaseManager.BeginSession()) {
                    string condition = id == -1 ? "Name = @Value" : "Id = @Value";
                    object value = id == -1 ? (object)designation : id;
                    Anime? anime = session.Query<Anime>(
                        $"SELECT {SelectAnimeColumns} FROM Anime WHERE {condition};",
                        MapAnime,
                        ("@Value", value)
                    ).FirstOrDefault();
                    if (anime is null) {
                        return null;
                    }
                    anime.Status = this._statusService.SelectByAnimeId(session, anime.Id);
                    anime.Rating = this._ratingService.SelectByAnimeId(session, anime.Id);
                    anime.MediaInfo = this._mediaInfoService.SelectByAnimeId(session, anime.Id);
                    anime.Seasons = this._seasonService.SelectByAnimeId(session, anime.Id);
                    anime.Ovas = this._videoAnimationService.SelectByAnimeId(session, anime.Id);
                    anime.Tags = this._tagService.SelectByAnimeId(session, anime.Id);
                    return anime;
                }
            }
            catch (Exception ex) {
                Log.Error(ex, "Anime (Id: {AnimeId}, Name: '{AnimeName}') konnte nicht geladen werden.", id, designation);
                return null;
            }
        }

        public bool UpdateAnime(Anime animeToUpdate) {
            try {
                using (DatabaseSession session = this._databaseManager.BeginSession()) {
                    int affectedRows = session.Execute(
                        @"UPDATE Anime
                            SET Name = @Name, OriginalName = @OriginalName, Url = @Url, Favorite = @Favorite, Tier = @Tier,
                                RecommendedFrom = @RecommendedFrom, Predecessor = @Predecessor, Successor = @Successor,
                                Related = @Related, Comment = @Comment
                            WHERE Id = @Id;",
                        ("@Name", animeToUpdate.Name), ("@OriginalName", animeToUpdate.OriginalName), ("@Url", animeToUpdate.Url),
                        ("@Favorite", animeToUpdate.Favorite), ("@Tier", animeToUpdate.Tier), ("@RecommendedFrom", animeToUpdate.RecommendedFrom),
                        ("@Predecessor", animeToUpdate.Predecessor), ("@Successor", animeToUpdate.Successor), ("@Related", animeToUpdate.Related),
                        ("@Comment", animeToUpdate.Comment), ("@Id", animeToUpdate.Id)
                    );
                    if (affectedRows == 0) {
                        Log.Error("Anime mit der Id {AnimeId} existiert nicht und kann nicht aktualisiert werden.", animeToUpdate.Id);
                        return false;
                    }
                    this.SaveRelatedData(session, animeToUpdate);
                    session.Commit();
                }
                return true;
            }
            catch (Exception ex) {
                Log.Error(ex, "Anime '{AnimeName}' (Id: {AnimeId}) konnte nicht aktualisiert werden.", animeToUpdate.Name, animeToUpdate.Id);
                return false;
            }
        }

        public bool DeleteAnime(int animeId) {
            try {
                using (DatabaseSession session = this._databaseManager.BeginSession()) {
                    // Verweise anderer Animes auf diesen zurücksetzen
                    session.Execute("UPDATE Anime SET Predecessor = -1 WHERE Predecessor = @Id;", ("@Id", animeId));
                    session.Execute("UPDATE Anime SET Successor = -1 WHERE Successor = @Id;", ("@Id", animeId));
                    session.Execute("UPDATE Anime SET Related = -1 WHERE Related = @Id;", ("@Id", animeId));
                    // Alle abhängigen Daten werden über ON DELETE CASCADE mit entfernt
                    int affectedRows = session.Execute("DELETE FROM Anime WHERE Id = @Id;", ("@Id", animeId));
                    if (affectedRows == 0) {
                        return false;
                    }
                    session.Commit();
                }
                return true;
            }
            catch (Exception ex) {
                Log.Error(ex, "Anime (Id: {AnimeId}) konnte nicht gelöscht werden.", animeId);
                return false;
            }
        }

        public List<AnimeListEntry> SelectAnimeListEntries() {
            try {
                using (DatabaseSession session = this._databaseManager.BeginSession()) {
                    return session.Query(
                        "SELECT a.Id, a.Name, a.Favorite, a.Tier, COALESCE(s.State, 0) FROM Anime a LEFT JOIN Status s ON s.AnimeId = a.Id ORDER BY a.Id;",
                        reader => new AnimeListEntry(
                            reader.GetInt(0),
                            reader.GetString(1),
                            reader.GetBool(2),
                            (Tier)reader.GetInt(3),
                            (State)reader.GetInt(4)
                        )
                    );
                }
            }
            catch (Exception ex) {
                Log.Error(ex, "Die Anime-Liste konnte nicht geladen werden.");
                return [];
            }
        }

        /// <summary>Speichert alles, was zum Anime gehört. Setzt dabei die Ids der Kind-Objekte im Modell.</summary>
        private void SaveRelatedData(DatabaseSession session, Anime anime) {
            this._statusService.Save(session, anime.Id, anime.Status);
            this._ratingService.Save(session, anime.Id, anime.Rating);
            this._mediaInfoService.Save(session, anime.Id, anime.MediaInfo);
            this._seasonService.ReplaceForAnime(session, anime.Id, anime.Seasons);
            this._videoAnimationService.ReplaceForAnime(session, anime.Id, anime.Ovas);
            this._tagService.ReplaceForAnime(session, anime.Id, anime.Tags);
        }

        /// <summary>Wandelt eine Zeile (Spalten wie in <see cref="SelectAnimeColumns"/>) in einen Anime um.</summary>
        private static Anime MapAnime(SqliteDataReader reader) {
            return new Anime() {
                Id = reader.GetInt(0),
                Name = reader.GetString(1),
                OriginalName = reader.GetStringOrNull(2),
                Url = reader.GetStringOrNull(3),
                Favorite = reader.GetBool(4),
                Tier = (Tier)reader.GetInt(5),
                RecommendedFrom = reader.GetStringOrNull(6),
                Predecessor = reader.GetInt(7),
                Successor = reader.GetInt(8),
                Related = reader.GetInt(9),
                Comment = reader.GetStringOrNull(10)
            };
        }
    }
}
