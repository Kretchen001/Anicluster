namespace Anicluster.AnimeNetwork {

    /// <summary>
    /// Konvexe Hülle einer Punktmenge (Andrew's Monotone Chain), unabhängig von WPF.
    /// Die Hülle der Cluster liefert die Form des Hintergrunds, der die Animes eines Clusters umgibt.
    /// </summary>
    public static class ConvexHull {

        /// <summary>Liefert die Eckpunkte der Hülle gegen den Uhrzeigersinn. Bei weniger als drei Punkten werden die Punkte unverändert geliefert.</summary>
        public static List<(double X, double Y)> Compute(IReadOnlyList<(double X, double Y)> points) {
            if (points.Count < 3) {
                return points.ToList();
            }
            List<(double X, double Y)> sorted = points.OrderBy(p => p.X).ThenBy(p => p.Y).ToList();
            List<(double X, double Y)> hull = [];

            // Untere Hülle von links nach rechts
            foreach ((double X, double Y) point in sorted) {
                while (hull.Count >= 2 && Cross(hull[hull.Count - 2], hull[hull.Count - 1], point) <= 0) {
                    hull.RemoveAt(hull.Count - 1);
                }
                hull.Add(point);
            }

            // Obere Hülle von rechts nach links
            int lowerCount = hull.Count + 1;
            for (int index = sorted.Count - 2; index >= 0; index -= 1) {
                (double X, double Y) point = sorted[index];
                while (hull.Count >= lowerCount && Cross(hull[hull.Count - 2], hull[hull.Count - 1], point) <= 0) {
                    hull.RemoveAt(hull.Count - 1);
                }
                hull.Add(point);
            }

            // Der letzte Punkt ist der erste
            hull.RemoveAt(hull.Count - 1);
            return hull;
        }

        private static double Cross((double X, double Y) origin, (double X, double Y) a, (double X, double Y) b) {
            return (a.X - origin.X) * (b.Y - origin.Y) - (a.Y - origin.Y) * (b.X - origin.X);
        }
    }
}
