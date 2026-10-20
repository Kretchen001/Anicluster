using Modells.Anime.SoundRating;

namespace Anicluster.Database.Services {

    /// <summary>
    /// Liest und schreibt die Akustik-Bewertung (genau ein Eintrag pro Bewertung) samt Musikstücken und Syncros.
    /// </summary>
    internal class AcousticService {

        private readonly MusicPieceService _musicPieceService = new MusicPieceService();
        private readonly SyncroService _syncroService = new SyncroService();

        /// <summary>Lädt die Akustik, ohne Eintrag wird eine leere Akustik geliefert.</summary>
        public Acoustic SelectByRatingId(DatabaseSession session, int ratingId) {
            Acoustic? acoustic = session.Query(
                "SELECT Id, Soundtrack, Comment FROM Acoustic WHERE RatingId = @RatingId;",
                reader => new Acoustic() {
                    Id = reader.GetInt(0),
                    Soundtrack = reader.GetInt(1),
                    Comment = reader.GetStringOrNull(2)
                },
                ("@RatingId", ratingId)
            ).FirstOrDefault();
            if (acoustic is null) {
                return new Acoustic();
            }
            List<MusicPiece> pieces = this._musicPieceService.SelectByAcousticId(session, acoustic.Id);
            acoustic.Opening = pieces.Where(x => x.Type == MusicPieceType.Opening).ToList();
            acoustic.Ending = pieces.Where(x => x.Type == MusicPieceType.Ending).ToList();
            acoustic.Sounds = pieces.Where(x => x.Type == MusicPieceType.Other).ToList();
            acoustic.Syncro = this._syncroService.SelectByAcousticId(session, acoustic.Id);
            return acoustic;
        }

        /// <summary>Legt die Akustik an oder aktualisiert sie, ersetzt Musikstücke und Syncros und setzt die Ids im Modell.</summary>
        public void Save(DatabaseSession session, int ratingId, Acoustic acoustic) {
            long? existingId = session.ScalarLong("SELECT Id FROM Acoustic WHERE RatingId = @RatingId;", ("@RatingId", ratingId));
            if (existingId is null) {
                acoustic.Id = session.Insert(
                    "INSERT INTO Acoustic (RatingId, Soundtrack, Comment) VALUES (@RatingId, @Soundtrack, @Comment);",
                    ("@RatingId", ratingId), ("@Soundtrack", acoustic.Soundtrack), ("@Comment", acoustic.Comment)
                );
            }
            else {
                session.Execute(
                    "UPDATE Acoustic SET Soundtrack = @Soundtrack, Comment = @Comment WHERE Id = @Id;",
                    ("@Soundtrack", acoustic.Soundtrack), ("@Comment", acoustic.Comment), ("@Id", (int)existingId)
                );
                acoustic.Id = (int)existingId;
            }
            this._musicPieceService.ReplaceForAcoustic(session, acoustic.Id, acoustic);
            this._syncroService.ReplaceForAcoustic(session, acoustic.Id, acoustic.Syncro);
        }
    }
}
