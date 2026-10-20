using Modells.Anime;

namespace Anicluster.Database.Services {

    /// <summary>
    /// Liest und schreibt das Bild eines Animes (höchstens ein Eintrag pro Anime).
    /// </summary>
    internal class ImageService {

        /// <summary>Lädt das Bild in beiden Größen, <c>null</c> wenn der Anime keins hat.</summary>
        public AnimeImage? SelectByAnimeId(DatabaseSession session, int animeId) {
            return session.Query(
                "SELECT Data, Thumbnail FROM AnimeImage WHERE AnimeId = @AnimeId;",
                reader => new AnimeImage() {
                    Data = reader.GetBlob(0),
                    Thumbnail = reader.GetBlob(1)
                },
                ("@AnimeId", animeId)
            ).FirstOrDefault();
        }

        /// <summary>Lädt nur die kleine Vorschau, <c>null</c> wenn der Anime kein Bild hat.</summary>
        public byte[]? SelectThumbnail(DatabaseSession session, int animeId) {
            return session.Query(
                "SELECT Thumbnail FROM AnimeImage WHERE AnimeId = @AnimeId;",
                reader => reader.GetBlob(0),
                ("@AnimeId", animeId)
            ).FirstOrDefault();
        }

        /// <summary>Speichert das Bild (legt es an oder ersetzt es). Ohne Bild wird ein vorhandenes gelöscht.</summary>
        public void Save(DatabaseSession session, int animeId, AnimeImage? image) {
            if (image is null) {
                session.Execute("DELETE FROM AnimeImage WHERE AnimeId = @AnimeId;", ("@AnimeId", animeId));
                return;
            }
            session.Execute(
                @"INSERT INTO AnimeImage (AnimeId, Data, Thumbnail) VALUES (@AnimeId, @Data, @Thumbnail)
                    ON CONFLICT (AnimeId) DO UPDATE SET Data = excluded.Data, Thumbnail = excluded.Thumbnail;",
                ("@AnimeId", animeId), ("@Data", image.Data), ("@Thumbnail", image.Thumbnail)
            );
        }
    }
}
