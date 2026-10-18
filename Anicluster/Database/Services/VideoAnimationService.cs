using Modells.Anime;

namespace Anicluster.Database.Services {

    /// <summary>
    /// Liest und schreibt Videos (Episoden, OVAs, Filme). Ein Video gehört entweder zu einer Staffel (Episode)
    /// oder direkt zu einem Anime (OVA, Film, ...). Die Reihenfolge der Liste bleibt über die Spalte Position erhalten.
    /// </summary>
    internal class VideoAnimationService {

        private const string AnimeColumn = "AnimeId";
        private const string SeasonColumn = "SeasonId";

        public List<VideoAnimation> SelectByAnimeId(DatabaseSession session, int animeId) {
            return this.SelectByOwner(session, AnimeColumn, animeId);
        }

        public List<VideoAnimation> SelectBySeasonId(DatabaseSession session, int seasonId) {
            return this.SelectByOwner(session, SeasonColumn, seasonId);
        }

        /// <summary>Ersetzt alle OVAs/Filme des Animes durch die übergebenen.</summary>
        public void ReplaceForAnime(DatabaseSession session, int animeId, List<VideoAnimation> videos) {
            session.Execute("DELETE FROM VideoAnimation WHERE AnimeId = @AnimeId;", ("@AnimeId", animeId));
            this.InsertAll(session, AnimeColumn, animeId, videos);
        }

        /// <summary>Legt die Episoden einer neu angelegten Staffel an.</summary>
        public void InsertForSeason(DatabaseSession session, int seasonId, List<VideoAnimation> videos) {
            this.InsertAll(session, SeasonColumn, seasonId, videos);
        }

        private List<VideoAnimation> SelectByOwner(DatabaseSession session, string ownerColumn, int ownerId) {
            return session.Query(
                $"SELECT Id, VideoType, Comment FROM VideoAnimation WHERE {ownerColumn} = @OwnerId ORDER BY Position, Id;",
                reader => new VideoAnimation(reader.GetInt(0), (VideoType)reader.GetInt(1), reader.GetStringOrNull(2)),
                ("@OwnerId", ownerId)
            );
        }

        /// <summary>Fügt die Videos in Listenreihenfolge ein. Die Spaltennamen kommen ausschließlich aus den Konstanten dieser Klasse.</summary>
        private void InsertAll(DatabaseSession session, string ownerColumn, int ownerId, List<VideoAnimation> videos) {
            int position = 1;
            foreach (VideoAnimation video in videos) {
                video.Id = session.Insert(
                    $"INSERT INTO VideoAnimation ({ownerColumn}, Position, VideoType, Comment) VALUES (@OwnerId, @Position, @VideoType, @Comment);",
                    ("@OwnerId", ownerId), ("@Position", position), ("@VideoType", video.VideoType), ("@Comment", video.Comment)
                );
                position += 1;
            }
        }
    }
}
