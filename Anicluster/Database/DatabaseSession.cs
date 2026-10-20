using Microsoft.Data.Sqlite;
using System.Data;
using System.Globalization;

namespace Anicluster.Database {

    /// <summary>
    /// Eine geöffnete Verbindung mit laufender Transaktion. Alle Services, die an einem Vorgang beteiligt sind,
    /// teilen sich dieselbe Session, dadurch wird ein Vorgang (z. B. das Speichern eines Animes) ganz oder gar nicht ausgeführt.
    /// Parameter werden als Tupel (Name, Wert) übergeben, <c>null</c> wird als SQL-NULL gespeichert.
    /// </summary>
    public sealed class DatabaseSession : IDisposable {

        private readonly SqliteConnection _connection;
        private readonly SqliteTransaction _transaction;

        internal DatabaseSession(SqliteConnection connection) {
            this._connection = connection;
            this._transaction = connection.BeginTransaction();
        }

        /// <summary>Führt ein Statement ohne Ergebnis aus und liefert die Anzahl betroffener Zeilen.</summary>
        public int Execute(string sql, params (string Name, object? Value)[] parameters) {
            using (SqliteCommand cmd = this.CreateCommand(sql, parameters)) {
                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>Führt ein INSERT aus und liefert die Id der neuen Zeile.</summary>
        public int Insert(string sql, params (string Name, object? Value)[] parameters) {
            this.Execute(sql, parameters);
            return Convert.ToInt32(this.ScalarLong("SELECT last_insert_rowid();"));
        }

        /// <summary>Liefert den ersten Wert der ersten Zeile, <c>null</c> wenn es keine Zeile gibt.</summary>
        public long? ScalarLong(string sql, params (string Name, object? Value)[] parameters) {
            using (SqliteCommand cmd = this.CreateCommand(sql, parameters)) {
                object? result = cmd.ExecuteScalar();
                if (result is null || result is DBNull) {
                    return null;
                }
                return Convert.ToInt64(result);
            }
        }

        /// <summary>Führt eine Abfrage aus und wandelt jede Zeile mit <paramref name="map"/> um.</summary>
        public List<T> Query<T>(string sql, Func<SqliteDataReader, T> map, params (string Name, object? Value)[] parameters) {
            List<T> result = [];
            using (SqliteCommand cmd = this.CreateCommand(sql, parameters)) {
                using (SqliteDataReader reader = cmd.ExecuteReader()) {
                    while (reader.Read()) {
                        result.Add(map(reader));
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Führt beliebiges SQL (auch mehrere Statements) aus. Geliefert wird die letzte Ergebnismenge mit Spalten,
        /// <paramref name="recordsAffected"/> enthält die Zahl der geänderten Zeilen (−1 bei reinen Abfragen).
        /// </summary>
        public DataTable ExecuteRaw(string sql, out int recordsAffected) {
            DataTable result = new DataTable();
            using (SqliteCommand cmd = this.CreateCommand(sql, [])) {
                using (SqliteDataReader reader = cmd.ExecuteReader()) {
                    do {
                        if (reader.FieldCount > 0) {
                            result = new DataTable();
                            result.Load(reader);
                        }
                    } while (!reader.IsClosed && reader.NextResult());
                    recordsAffected = reader.RecordsAffected;
                }
            }
            return result;
        }

        /// <summary>Schreibt alle Änderungen dieser Session fest.</summary>
        public void Commit() {
            this._transaction.Commit();
        }

        /// <summary>Beendet die Session, nicht festgeschriebene Änderungen werden verworfen (Rollback).</summary>
        public void Dispose() {
            this._transaction.Dispose();
            this._connection.Dispose();
        }

        private SqliteCommand CreateCommand(string sql, (string Name, object? Value)[] parameters) {
            SqliteCommand cmd = this._connection.CreateCommand();
            cmd.Transaction = this._transaction;
            cmd.CommandText = sql;
            foreach ((string Name, object? Value) parameter in parameters) {
                cmd.Parameters.AddWithValue(parameter.Name, ToDatabaseValue(parameter.Value));
            }
            return cmd;
        }

        /// <summary>Einheitliche Umwandlung: null → NULL, Enum → Zahl, bool → 0/1, DateTime → "yyyy-MM-dd HH:mm:ss".</summary>
        private static object ToDatabaseValue(object? value) {
            return value switch {
                null => DBNull.Value,
                DateTime dateTime => dateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                Enum enumValue => Convert.ToInt32(enumValue),
                bool flag => flag ? 1 : 0,
                _ => value
            };
        }
    }
}
