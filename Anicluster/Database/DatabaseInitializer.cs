using Serilog;

namespace Anicluster.Database {

    /// <summary>
    /// Bereitet die Datenbank beim Start der Anwendung vor.
    /// </summary>
    public static class DatabaseInitializer {

        /// <summary>
        /// Legt die Datenbank an bzw. bringt sie auf die aktuelle Struktur und prüft anschließend den Zugriff.
        /// </summary>
        public static void CheckAndInitialize(DatabaseManager dbManager) {
            DatabaseSchema.EnsureCurrent(dbManager);
            using (DatabaseSession session = dbManager.BeginSession()) {
                long animeCount = session.ScalarLong("SELECT COUNT(*) FROM Anime;") ?? 0;
                Log.Information("Datenbank bereit (Struktur Version {Version}, {Count} Anime).", DatabaseSchema.CurrentVersion, animeCount);
            }
        }
    }
}
