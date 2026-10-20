using Modells.Anime;

namespace Anicluster.Database.Services {

    /// <summary>
    /// Liest und schreibt den Status eines Animes (genau ein Eintrag pro Anime).
    /// </summary>
    internal class StatusService {

        /// <summary>Lädt den Status, ohne Eintrag wird ein Standard-Status geliefert.</summary>
        public Status SelectByAnimeId(DatabaseSession session, int animeId) {
            Status? status = session.Query(
                "SELECT Id, State, Comment FROM Status WHERE AnimeId = @AnimeId;",
                reader => new Status(reader.GetInt(0), (State)reader.GetInt(1), reader.GetStringOrNull(2)),
                ("@AnimeId", animeId)
            ).FirstOrDefault();
            return status ?? new Status();
        }

        /// <summary>Legt den Status an oder aktualisiert ihn und setzt die Id im Modell.</summary>
        public void Save(DatabaseSession session, int animeId, Status status) {
            long? existingId = session.ScalarLong("SELECT Id FROM Status WHERE AnimeId = @AnimeId;", ("@AnimeId", animeId));
            if (existingId is null) {
                status.Id = session.Insert(
                    "INSERT INTO Status (AnimeId, State, Comment) VALUES (@AnimeId, @State, @Comment);",
                    ("@AnimeId", animeId), ("@State", status.State), ("@Comment", status.Comment)
                );
            }
            else {
                session.Execute(
                    "UPDATE Status SET State = @State, Comment = @Comment WHERE Id = @Id;",
                    ("@State", status.State), ("@Comment", status.Comment), ("@Id", (int)existingId)
                );
                status.Id = (int)existingId;
            }
        }
    }
}
