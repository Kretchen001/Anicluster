using Modells.Anime;

namespace Anicluster.Database.Services {

    /// <summary>
    /// Liest und schreibt die Medieninfos eines Animes (genau ein Eintrag pro Anime) samt Veröffentlichungszeitraum.
    /// </summary>
    internal class MediaInfoService {

        private readonly PublishingTimeService _publishingTimeService = new PublishingTimeService();

        /// <summary>Lädt die Medieninfos, ohne Eintrag werden leere Medieninfos geliefert.</summary>
        public MediaInfo SelectByAnimeId(DatabaseSession session, int animeId) {
            MediaInfo? mediaInfo = session.Query(
                "SELECT Id, Author, Producer, Publisher FROM MediaInfo WHERE AnimeId = @AnimeId;",
                reader => new MediaInfo() {
                    Id = reader.GetInt(0),
                    Author = reader.GetStringOrNull(1),
                    Producer = reader.GetStringOrNull(2),
                    Publisher = reader.GetStringOrNull(3)
                },
                ("@AnimeId", animeId)
            ).FirstOrDefault();
            if (mediaInfo is null) {
                return new MediaInfo();
            }
            mediaInfo.PublishingTime = this._publishingTimeService.SelectByMediaInfoId(session, mediaInfo.Id);
            return mediaInfo;
        }

        /// <summary>Legt die Medieninfos an oder aktualisiert sie, speichert den Zeitraum mit und setzt die Ids im Modell.</summary>
        public void Save(DatabaseSession session, int animeId, MediaInfo mediaInfo) {
            long? existingId = session.ScalarLong("SELECT Id FROM MediaInfo WHERE AnimeId = @AnimeId;", ("@AnimeId", animeId));
            if (existingId is null) {
                mediaInfo.Id = session.Insert(
                    "INSERT INTO MediaInfo (AnimeId, Author, Producer, Publisher) VALUES (@AnimeId, @Author, @Producer, @Publisher);",
                    ("@AnimeId", animeId), ("@Author", mediaInfo.Author), ("@Producer", mediaInfo.Producer), ("@Publisher", mediaInfo.Publisher)
                );
            }
            else {
                session.Execute(
                    "UPDATE MediaInfo SET Author = @Author, Producer = @Producer, Publisher = @Publisher WHERE Id = @Id;",
                    ("@Author", mediaInfo.Author), ("@Producer", mediaInfo.Producer), ("@Publisher", mediaInfo.Publisher), ("@Id", (int)existingId)
                );
                mediaInfo.Id = (int)existingId;
            }
            this._publishingTimeService.SaveForMediaInfo(session, mediaInfo.Id, mediaInfo.PublishingTime);
        }
    }
}
