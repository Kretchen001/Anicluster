namespace Anicluster.AnimeNetwork {

    /// <summary>
    /// Findet Cluster in einem Netzwerk. Nur Animes mit mindestens einer Verbindung bekommen eine Cluster-Nummer,
    /// die Nummern sind nach Größe sortiert (0 = größter Cluster, bei Gleichstand entscheidet die kleinste Anime-Id).
    /// Das Ergebnis hängt nur von den Eingaben ab, nicht von Zufall.
    /// </summary>
    public static class ClusterDetector {

        private const int MaxIterations = 25;
        private const double Epsilon = 1e-9;

        /// <summary>Zusammenhängende Gruppen (alle Animes, die über Verbindungen erreichbar sind, bilden einen Cluster).</summary>
        public static Dictionary<int, int> ConnectedComponents(IReadOnlyList<AnimeNetworkEdge> edges) {
            Dictionary<int, List<(int Other, double Weight)>> adjacency = BuildAdjacency(edges);
            Dictionary<int, int> labels = [];
            int nextLabel = 0;
            foreach (int start in adjacency.Keys.OrderBy(x => x)) {
                if (labels.ContainsKey(start)) {
                    continue;
                }
                Stack<int> pending = new Stack<int>();
                pending.Push(start);
                labels[start] = nextLabel;
                while (pending.Count > 0) {
                    int current = pending.Pop();
                    foreach (int other in adjacency[current].Select(x => x.Other)) {
                        if (!labels.ContainsKey(other)) {
                            labels[other] = nextLabel;
                            pending.Push(other);
                        }
                    }
                }
                nextLabel += 1;
            }
            return Renumber(labels);
        }

        /// <summary>
        /// Label Propagation: Jeder Anime übernimmt der Reihe nach das Label, das unter seinen Nachbarn (nach Verbindungsstärke) am stärksten
        /// vertreten ist. Dadurch zerfällt ein Netz auch dann in Gruppen, wenn einzelne Verbindungen sie verknüpfen.
        /// </summary>
        public static Dictionary<int, int> LabelPropagation(IReadOnlyList<AnimeNetworkEdge> edges) {
            Dictionary<int, List<(int Other, double Weight)>> adjacency = BuildAdjacency(edges);
            List<int> order = adjacency.Keys.OrderBy(x => x).ToList();
            Dictionary<int, int> labels = [];
            foreach (int id in order) {
                labels[id] = id;
            }
            Dictionary<int, double> scores = [];
            for (int iteration = 0; iteration < MaxIterations; iteration += 1) {
                bool changed = false;
                foreach (int id in order) {
                    scores.Clear();
                    foreach ((int other, double weight) in adjacency[id]) {
                        int label = labels[other];
                        scores[label] = scores.GetValueOrDefault(label) + weight;
                    }
                    double maxScore = scores.Values.Max();
                    int current = labels[id];
                    // Das eigene Label bleibt bei Gleichstand erhalten, das verhindert ein Hin und Her
                    if (scores.GetValueOrDefault(current) >= maxScore - Epsilon) {
                        continue;
                    }
                    int best = int.MaxValue;
                    foreach (KeyValuePair<int, double> pair in scores) {
                        if (pair.Value >= maxScore - Epsilon && pair.Key < best) {
                            best = pair.Key;
                        }
                    }
                    labels[id] = best;
                    changed = true;
                }
                if (!changed) {
                    break;
                }
            }
            return Renumber(labels);
        }

        private static Dictionary<int, List<(int Other, double Weight)>> BuildAdjacency(IReadOnlyList<AnimeNetworkEdge> edges) {
            Dictionary<int, List<(int Other, double Weight)>> adjacency = [];
            foreach (AnimeNetworkEdge edge in edges) {
                AddNeighbor(adjacency, edge.IdA, edge.IdB, edge.Weight);
                AddNeighbor(adjacency, edge.IdB, edge.IdA, edge.Weight);
            }
            return adjacency;
        }

        private static void AddNeighbor(Dictionary<int, List<(int Other, double Weight)>> adjacency, int id, int other, double weight) {
            if (!adjacency.TryGetValue(id, out List<(int Other, double Weight)>? list)) {
                list = [];
                adjacency[id] = list;
            }
            list.Add((other, weight));
        }

        /// <summary>Nummeriert die Labels nach Größe der Gruppe neu (0 = größte Gruppe).</summary>
        private static Dictionary<int, int> Renumber(Dictionary<int, int> labels) {
            List<IGrouping<int, int>> groups = labels
                .GroupBy(x => x.Value, x => x.Key)
                .OrderByDescending(group => group.Count())
                .ThenBy(group => group.Min())
                .ToList();
            Dictionary<int, int> result = [];
            for (int index = 0; index < groups.Count; index += 1) {
                foreach (int id in groups[index]) {
                    result[id] = index;
                }
            }
            return result;
        }
    }
}
