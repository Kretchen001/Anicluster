using Modells.Anime;
using Modells.Anime.SoundRating;

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

        /// <summary>
        /// Lädt die Teilnoten und die Gesamtnote aller bewerteten Animes in vier Abfragen (statt je Anime einzeln).
        /// Die Akustik wird aus Musikstücken und Syncros zusammengesetzt und vom Modell berechnet, die Formel steht nur in <see cref="Acoustic"/>.
        /// </summary>
        public List<AnimeRatingEntry> SelectEntries(DatabaseSession session) {
            List<RatedAnime> rated = session.Query(
                @"SELECT r.AnimeId, r.Story, r.Animation, r.SpecialEffects, COALESCE(ac.Id, 0), COALESCE(ac.Soundtrack, -1)
                    FROM Rating r
                    LEFT JOIN Acoustic ac ON ac.RatingId = r.Id
                    WHERE r.IsRated = 1
                    ORDER BY r.AnimeId;",
                reader => new RatedAnime(reader.GetInt(0), reader.GetInt(1), reader.GetInt(2), reader.GetInt(3), reader.GetInt(4), reader.GetInt(5))
            );

            Dictionary<int, Acoustic> acoustics = [];
            foreach (RatedAnime item in rated) {
                if (item.AcousticId > 0) {
                    acoustics[item.AcousticId] = new Acoustic { Id = item.AcousticId, Soundtrack = item.Soundtrack };
                }
            }

            session.Query(
                @"SELECT mp.AcousticId, mp.Type, mp.General
                    FROM MusicPiece mp
                    JOIN Acoustic ac ON ac.Id = mp.AcousticId
                    JOIN Rating r ON r.Id = ac.RatingId
                    WHERE r.IsRated = 1
                    ORDER BY mp.Id;",
                reader => (AcousticId: reader.GetInt(0), PieceType: (MusicPieceType)reader.GetInt(1), General: reader.GetInt(2))
            ).ForEach(x => {
                if (!acoustics.TryGetValue(x.AcousticId, out Acoustic? target)) {
                    return;
                }
                MusicPiece piece = new MusicPiece { Type = x.PieceType, General = x.General };
                switch (x.PieceType) {
                    case MusicPieceType.Opening:
                        target.Opening.Add(piece);
                        break;
                    case MusicPieceType.Ending:
                        target.Ending.Add(piece);
                        break;
                    default:
                        target.Sounds.Add(piece);
                        break;
                }
            });

            session.Query(
                @"SELECT sy.AcousticId, sy.General, sy.IsAssessed
                    FROM Syncro sy
                    JOIN Acoustic ac ON ac.Id = sy.AcousticId
                    JOIN Rating r ON r.Id = ac.RatingId
                    WHERE r.IsRated = 1
                    ORDER BY sy.Id;",
                reader => (AcousticId: reader.GetInt(0), General: reader.GetInt(1), IsAssessed: reader.GetBool(2))
            ).ForEach(x => {
                if (acoustics.TryGetValue(x.AcousticId, out Acoustic? target)) {
                    target.Syncro.Add(new Syncro { General = x.General, IsAssessed = x.IsAssessed });
                }
            });

            List<AnimeRatingEntry> entries = [];
            foreach (RatedAnime item in rated) {
                Acoustic acoustic = acoustics.GetValueOrDefault(item.AcousticId) ?? new Acoustic();
                int acousticGeneral = acoustic.General;
                entries.Add(new AnimeRatingEntry(
                    item.AnimeId,
                    item.Story,
                    item.Animation,
                    item.SpecialEffects,
                    acousticGeneral,
                    Rating.CalcGeneral(item.Story, item.Animation, item.SpecialEffects, acousticGeneral)
                ));
            }
            return entries;
        }

        /// <summary>Eine Zeile der ersten Abfrage von <see cref="SelectEntries"/>: Teilnoten und die Akustik eines bewerteten Animes.</summary>
        private record RatedAnime(int AnimeId, int Story, int Animation, int SpecialEffects, int AcousticId, int Soundtrack);

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
