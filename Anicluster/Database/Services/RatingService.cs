using Modells.Anime;

namespace Anicluster.Database.Services {

    /// <summary>
    /// Liest und schreibt die Bewertung eines Animes (genau ein Eintrag pro Anime) samt Akustik.
    /// </summary>
    internal class RatingService {

        private readonly AcousticService _acousticService = new AcousticService();

        /// <summary>Lädt die Bewertung, ohne Eintrag wird eine unbewertete Standard-Bewertung geliefert.</summary>
        public Rating SelectByAnimeId(DatabaseSession session, int animeId) {
            Rating? rating = session.Query(
                "SELECT Id, Story, Animation, SpecialEffects, IsRated FROM Rating WHERE AnimeId = @AnimeId;",
                reader => new Rating() {
                    Id = reader.GetInt(0),
                    Story = reader.GetInt(1),
                    Animation = reader.GetInt(2),
                    SpecialEffects = reader.GetInt(3),
                    IsRated = reader.GetBool(4)
                },
                ("@AnimeId", animeId)
            ).FirstOrDefault();
            if (rating is null) {
                return new Rating();
            }
            rating.Acoustic = this._acousticService.SelectByRatingId(session, rating.Id);
            return rating;
        }

        /// <summary>Legt die Bewertung an oder aktualisiert sie, speichert die Akustik mit und setzt die Ids im Modell.</summary>
        public void Save(DatabaseSession session, int animeId, Rating rating) {
            long? existingId = session.ScalarLong("SELECT Id FROM Rating WHERE AnimeId = @AnimeId;", ("@AnimeId", animeId));
            if (existingId is null) {
                rating.Id = session.Insert(
                    "INSERT INTO Rating (AnimeId, Story, Animation, SpecialEffects, IsRated) VALUES (@AnimeId, @Story, @Animation, @SpecialEffects, @IsRated);",
                    ("@AnimeId", animeId), ("@Story", rating.Story), ("@Animation", rating.Animation), ("@SpecialEffects", rating.SpecialEffects), ("@IsRated", rating.IsRated)
                );
            }
            else {
                session.Execute(
                    "UPDATE Rating SET Story = @Story, Animation = @Animation, SpecialEffects = @SpecialEffects, IsRated = @IsRated WHERE Id = @Id;",
                    ("@Story", rating.Story), ("@Animation", rating.Animation), ("@SpecialEffects", rating.SpecialEffects), ("@IsRated", rating.IsRated), ("@Id", (int)existingId)
                );
                rating.Id = (int)existingId;
            }
            this._acousticService.Save(session, rating.Id, rating.Acoustic);
        }
    }
}
