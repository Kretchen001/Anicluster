using Modells.Anime.SoundRating;

namespace Anicluster.Database.Services {

    /// <summary>
    /// Liest und schreibt die Syncros einer Akustik.
    /// </summary>
    internal class SyncroService {

        public List<Syncro> SelectByAcousticId(DatabaseSession session, int acousticId) {
            return session.Query(
                "SELECT General, Language, IsAssessed, Comment FROM Syncro WHERE AcousticId = @AcousticId ORDER BY Id;",
                reader => new Syncro(reader.GetInt(0), (SyncroLanuage)reader.GetInt(1), reader.GetBool(2), reader.GetStringOrNull(3)),
                ("@AcousticId", acousticId)
            );
        }

        /// <summary>Ersetzt alle Syncros der Akustik durch die übergebenen.</summary>
        public void ReplaceForAcoustic(DatabaseSession session, int acousticId, List<Syncro> syncros) {
            session.Execute("DELETE FROM Syncro WHERE AcousticId = @AcousticId;", ("@AcousticId", acousticId));
            foreach (Syncro syncro in syncros) {
                session.Execute(
                    "INSERT INTO Syncro (AcousticId, General, Language, IsAssessed, Comment) VALUES (@AcousticId, @General, @Language, @IsAssessed, @Comment);",
                    ("@AcousticId", acousticId), ("@General", syncro.General), ("@Language", syncro.Language), ("@IsAssessed", syncro.IsAssessed), ("@Comment", syncro.Comment)
                );
            }
        }
    }
}
