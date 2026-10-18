using Microsoft.Data.Sqlite;
using System.IO;

namespace Anicluster.Database {

    /// <summary>
    /// Zentrale Anlaufstelle für die SQLite-Anbindung.
    /// </summary>
    public class DatabaseManager {

        private readonly string _connectionString;

        /// <summary>Vollständiger Pfad der Datenbankdatei</summary>
        public string DatabasePath { get; }

        public DatabaseManager(string connectionString) {
            this._connectionString = connectionString;
            this.DatabasePath = Path.GetFullPath(new SqliteConnectionStringBuilder(connectionString).DataSource);
        }

        /// <summary>Neue, noch nicht geöffnete Verbindung (z. B. für Schema-Arbeiten).</summary>
        public SqliteConnection GetConnection() {
            return new SqliteConnection(this._connectionString);
        }

        /// <summary>
        /// Öffnet eine Verbindung mit aktivierten Fremdschlüsseln und startet eine Transaktion.
        /// Erst <see cref="DatabaseSession.Commit"/> schreibt die Änderungen fest, ohne Commit werden sie beim Dispose verworfen.
        /// </summary>
        public DatabaseSession BeginSession() {
            SqliteConnection connection = this.GetConnection();
            try {
                connection.Open();
                using (SqliteCommand cmd = connection.CreateCommand()) {
                    // Gilt pro Verbindung und muss vor Beginn der Transaktion gesetzt werden
                    cmd.CommandText = "PRAGMA foreign_keys = ON;";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "PRAGMA foreign_keys;";
                    if (Convert.ToInt32(cmd.ExecuteScalar()) != 1) {
                        throw new InvalidOperationException("Fremdschlüssel konnten nicht aktiviert werden.");
                    }
                }
                return new DatabaseSession(connection);
            }
            catch {
                connection.Dispose();
                throw;
            }
        }
    }
}
