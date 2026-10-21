namespace Anicluster.Backup {

    /// <summary>Exportiert die ganze Bibliothek in eine JSON-Datei und liest sie wieder ein.</summary>
    public interface IBackupService {

        /// <summary>Schreibt alle Animes und Tags in die Datei. Die Datei entsteht erst vollständig, eine vorhandene bleibt sonst unverändert.</summary>
        Task<BackupResult> ExportAsync(string path, bool includeImages, IProgress<string>? progress = null);

        /// <summary>Liest und prüft eine Sicherungsdatei, ohne etwas zu verändern.</summary>
        Task<BackupReadResult> ReadAsync(string path);

        /// <summary>
        /// Übernimmt eine geprüfte Sicherung. Vorher wird die Datenbankdatei kopiert. Schlägt etwas fehl,
        /// wird die Kopie zurückgespielt, die Datenbank bleibt dann unverändert.
        /// </summary>
        Task<BackupResult> ImportAsync(BackupFile file, BackupImportMode mode, IProgress<string>? progress = null);
    }
}
