using Modells.Anime;

namespace Anicluster.TasteProfile {

    /// <summary>
    /// Berechnet das Geschmacksprofil: Welche Tags kommen bei den Lieblings-Animes auffällig häufig oder selten vor,
    /// und welche Animes der Wunschliste passen deshalb besonders gut? Reines C# ohne WPF-Abhängigkeit, damit es testbar bleibt.
    /// </summary>
    /// <remarks>
    /// Grundlage ist der Vergleich zweier Anteile: Kommt ein Tag bei 60 % der Lieblinge vor, aber nur bei 30 % aller gesehenen Animes,
    /// ist sein Lift 2. Damit wenige Treffer nicht überbewertet werden, ist der Lift geglättet: Zur beobachteten und zur
    /// erwarteten Anzahl wird jeweils <see cref="Smoothing"/> addiert. Wunschlisten-Animes werden über den Mittelwert der
    /// Tag-Gewichte (Logarithmus des Lifts) bewertet.
    /// </remarks>
    public static class TasteProfileCalculator {

        /// <summary>Mindestanzahl Lieblinge, ab der ein Profil berechnet wird</summary>
        public const int MinLikedAnimes = 3;

        private const int MinPreferredSupport = 2;
        private const int MinAvoidedSupport = 3;
        private const double PreferredLift = 1.2;
        private const double AvoidedLift = 0.6;
        private const double Smoothing = 0.5;
        private const double WeightLimit = 1.5;
        private const int MaxPreferred = 15;
        private const int MaxAvoided = 10;
        private const int MaxSuggestions = 100;
        private const int MaxMatchingTags = 4;

        /// <param name="entries">Alle Animes der Bibliothek</param>
        /// <param name="tagNames">Bezeichnungen der Tags nach Id (Tags ohne Namen werden ignoriert)</param>
        /// <param name="options">Was als Liebling zählt</param>
        public static TasteProfile Calculate(
            IReadOnlyList<AnimeListEntry> entries,
            IReadOnlyDictionary<int, string> tagNames,
            TasteOptions options
        ) {
            ArgumentNullException.ThrowIfNull(entries);
            ArgumentNullException.ThrowIfNull(tagNames);
            ArgumentNullException.ThrowIfNull(options);

            // Wunschlisten-Animes sind noch nicht gesehen und sagen nichts über den Geschmack aus
            List<AnimeListEntry> watched = entries.Where(x => x.State != State.Wishlist).ToList();
            List<AnimeListEntry> liked = watched.Where(x => IsLiked(x, options)).ToList();
            if (liked.Count < MinLikedAnimes) {
                return new TasteProfile(liked.Count, watched.Count, false, [], [], []);
            }

            Dictionary<int, TagTaste> tastes = CalculateTagTastes(watched, liked, tagNames);
            List<TagTaste> preferred = tastes.Values
                .Where(x => x.LikedCount >= MinPreferredSupport && x.Lift >= PreferredLift)
                .OrderByDescending(x => x.Weight)
                .ThenByDescending(x => x.LikedCount)
                .ThenBy(x => x.Name, StringComparer.CurrentCultureIgnoreCase)
                .Take(MaxPreferred)
                .ToList();
            List<TagTaste> avoided = tastes.Values
                .Where(x => x.TotalCount >= MinAvoidedSupport && x.Lift <= AvoidedLift)
                .OrderBy(x => x.Weight)
                .ThenByDescending(x => x.TotalCount)
                .ThenBy(x => x.Name, StringComparer.CurrentCultureIgnoreCase)
                .Take(MaxAvoided)
                .ToList();
            List<TasteSuggestion> suggestions = RateAnimes(entries, tastes, options.SuggestionState);
            return new TasteProfile(liked.Count, watched.Count, true, preferred, avoided, suggestions);
        }

        private static bool IsLiked(AnimeListEntry entry, TasteOptions options) {
            return options.LikedTiers.Contains(entry.Tier) || (options.IncludeFavorites && entry.IsFavorite);
        }

        /// <summary>Wertet jeden Tag aus, der bei mindestens einem gesehenen Anime vorkommt.</summary>
        private static Dictionary<int, TagTaste> CalculateTagTastes(
            List<AnimeListEntry> watched,
            List<AnimeListEntry> liked,
            IReadOnlyDictionary<int, string> tagNames
        ) {
            Dictionary<int, int> totalCounts = CountTags(watched);
            Dictionary<int, int> likedCounts = CountTags(liked);
            double likedRatio = (double)liked.Count / watched.Count;

            Dictionary<int, TagTaste> tastes = [];
            foreach ((int tagId, int totalCount) in totalCounts) {
                if (!tagNames.TryGetValue(tagId, out string? name)) {
                    continue;
                }
                int likedCount = likedCounts.GetValueOrDefault(tagId);
                double expected = totalCount * likedRatio;
                double lift = (likedCount + Smoothing) / (expected + Smoothing);
                tastes[tagId] = new TagTaste(
                    tagId,
                    name,
                    likedCount,
                    totalCount,
                    (double)likedCount / liked.Count,
                    (double)totalCount / watched.Count,
                    lift
                );
            }
            return tastes;
        }

        private static Dictionary<int, int> CountTags(List<AnimeListEntry> animes) {
            Dictionary<int, int> counts = [];
            foreach (AnimeListEntry anime in animes) {
                foreach (int tagId in anime.TagIds) {
                    counts[tagId] = counts.GetValueOrDefault(tagId) + 1;
                }
            }
            return counts;
        }

        /// <summary>
        /// Bewertet die Animes mit dem gewünschten Status (ohne Angabe alle), meist die Wunschliste. Tags, die zu selten vorkommen, um etwas auszusagen, zählen neutral (0).
        /// Nur Animes mit positiver Passung werden geliefert.
        /// </summary>
        private static List<TasteSuggestion> RateAnimes(
            IReadOnlyList<AnimeListEntry> entries, Dictionary<int, TagTaste> tastes, State? state) {
            List<(AnimeListEntry Entry, double Score, List<TagTaste> Matching)> rated = [];
            foreach (AnimeListEntry entry in entries.Where(x => (state is null || x.State == state) && x.TagIds.Count > 0)) {
                double sum = 0;
                List<TagTaste> matching = [];
                foreach (int tagId in entry.TagIds) {
                    if (!tastes.TryGetValue(tagId, out TagTaste? taste) || taste.TotalCount < MinPreferredSupport) {
                        continue;
                    }
                    double weight = Math.Clamp(taste.Weight, -WeightLimit, WeightLimit);
                    sum += weight;
                    if (weight > 0) {
                        matching.Add(taste);
                    }
                }
                double score = sum / entry.TagIds.Count;
                if (score > 0) {
                    rated.Add((entry, score, matching));
                }
            }
            if (rated.Count == 0) {
                return [];
            }

            double best = rated.Max(x => x.Score);
            return rated
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.Entry.Name, StringComparer.CurrentCultureIgnoreCase)
                .Take(MaxSuggestions)
                .Select(x => new TasteSuggestion(
                    x.Entry,
                    (int)Math.Round(x.Score / best * 100),
                    x.Matching.OrderByDescending(t => t.Weight).Take(MaxMatchingTags).ToList()
                ))
                .ToList();
        }
    }
}
