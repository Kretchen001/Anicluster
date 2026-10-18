using Modells.Anime;

namespace Anicluster.Database.Services {

    /// <summary>
    /// Liest und schreibt Veröffentlichungszeiträume samt Unterbrechungen.
    /// Ein Zeitraum gehört entweder zu einer Medieninfo oder zu einer Staffel.
    /// </summary>
    internal class PublishingTimeService {

        private const string MediaInfoColumn = "MediaInfoId";
        private const string SeasonColumn = "SeasonId";

        public PublishingTime SelectByMediaInfoId(DatabaseSession session, int mediaInfoId) {
            return this.SelectByOwner(session, MediaInfoColumn, mediaInfoId);
        }

        public PublishingTime SelectBySeasonId(DatabaseSession session, int seasonId) {
            return this.SelectByOwner(session, SeasonColumn, seasonId);
        }

        public void SaveForMediaInfo(DatabaseSession session, int mediaInfoId, PublishingTime publishingTime) {
            this.Save(session, MediaInfoColumn, mediaInfoId, publishingTime);
        }

        public void SaveForSeason(DatabaseSession session, int seasonId, PublishingTime publishingTime) {
            this.Save(session, SeasonColumn, seasonId, publishingTime);
        }

        /// <summary>Lädt den Zeitraum des Besitzers, ohne Eintrag wird ein leerer Zeitraum geliefert.</summary>
        private PublishingTime SelectByOwner(DatabaseSession session, string ownerColumn, int ownerId) {
            PublishingTime? publishingTime = session.Query(
                $"SELECT Id, StartDate, EndDate FROM PublishingTime WHERE {ownerColumn} = @OwnerId;",
                reader => new PublishingTime() {
                    Id = reader.GetInt(0),
                    StartDate = reader.GetDate(1),
                    EndDate = reader.GetDate(2)
                },
                ("@OwnerId", ownerId)
            ).FirstOrDefault();
            if (publishingTime is null) {
                return new PublishingTime();
            }
            publishingTime.Interruptions = session.Query(
                "SELECT Id, StartDate, EndDate, Comment FROM Interruption WHERE PublishingTimeId = @PublishingTimeId ORDER BY StartDate, Id;",
                reader => new Interruption(reader.GetDate(1), reader.GetDate(2), reader.GetStringOrNull(3)) { Id = reader.GetInt(0) },
                ("@PublishingTimeId", publishingTime.Id)
            );
            return publishingTime;
        }

        /// <summary>
        /// Legt den Zeitraum des Besitzers an oder aktualisiert ihn und ersetzt dessen Unterbrechungen.
        /// Die Spaltennamen kommen ausschließlich aus den Konstanten dieser Klasse.
        /// </summary>
        private void Save(DatabaseSession session, string ownerColumn, int ownerId, PublishingTime publishingTime) {
            long? existingId = session.ScalarLong($"SELECT Id FROM PublishingTime WHERE {ownerColumn} = @OwnerId;", ("@OwnerId", ownerId));
            if (existingId is null) {
                publishingTime.Id = session.Insert(
                    $"INSERT INTO PublishingTime ({ownerColumn}, StartDate, EndDate) VALUES (@OwnerId, @StartDate, @EndDate);",
                    ("@OwnerId", ownerId), ("@StartDate", publishingTime.StartDate), ("@EndDate", publishingTime.EndDate)
                );
            }
            else {
                session.Execute(
                    "UPDATE PublishingTime SET StartDate = @StartDate, EndDate = @EndDate WHERE Id = @Id;",
                    ("@StartDate", publishingTime.StartDate), ("@EndDate", publishingTime.EndDate), ("@Id", (int)existingId)
                );
                publishingTime.Id = (int)existingId;
            }
            session.Execute("DELETE FROM Interruption WHERE PublishingTimeId = @PublishingTimeId;", ("@PublishingTimeId", publishingTime.Id));
            foreach (Interruption interruption in publishingTime.Interruptions) {
                interruption.Id = session.Insert(
                    "INSERT INTO Interruption (PublishingTimeId, StartDate, EndDate, Comment) VALUES (@PublishingTimeId, @StartDate, @EndDate, @Comment);",
                    ("@PublishingTimeId", publishingTime.Id), ("@StartDate", interruption.Start), ("@EndDate", interruption.End), ("@Comment", interruption.Comment)
                );
            }
        }
    }
}
