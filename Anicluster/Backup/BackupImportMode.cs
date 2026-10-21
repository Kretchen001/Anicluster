namespace Anicluster.Backup {

    /// <summary>Wie eine Datensicherung eingelesen wird.</summary>
    public enum BackupImportMode {

        /// <summary>Ergänzt nur Animes, die noch nicht vorhanden sind. Vorhandene bleiben unverändert.</summary>
        Merge,

        /// <summary>Ersetzt alle vorhandenen Animes und Tags durch den Inhalt der Datei (Wiederherstellen).</summary>
        Replace
    }
}
