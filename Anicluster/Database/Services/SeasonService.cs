using Modells.Anime;

namespace Anicluster.Database.Services {

    /// <summary>
    /// Liest und schreibt die Staffeln eines Animes samt Veröffentlichungszeitraum und Episoden.
    /// </summary>
    internal class SeasonService {

        private readonly PublishingTimeService _publishingTimeService = new PublishingTimeService();
        private readonly VideoAnimationService _videoAnimationService = new VideoAnimationService();

        /// <summary>Lädt die Staffeln des Animes sortiert nach Nummer.</summary>
        public List<Season> SelectByAnimeId(DatabaseSession session, int animeId) {
            List<Season> seasons = session.Query(
                "SELECT Id, Number, Comment FROM Season WHERE AnimeId = @AnimeId ORDER BY Number, Id;",
                reader => new Season(reader.GetInt(0), reader.GetInt(1), reader.GetStringOrNull(2)),
                ("@AnimeId", animeId)
            );
            foreach (Season season in seasons) {
                season.PublishingTime = this._publishingTimeService.SelectBySeasonId(session, season.Id);
                season.Episodes = this._videoAnimationService.SelectBySeasonId(session, season.Id);
            }
            return seasons;
        }

        /// <summary>
        /// Ersetzt alle Staffeln des Animes durch die übergebenen. Das Löschen entfernt über die Fremdschlüssel
        /// auch Zeiträume, Unterbrechungen und Episoden der alten Staffeln. Die neuen Ids werden im Modell gesetzt.
        /// </summary>
        public void ReplaceForAnime(DatabaseSession session, int animeId, List<Season> seasons) {
            session.Execute("DELETE FROM Season WHERE AnimeId = @AnimeId;", ("@AnimeId", animeId));
            foreach (Season season in seasons) {
                season.Id = session.Insert(
                    "INSERT INTO Season (AnimeId, Number, Comment) VALUES (@AnimeId, @Number, @Comment);",
                    ("@AnimeId", animeId), ("@Number", season.Number), ("@Comment", season.Comment)
                );
                this._publishingTimeService.SaveForSeason(session, season.Id, season.PublishingTime);
                this._videoAnimationService.InsertForSeason(session, season.Id, season.Episodes);
            }
        }
    }
}
