namespace Anicluster.AnimeNetwork {

    /// <summary>Erzeugt die kurze Beschriftung eines Clusters aus seinen Mitgliedern.</summary>
    public static class ClusterLabeler {

        /// <summary>Ein Tag taucht in der Beschriftung auf, wenn mindestens dieser Anteil der Mitglieder ihn trägt (und mindestens zwei).</summary>
        private const double MinShare = 0.4;

        /// <summary>
        /// Die häufigsten Tags der Mitglieder, getrennt durch " · ". Leer, wenn kein Tag bei mindestens zwei Mitgliedern
        /// und einem ausreichenden Anteil vorkommt.
        /// </summary>
        public static string TopTags(
            IReadOnlyList<IReadOnlyCollection<int>> memberTagIds,
            IReadOnlyDictionary<int, string> tagNames,
            int maxCount
        ) {
            Dictionary<int, int> frequency = [];
            foreach (IReadOnlyCollection<int> tagIds in memberTagIds) {
                foreach (int tagId in tagIds.Distinct()) {
                    frequency[tagId] = frequency.GetValueOrDefault(tagId) + 1;
                }
            }
            double minCount = Math.Max(2.0, memberTagIds.Count * MinShare);
            List<string> names = frequency
                .Where(x => x.Value >= minCount && tagNames.ContainsKey(x.Key))
                .OrderByDescending(x => x.Value)
                .ThenBy(x => tagNames[x.Key], StringComparer.CurrentCultureIgnoreCase)
                .Take(maxCount)
                .Select(x => tagNames[x.Key])
                .ToList();
            return string.Join(" · ", names);
        }

        /// <summary>Durchschnittliche Gesamtnote der Mitglieder als Text ("Ø 82"), leer ohne Noten.</summary>
        public static string AverageRating(IReadOnlyList<int> generalRatings) {
            return generalRatings.Count == 0 ? string.Empty : $"Ø {Math.Round(generalRatings.Average())}";
        }
    }
}
