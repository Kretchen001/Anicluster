using Modells.Anime;

namespace Anicluster.AnimeNetwork {

    /// <summary>
    /// Berechnet die Verbindungen des Anime-Netzwerks, rein und ohne Oberfläche:
    /// <list type="bullet">
    /// <item><description>Beziehungen: Vorgänger, Nachfolger und Verwandt, so wie sie bei den Animes eingetragen sind.</description></item>
    /// <item><description>Tags: Ähnlichkeit ist der Jaccard-Wert der Tag-Mengen, wobei seltene Tags stärker zählen (IDF-Gewichtung).</description></item>
    /// <item><description>Bewertung: Ähnlichkeit aus dem Abstand der vier Teilnoten, gewichtet wie die Gesamtnote.</description></item>
    /// </list>
    /// Bei den Ähnlichkeitsarten behält jeder Anime nur seine stärksten Nachbarn, sonst entstünde ein undurchsichtiges Knäuel.
    /// </summary>
    public static class AnimeNetworkBuilder {

        /// <summary>Zwei Animes werden nur verbunden, wenn sie mindestens so viele Tags gemeinsam haben.</summary>
        public const int MinSharedTags = 2;

        private const double SuccessorWeight = 1.0;
        private const double RelatedWeight = 0.6;

        /// <summary>Größter möglicher Abstand zweier Bewertungen (Teilnoten von 0 bis 100, Gewichte ergeben zusammen 1).</summary>
        private const double MaxRatingDistance = 100.0;

        /// <summary>Gewichte von Story, Animation, Effekten und Akustik, wie in der Gesamtnote (40/25/10/25).</summary>
        private static readonly double[] RatingWeights = [0.40, 0.25, 0.10, 0.25];

        #region Beziehungen
        /// <summary>
        /// Verbindungen aus Vorgänger, Nachfolger und Verwandt. Ein Eintrag, der von beiden Seiten gesetzt ist, ergibt nur eine Verbindung.
        /// Verweise auf nicht vorhandene Animes werden ignoriert. Die Cluster sind die zusammenhängenden Gruppen.
        /// </summary>
        public static AnimeNetworkResult BuildRelations(IReadOnlyList<AnimeRelationEntry> entries) {
            HashSet<int> ids = entries.Select(x => x.Id).ToHashSet();
            Dictionary<(int, int), AnimeNetworkEdge> edges = [];
            foreach (AnimeRelationEntry entry in entries) {
                AddRelation(edges, ids, entry.Predecessor, entry.Id, AnimeEdgeKind.Successor);
                AddRelation(edges, ids, entry.Id, entry.Successor, AnimeEdgeKind.Successor);
                AddRelation(edges, ids, entry.Id, entry.Related, AnimeEdgeKind.Related);
            }
            List<AnimeNetworkEdge> edgeList = edges.Values.ToList();
            return new AnimeNetworkResult(edgeList, ClusterDetector.ConnectedComponents(edgeList), ids);
        }

        private static void AddRelation(
            Dictionary<(int, int), AnimeNetworkEdge> edges,
            HashSet<int> ids,
            int from,
            int to,
            AnimeEdgeKind kind
        ) {
            if (from == to || !ids.Contains(from) || !ids.Contains(to)) {
                return;
            }
            (int, int) key = from < to ? (from, to) : (to, from);
            double weight = kind == AnimeEdgeKind.Successor ? SuccessorWeight : RelatedWeight;
            if (edges.TryGetValue(key, out AnimeNetworkEdge? existing)) {
                // Vorgänger/Nachfolger hat Vorrang vor "Verwandt", bei Widerspruch gilt die zuerst gefundene Richtung
                if (existing.Kind == AnimeEdgeKind.Related && kind == AnimeEdgeKind.Successor) {
                    edges[key] = new AnimeNetworkEdge(from, to, weight, kind);
                }
                return;
            }
            edges[key] = new AnimeNetworkEdge(from, to, weight, kind);
        }
        #endregion

