using Serilog;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Anicluster.Settings {

    /// <summary>
    /// Lädt und speichert die <see cref="AppSettings"/> in %AppData%\Anicluster\settings.json.
    /// </summary>
    public class AppSettingsService {

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        private readonly string _directory;
        private readonly string _filePath;

        public AppSettings Current { get; private set; } = new AppSettings();

        public AppSettingsService() {
            this._directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Anicluster");
            this._filePath = Path.Combine(this._directory, "settings.json");
        }

        /// <summary>Lädt die Einstellungen, ohne (oder mit defekter) Datei gelten die Standardwerte.</summary>
        public void Load() {
            try {
                if (File.Exists(this._filePath)) {
                    this.Current = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(this._filePath), JsonOptions) ?? new AppSettings();
                }
            }
            catch (Exception ex) {
                Log.Error(ex, "Einstellungen konnten nicht geladen werden, es gelten die Standardwerte.");
                this.Current = new AppSettings();
            }
        }

        public void Save() {
            try {
                Directory.CreateDirectory(this._directory);
                File.WriteAllText(this._filePath, JsonSerializer.Serialize(this.Current, JsonOptions));
            }
            catch (Exception ex) {
                Log.Error(ex, "Einstellungen konnten nicht gespeichert werden.");
            }
        }
    }
}
