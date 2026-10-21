using Anicluster.Database;
using Anicluster.Dialogs;
using Microsoft.Data.Sqlite;
using Serilog;
using System.Data;
using System.Windows;

namespace Anicluster.ViewModels {

    /// <summary>
    /// Verstecktes Werkzeug: führt SQL direkt gegen die Datenbank aus und zeigt das Ergebnis als Tabelle.
    /// Jede Ausführung läuft in einer eigenen Transaktion, bei einem Fehler wird nichts übernommen.
    /// </summary>
    public class SqlConsoleViewModel : DialogViewModelBase {

        private static readonly string[] ReadOnlyKeywords = ["SELECT", "PRAGMA", "EXPLAIN", "WITH"];

        private readonly DatabaseManager _databaseManager;
        private readonly IDialogService _dialogService;
        private string _sqlText = "SELECT name, type FROM sqlite_master ORDER BY type, name;";
        private DataView? _resultView;
        private string _statusText = "Strg + Enter führt den Befehl aus.";
        private bool _hasError;

        public override string Title => "SQL-Konsole";

        public override bool FillsOwnerWindow => true;

        public string SqlText {
            get => this._sqlText;
            set => this.SetProperty(ref this._sqlText, value);
        }

        /// <summary>Ergebnis der letzten Ausführung (leer bei Befehlen ohne Ergebnismenge).</summary>
        public DataView? ResultView {
            get => this._resultView;
            private set => this.SetProperty(ref this._resultView, value);
        }

        public string StatusText {
            get => this._statusText;
            private set => this.SetProperty(ref this._statusText, value);
        }

        /// <summary>Färbt die Statuszeile ein, wenn die letzte Ausführung fehlgeschlagen ist.</summary>
        public bool HasError {
            get => this._hasError;
            private set => this.SetProperty(ref this._hasError, value);
        }

        public SqlConsoleViewModel(DatabaseManager databaseManager, IDialogService dialogService) {
            this._databaseManager = databaseManager;
            this._dialogService = dialogService;
        }

        public void Close() {
            this.RequestClose(false);
        }

        /// <summary>Führt den eingegebenen Befehl aus. Verändernde Befehle müssen vorher bestätigt werden.</summary>
        public void Execute() {
            string sql = this.SqlText.Trim();
            if (sql.Length == 0) {
                return;
            }
            if (!IsReadOnly(sql) && !this._dialogService.Confirm("Der Befehl kann Daten oder die Struktur der Datenbank verändern.\nWirklich ausführen?", "SQL-Konsole")) {
                return;
            }

            try {
                System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
                DataTable table;
                int recordsAffected;
                using (DatabaseSession session = this._databaseManager.BeginSession()) {
                    table = session.ExecuteRaw(sql, out recordsAffected);
                    session.Commit();
                }
                stopwatch.Stop();

                this.ResultView = table.Columns.Count > 0 ? table.DefaultView : null;
                this.HasError = false;
                this.StatusText = table.Columns.Count > 0
                    ? $"{table.Rows.Count} Zeile(n) in {stopwatch.ElapsedMilliseconds} ms"
                    : $"Ausgeführt in {stopwatch.ElapsedMilliseconds} ms, {Math.Max(recordsAffected, 0)} Zeile(n) betroffen";
                if (!IsReadOnly(sql)) {
                    Log.Warning("SQL-Konsole: {Sql}", sql);
                }
            }
            catch (Exception ex) when (ex is SqliteException or InvalidOperationException) {
                this.ResultView = null;
                this.HasError = true;
                this.StatusText = ex.Message;
            }
        }

        /// <summary>Erkennt reine Leseabfragen am ersten Schlüsselwort (Kommentare am Anfang werden übersprungen).</summary>
        private static bool IsReadOnly(string sql) {
            string text = sql;
            while (true) {
                text = text.TrimStart();
                if (text.StartsWith("--")) {
                    int end = text.IndexOf('\n');
                    text = end < 0 ? string.Empty : text[end..];
                }
                else if (text.StartsWith("/*")) {
                    int end = text.IndexOf("*/", StringComparison.Ordinal);
                    text = end < 0 ? string.Empty : text[(end + 2)..];
                }
                else {
                    break;
                }
            }
            // Mehrere Statements gelten nur dann als lesend, wenn nach dem ersten Semikolon nichts Verändernder folgt
            foreach (string part in text.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)) {
                if (!ReadOnlyKeywords.Any(keyword => part.StartsWith(keyword, StringComparison.OrdinalIgnoreCase))
                    || part.StartsWith("PRAGMA", StringComparison.OrdinalIgnoreCase) && part.Contains('=')) {
                    return false;
                }
            }
            return true;
        }
    }
}