        #region Tags
        /// <summary>
        /// Verbindungen nach ähnlichen Tags. Animes ohne Tags nehmen nicht teil. Die Ähnlichkeit ist der Jaccard-Wert der Tag-Mengen,
        /// jeder Tag zählt mit seinem IDF-Gewicht (selten = wichtig). Verbunden wird ab <paramref name="minSimilarity"/> (0 bis 1)
        /// und mindestens <see cref="MinSharedTags"/> gemeinsamen Tags, jeder Anime behält höchstens <paramref name="neighborCount"/> stärkste Nachbarn.
        /// </summary>
        public static AnimeNetworkResult BuildTags(IReadOnlyList<AnimeListEntry> animes, double minSimilarity, int neighborCount) {
            List<AnimeListEntry> eligible = animes.Where(x => x.TagIds.Count > 0).ToList();
            int count = eligible.Count;
            int[][] tags = eligible.Select(x => x.TagIds.Distinct().ToArray()).ToArray();

            // IDF: Ein Tag, der bei fast allen Animes vorkommt, sagt wenig über Ähnlichkeit aus
            Dictionary<int, int> frequency = [];
            foreach (int[] animeTags in tags) {
                foreach (int tag in animeTags) {
                    frequency[tag] = frequency.GetValueOrDefault(tag) + 1;
                }
            }
            Dictionary<int, double> idf = [];
            foreach (KeyValuePair<int, int> pair in frequency) {
                idf[pair.Key] = Math.Log((count + 1.0) / (pair.Value + 1.0)) + 1.0;
            }
            double[] total = tags.Select(animeTags => animeTags.Sum(tag => idf[tag])).ToArray();

            // Zu jedem Tag die Animes, die ihn tragen (in aufsteigender Reihenfolge), so müssen nur Animes mit gemeinsamen Tags verglichen werden
            Dictionary<int, List<int>> postings = [];
            for (int index = 0; index < count; index += 1) {
                foreach (int tag in tags[index]) {
                    if (!postings.TryGetValue(tag, out List<int>? list)) {
                        list = [];
                        postings[tag] = list;
                    }
                    list.Add(index);
                }
            }

            List<(int A, int B, double Similarity)> pairs = [];
            Dictionary<int, double> sharedWeight = [];
            Dictionary<int, int> sharedCount = [];
            for (int index = 0; index < count; index += 1) {
                sharedWeight.Clear();
                sharedCount.Clear();
                foreach (int tag in tags[index]) {
                    foreach (int other in postings[tag]) {
                        if (other <= index) {
                            continue;
                        }
                        sharedWeight[other] = sharedWeight.GetValueOrDefault(other) + idf[tag];
                        sharedCount[other] = sharedCount.GetValueOrDefault(other) + 1;
                    }
                }
                foreach (KeyValuePair<int, double> pair in sharedWeight) {
                    if (sharedCount[pair.Key] < MinSharedTags) {
                        continue;
                    }
                    double union = total[index] + total[pair.Key] - pair.Value;
                    double similarity = union > 0 ? pair.Value / union : 0.0;
                    if (similarity >= minSimilarity) {
                        pairs.Add((index, pair.Key, similarity));
                    }
                }
            }

            List<int> ids = eligible.Select(x => x.Id).ToList();
            List<AnimeNetworkEdge> edges = SelectNearest(pairs, ids, neighborCount);
            return new AnimeNetworkResult(edges, ClusterDetector.LabelPropagation(edges), ids);
        }
        #endregion

        #region Bewertung
        /// <summary>
        /// Verbindungen nach ähnlicher Bewertung. Jeder bewertete Anime ist ein Punkt aus vier Teilnoten (Story, Animation, Effekte, Akustik).
        /// Die Ähnlichkeit ist 1 minus dem gewichteten Abstand (Gewichte wie in der Gesamtnote) geteilt durch den größtmöglichen Abstand.
        /// </summary>
        public static AnimeNetworkResult BuildRatings(IReadOnlyList<AnimeRatingEntry> ratings, double minSimilarity, int neighborCount) {
            int count = ratings.Count;
            double[][] vectors = ratings
                .Select(x => new double[] { x.Story, x.Animation, x.SpecialEffects, x.Acoustic })
                .ToArray();

            List<(int A, int B, double Similarity)> pairs = [];
            for (int first = 0; first < count; first += 1) {
                for (int second = first + 1; second < count; second += 1) {
                    double sum = 0.0;
                    for (int dimension = 0; dimension < RatingWeights.Length; dimension += 1) {
                        double difference = vectors[first][dimension] - vectors[second][dimension];
                        sum += RatingWeights[dimension] * difference * difference;
                    }
                    double similarity = 1.0 - Math.Sqrt(sum) / MaxRatingDistance;
                    if (similarity >= minSimilarity) {
                        pairs.Add((first, second, similarity));
                    }
                }
            }

            List<int> ids = ratings.Select(x => x.Id).ToList();
            List<AnimeNetworkEdge> edges = SelectNearest(pairs, ids, neighborCount);
            return new AnimeNetworkResult(edges, ClusterDetector.LabelPropagation(edges), ids);
        }
        #endregion

        /// <summary>
        /// Behält je Anime nur die <paramref name="neighborCount"/> ähnlichsten Verbindungen. Eine Verbindung bleibt erhalten,
        /// wenn sie für mindestens einen der beiden Animes zu dessen stärksten gehört.
        /// </summary>
        private static List<AnimeNetworkEdge> SelectNearest(
            IReadOnlyList<(int A, int B, double Similarity)> pairs,
            IReadOnlyList<int> ids,
            int neighborCount
        ) {
            int limit = Math.Max(1, neighborCount);
            List<int>[] byAnime = new List<int>[ids.Count];
            for (int index = 0; index < byAnime.Length; index += 1) {
                byAnime[index] = [];
            }
            for (int pairIndex = 0; pairIndex < pairs.Count; pairIndex += 1) {
                byAnime[pairs[pairIndex].A].Add(pairIndex);
                byAnime[pairs[pairIndex].B].Add(pairIndex);
            }

            HashSet<int> selected = [];
            foreach (List<int> candidates in byAnime) {
                foreach (int pairIndex in candidates.OrderByDescending(x => pairs[x].Similarity).Take(limit)) {
                    selected.Add(pairIndex);
                }
            }

            List<AnimeNetworkEdge> edges = [];
            foreach (int pairIndex in selected.OrderBy(x => x)) {
                (int A, int B, double Similarity) pair = pairs[pairIndex];
                edges.Add(new AnimeNetworkEdge(ids[pair.A], ids[pair.B], pair.Similarity, AnimeEdgeKind.Similarity));
            }
            return edges;
        }
    }
}
