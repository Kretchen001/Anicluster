namespace Anicluster.Backup {

    /// <summary>Ergebnis eines Exports oder Imports mit einer Meldung für den Benutzer.</summary>
    public record BackupResult(bool Success, string Message);

    /// <summary>
    /// Ergebnis beim Lesen einer Sicherungsdatei. Bei einem Fehler ist <see cref="File"/> leer und <see cref="Message"/> der Grund.
    /// </summary>
    public record BackupReadResult(BackupFile? File, string Message, int ExistingAnimeCount);
}
