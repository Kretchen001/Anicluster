using Microsoft.Data.Sqlite;
using System.Globalization;

namespace Anicluster.Database {

    /// <summary>
    /// Hilfsmethoden zum Lesen von Spalten (NULL-sicher, Spalten werden über ihre Position im SELECT angesprochen).
    /// </summary>
    internal static class SqliteReaderExtensions {

        private static readonly string[] DateFormats = ["yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd HH:mm:ss.FFFFFFF", "yyyy-MM-dd"];

        public static int GetInt(this SqliteDataReader reader, int ordinal) {
            return reader.IsDBNull(ordinal) ? 0 : reader.GetInt32(ordinal);
        }

        public static bool GetBool(this SqliteDataReader reader, int ordinal) {
            return reader.GetInt(ordinal) != 0;
        }

        public static string? GetStringOrNull(this SqliteDataReader reader, int ordinal) {
            return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
        }

        /// <summary>
        /// Liest ein Datum. Erwartet wird das Format "yyyy-MM-dd HH:mm:ss", ältere Datenbanken können
        /// Texte im Format der Systemkultur enthalten, die dann als Rückfall interpretiert werden.
        /// </summary>
        public static DateTime GetDate(this SqliteDataReader reader, int ordinal) {
            if (reader.IsDBNull(ordinal)) {
                return new DateTime(1, 1, 1);
            }
            string text = reader.GetString(ordinal);
            if (DateTime.TryParseExact(text, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result)) {
                return result;
            }
            if (DateTime.TryParse(text, CultureInfo.CurrentCulture, DateTimeStyles.None, out result)) {
                return result;
            }
            return new DateTime(1, 1, 1);
        }
    }
}
