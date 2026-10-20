using Microsoft.Data.Sqlite;
using Modells.Anime;
using Serilog;

namespace Anicluster.Database.Services {

    /// <summary>
    /// Liest und schreibt Animes. Ein Anime wird immer als Ganzes in einer Transaktion gespeichert:
    /// Stammdaten per INSERT/UPDATE, 1:1-Daten (Status, Bewertung, Medieninfo, Bild) per Anlegen oder Aktualisieren,
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
        private readonly ImageService _imageService = new ImageService();
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
                    anime.Image = this._imageService.SelectByAnimeId(session, anime.Id);
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
                        @"SELECT a.Id, a.Name, a.Favorite, a.Tier, COALESCE(s.State, 0),
                                 (SELECT GROUP_CONCAT(m.TagId) FROM AnimeTagAssociativeEntity m WHERE m.AnimeId = a.Id),
                                 EXISTS (SELECT 1 FROM AnimeImage i WHERE i.AnimeId = a.Id)
                            FROM Anime a
                            LEFT JOIN Status s ON s.AnimeId = a.Id
                            ORDER BY a.Id;",
                        reader => new AnimeListEntry(
                            reader.GetInt(0),
                            reader.GetString(1),
                            reader.GetBool(2),
                            (Tier)reader.GetInt(3),
                            (State)reader.GetInt(4),
                            ParseIds(reader.GetStringOrNull(5)),
                            reader.GetBool(6)
                        )
                    );
                }
            }
            catch (Exception ex) {
                Log.Error(ex, "Die Anime-Liste konnte nicht geladen werden.");
                return [];
            }
        }

        public List<TimelineEntry> SelectTimelineEntries() {
            try {
                using (DatabaseSession session = this._databaseManager.BeginSession()) {
                    List<TimelineBase> animes = session.Query(
                        @"SELECT a.Id, a.Name, a.Favorite, a.Tier, COALESCE(s.State, 0), pt.StartDate, pt.EndDate
                            FROM Anime a
                            LEFT JOIN Status s ON s.AnimeId = a.Id
                            LEFT JOIN MediaInfo mi ON mi.AnimeId = a.Id
                            LEFT JOIN PublishingTime pt ON pt.MediaInfoId = mi.Id
                            ORDER BY a.Id;",
                        reader => new TimelineBase(
                            reader.GetInt(0),
                            reader.GetString(1),
                            reader.GetBool(2),
                            (Tier)reader.GetInt(3),
                            (State)reader.GetInt(4),
                            ToSpan(reader.GetDate(5), reader.GetDate(6))
                        )
                    );
                    Dictionary<int, List<TimelineSeason>> seasons = [];
                    session.Query(
                        @"SELECT se.AnimeId, se.Number, pt.StartDate, pt.EndDate
                            FROM Season se
                            JOIN PublishingTime pt ON pt.SeasonId = se.Id
                            ORDER BY se.AnimeId, se.Number;",
                        reader => (
                            AnimeId: reader.GetInt(0),
                            Season: ToSeason(reader.GetInt(1), reader.GetDate(2), reader.GetDate(3)))
                    ).ForEach(x => {
                        if (x.Season is not null) {
                            AddTo(seasons, x.AnimeId, x.Season);
                        }
                    });
                    Dictionary<int, List<TimelineInterruption>> interruptions = [];
                    session.Query(
                        @"SELECT COALESCE(mi.AnimeId, se.AnimeId), i.StartDate, i.EndDate
                            FROM Interruption i
                            JOIN PublishingTime pt ON pt.Id = i.PublishingTimeId
                            LEFT JOIN MediaInfo mi ON mi.Id = pt.MediaInfoId
                            LEFT JOIN Season se ON se.Id = pt.SeasonId
                            ORDER BY i.StartDate;",
                        reader => (AnimeId: reader.GetInt(0), Start: reader.GetDate(1), End: reader.GetDate(2))
                    ).ForEach(x => {
                        if (x.Start.Year > 1 && x.End >= x.Start) {
                            AddTo(interruptions, x.AnimeId, new TimelineInterruption(x.Start, x.End));
                        }
                    });
                    return animes.Select(x => new TimelineEntry(
                        x.Id, x.Name, x.IsFavorite, x.Tier, x.State, x.Run,
                        seasons.GetValueOrDefault(x.Id) ?? [],
                        interruptions.GetValueOrDefault(x.Id) ?? []
                    )).ToList();
                }
            }
            catch (Exception ex) {
                Log.Error(ex, "Die Zeiträume für den Zeitstrahl konnten nicht geladen werden.");
                return [];
            }
        }

        /// <summary>Ein Anime mit seinem Gesamtzeitraum, noch ohne Staffeln und Unterbrechungen.</summary>
        private record TimelineBase(int Id, string Name, bool IsFavorite, Tier Tier, State State, TimelineSpan? Run);

        private static void AddTo<T>(Dictionary<int, List<T>> target, int animeId, T item) {
            if (!target.TryGetValue(animeId, out List<T>? list)) {
                list = [];
                target[animeId] = list;
            }
            list.Add(item);
        }

        /// <summary>Wandelt Start und Ende in einen Zeitraum um. Ohne Startdatum gibt es keinen Zeitraum, ohne Ende ist es offen.</summary>
        private static TimelineSpan? ToSpan(DateTime start, DateTime end) {
            if (start.Year <= 1) {
                return null;
            }
            return new TimelineSpan(start, end.Year > 1 && end >= start ? end : null);
        }

        private static TimelineSeason? ToSeason(int number, DateTime start, DateTime end) {
            TimelineSpan? span = ToSpan(start, end);
            return span is null ? null : new TimelineSeason(number, span);
        }

        public byte[]? SelectThumbnail(int animeId) {
            try {
                using (DatabaseSession session = this._databaseManager.BeginSession()) {
                    return this._imageService.SelectThumbnail(session, animeId);
                }
            }
            catch (Exception ex) {
                Log.Error(ex, "Die Vorschau des Animes (Id: {AnimeId}) konnte nicht geladen werden.", animeId);
                return null;
            }
        }

        /// <summary>Wandelt eine kommagetrennte Id-Liste (Ergebnis von GROUP_CONCAT) in Zahlen um.</summary>
        private static List<int> ParseIds(string? ids) {
            List<int> result = [];
            if (string.IsNullOrEmpty(ids)) {
                return result;
            }
            foreach (string part in ids.Split(',')) {
                if (int.TryParse(part, out int id)) {
                    result.Add(id);
                }
            }
            return result;
        }

        /// <summary>Speichert alles, was zum Anime gehört. Setzt dabei die Ids der Kind-Objekte im Modell.</summary>
        private void SaveRelatedData(DatabaseSession session, Anime anime) {
            this._statusService.Save(session, anime.Id, anime.Status);
            this._ratingService.Save(session, anime.Id, anime.Rating);
            this._mediaInfoService.Save(session, anime.Id, anime.MediaInfo);
            this._seasonService.ReplaceForAnime(session, anime.Id, anime.Seasons);
            this._videoAnimationService.ReplaceForAnime(session, anime.Id, anime.Ovas);
            this._tagService.ReplaceForAnime(session, anime.Id, anime.Tags);
            this._imageService.Save(session, anime.Id, anime.Image);
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
