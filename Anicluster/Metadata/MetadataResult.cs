namespace Anicluster.Metadata {

    /// <summary>Ausgang einer Abfrage bei einer Online-Quelle.</summary>
    public enum MetadataStatus {
        Success,
        NotFound,

        /// <summary>Die Quelle bremst (HTTP 429) oder wurde nach einer Bremse noch nicht wieder angefragt.</summary>
        RateLimited,

        /// <summary>Keine Verbindung (kein Netz, DNS- oder Verbindungsfehler)</summary>
        Offline,

        Timeout,

        /// <summary>Die Quelle meldet einen Fehler oder liefert eine unverständliche Antwort.</summary>
        ServiceError,

        Canceled
    }

    /// <summary>
    /// Ergebnis einer Abfrage: entweder der Wert oder ein Grund, warum es ihn nicht gibt. Fehler werfen keine Ausnahme,
    /// sie sind erwartbar (offline, Limit erreicht) und werden vom ViewModel in eine Meldung übersetzt.
    /// </summary>
    public class MetadataResult<T> {

        public MetadataStatus Status { get; }

        public T? Value { get; }

        /// <summary>Bei <see cref="MetadataStatus.RateLimited"/>: nach wie vielen Sekunden es wieder geht</summary>
        public int? RetryAfterSeconds { get; }

        /// <summary>Technische Angabe für das Log, nicht für den Benutzer</summary>
        public string? Detail { get; }

        public bool IsSuccess => this.Status == MetadataStatus.Success;

        private MetadataResult(MetadataStatus status, T? value, int? retryAfterSeconds, string? detail) {
            this.Status = status;
            this.Value = value;
            this.RetryAfterSeconds = retryAfterSeconds;
            this.Detail = detail;
        }

        public static MetadataResult<T> Success(T value) {
            return new MetadataResult<T>(MetadataStatus.Success, value, null, null);
        }

        public static MetadataResult<T> Failure(MetadataStatus status, int? retryAfterSeconds = null, string? detail = null) {
            return new MetadataResult<T>(status, default, retryAfterSeconds, detail);
        }

        /// <summary>Überträgt einen Fehler auf einen anderen Ergebnistyp (zum Beispiel Text → Trefferliste).</summary>
        public MetadataResult<TOther> AsFailure<TOther>() {
            return MetadataResult<TOther>.Failure(this.Status, this.RetryAfterSeconds, this.Detail);
        }
    }
}
