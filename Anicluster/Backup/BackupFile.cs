using Modells.Anime;

namespace Anicluster.Backup {

    /// <summary>
    /// Inhalt einer Datensicherung als JSON-Datei: ein Kopf mit Format und Version, danach alle Tags und Animes.
    /// Die Animes enthalten alles, was zu ihnen gehört (Bewertung, Staffeln, OVAs, Tags, Medieninfos, Bild).
    /// Die Ids in der Datei gelten nur innerhalb der Datei, Animes werden über <see cref="Anime.Uid"/> wiedererkannt.
    /// </summary>
    public class BackupFile {

        public const string FormatName = "Anicluster-Backup";

        /// <summary>Version des Dateiformats. Wird erhöht, wenn sich der Aufbau der Datei inkompatibel ändert.</summary>
        public const int CurrentFormatVersion = 1;

        public string Format { get; set; } = FormatName;

        public int FormatVersion { get; set; } = CurrentFormatVersion;

        /// <summary>Version von Anicluster, die die Datei erzeugt hat</summary>
        public string AppVersion { get; set; } = string.Empty;

        /// <summary>Version der Datenbankstruktur zum Zeitpunkt des Exports</summary>
        public int SchemaVersion { get; set; }

        /// <summary>Zeitpunkt des Exports (Ortszeit)</summary>
        public DateTime ExportedAt { get; set; }

        /// <summary>Enthält die Datei die Cover der Animes?</summary>
        public bool IncludesImages { get; set; }

        public List<Tag> Tags { get; set; } = [];

        public List<Anime> Animes { get; set; } = [];
    }
}
