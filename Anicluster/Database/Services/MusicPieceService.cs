using Modells.Anime.SoundRating;

namespace Anicluster.Database.Services {

    /// <summary>
    /// Liest und schreibt die Musikstücke (Opening, Ending, sonstiger Soundtrack) einer Akustik.
    /// </summary>
    internal class MusicPieceService {

        public List<MusicPiece> SelectByAcousticId(DatabaseSession session, int acousticId) {
            return session.Query(
                "SELECT Id, Type, Name, Comment, General FROM MusicPiece WHERE AcousticId = @AcousticId ORDER BY Id;",
                reader => new MusicPiece(reader.GetInt(0), (MusicPieceType)reader.GetInt(1), reader.GetStringOrNull(2), reader.GetStringOrNull(3), reader.GetInt(4)),
                ("@AcousticId", acousticId)
            );
        }

        /// <summary>
        /// Ersetzt alle Musikstücke der Akustik durch die des Modells. Der Typ ergibt sich aus der jeweiligen Liste.
        /// </summary>
        public void ReplaceForAcoustic(DatabaseSession session, int acousticId, Acoustic acoustic) {
            session.Execute("DELETE FROM MusicPiece WHERE AcousticId = @AcousticId;", ("@AcousticId", acousticId));
            this.InsertAll(session, acousticId, acoustic.Opening, MusicPieceType.Opening);
            this.InsertAll(session, acousticId, acoustic.Ending, MusicPieceType.Ending);
            this.InsertAll(session, acousticId, acoustic.Sounds, MusicPieceType.Other);
        }

        private void InsertAll(DatabaseSession session, int acousticId, List<MusicPiece> pieces, MusicPieceType type) {
            foreach (MusicPiece piece in pieces) {
                piece.Type = type;
                piece.Id = session.Insert(
                    "INSERT INTO MusicPiece (AcousticId, Type, Name, Comment, General) VALUES (@AcousticId, @Type, @Name, @Comment, @General);",
                    ("@AcousticId", acousticId), ("@Type", piece.Type), ("@Name", piece.Name), ("@Comment", piece.Comment), ("@General", piece.General)
                );
            }
        }
    }
}
