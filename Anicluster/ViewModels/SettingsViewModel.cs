using Anicluster.Backup;
using Anicluster.Dialogs;
using Anicluster.Settings;
using Anicluster.Theming;
using System.Windows;
using System.Windows.Threading;

namespace Anicluster.ViewModels {

    /// <summary>
    /// Einstellungsseite: Design wählen (Hell, Dunkel, Custom) und im Custom-Design die vier Grundfarben anpassen.
    /// Änderungen wirken sofort in der ganzen Anwendung (Live-Vorschau) und werden kurz verzögert gespeichert.
    /// </summary>
    public class SettingsViewModel : PropertyBaseChange {

        private readonly ThemeService _themeService;
        private readonly AppSettingsService _settingsService;
        private readonly IDialogService _dialogService;
        private readonly IBackupService _backupService;
        private readonly DispatcherTimer _saveTimer;
        private ThemeMode _mode;
        private bool _isLoadingColors;
        private bool _includeImages = true;
        private BackupImportMode _importMode = BackupImportMode.Merge;
        private bool _isBusy;
        private string _statusText = string.Empty;

        public ColorEditorViewModel BackgroundColor { get; }
        public ColorEditorViewModel SurfaceColor { get; }
        public ColorEditorViewModel TextColor { get; }
        public ColorEditorViewModel AccentColor { get; }

        public ThemeMode Mode {
            get => this._mode;
            set {
                if (this.SetProperty(ref this._mode, value)) {
                    this.OnPropertyChanged(nameof(this.IsCustomMode));
                    this.ApplyAndSave();
                }
            }
        }

        public bool IsCustomMode => this.Mode == ThemeMode.Custom;

        #region Datensicherung
        /// <summary>Wird ausgelöst, wenn ein Import die Daten verändert hat (die Bibliothek lädt dann neu).</summary>
        public event EventHandler? DataImported;

        /// <summary>Cover in den Export aufnehmen? Macht die Datei deutlich größer.</summary>
        public bool IncludeImages {
            get => this._includeImages;
            set => this.SetProperty(ref this._includeImages, value);
        }

        public BackupImportMode ImportMode {
            get => this._importMode;
            set {
                if (this.SetProperty(ref this._importMode, value)) {
                    this.OnPropertyChanged(nameof(this.ImportModeHint));
                }
            }
        }

        public string ImportModeHint => this.ImportMode == BackupImportMode.Merge
            ? "Ergänzt nur Animes, die noch nicht vorhanden sind (erkannt an Kennung, Quell-Id oder Name). Vorhandene bleiben unverändert."
            : "Ersetzt alle vorhandenen Animes und Tags durch den Inhalt der Datei. Vorher wird die Datenbank kopiert.";

        /// <summary>Läuft gerade ein Export oder Import?</summary>
        public bool IsBusy {
            get => this._isBusy;
            private set {
                if (this.SetProperty(ref this._isBusy, value)) {
                    this.OnPropertyChanged(nameof(this.IsIdle));
                }
            }
        }

        public bool IsIdle => !this.IsBusy;

        /// <summary>Fortschritt oder Ergebnis des letzten Vorgangs</summary>
        public string StatusText {
            get => this._statusText;
            private set {
                if (this.SetProperty(ref this._statusText, value)) {
                    this.OnPropertyChanged(nameof(this.HasStatus));
                }
            }
        }

        public bool HasStatus => this.StatusText.Length > 0;

        /// <summary>Fragt nach dem Ziel und exportiert die ganze Bibliothek in eine JSON-Datei.</summary>
        public async Task ExportAsync() {
            string? path = this._dialogService.PickSaveFile(
                "Datensicherung exportieren",
                "JSON-Datei (*.json)|*.json",
                $"Anicluster-Backup-{DateTime.Now:yyyyMMdd}.json"
            );
            if (path is null) {
                return;
            }
            this.IsBusy = true;
            try {
                Progress<string> progress = new Progress<string>(text => this.StatusText = text);
                BackupResult result = await this._backupService.ExportAsync(path, this.IncludeImages, progress);
                this.StatusText = result.Message;
                if (!result.Success) {
                    this._dialogService.ShowMessage(result.Message, "Export", MessageBoxImage.Error);
                }
            }
            finally {
                this.IsBusy = false;
            }
        }

        /// <summary>Fragt nach der Datei, prüft sie, lässt den Benutzer bestätigen und liest sie dann ein.</summary>
        public async Task ImportAsync() {
            string? path = this._dialogService.PickOpenFile("Datensicherung importieren", "JSON-Datei (*.json)|*.json|Alle Dateien|*.*");
            if (path is null) {
                return;
            }
            this.IsBusy = true;
            try {
                this.StatusText = "Prüfe Datei …";
                BackupReadResult read = await this._backupService.ReadAsync(path);
                if (read.File is null) {
                    this.StatusText = read.Message;
                    this._dialogService.ShowMessage(read.Message, "Import", MessageBoxImage.Warning);
                    return;
                }
                if (!this._dialogService.Confirm(this.BuildImportQuestion(read), "Datensicherung importieren")) {
                    this.StatusText = "Import abgebrochen.";
                    return;
                }
                Progress<string> progress = new Progress<string>(text => this.StatusText = text);
                BackupResult result = await this._backupService.ImportAsync(read.File, this.ImportMode, progress);
                this.StatusText = result.Message;
                if (result.Success) {
                    this.DataImported?.Invoke(this, EventArgs.Empty);
                }
                else {
                    this._dialogService.ShowMessage(result.Message, "Import", MessageBoxImage.Error);
                }
            }
            finally {
                this.IsBusy = false;
            }
        }

        private string BuildImportQuestion(BackupReadResult read) {
            int count = read.File?.Animes.Count ?? 0;
            string from = read.File is null ? string.Empty : $" vom {read.File.ExportedAt:dd.MM.yyyy HH:mm}";
            return this.ImportMode == BackupImportMode.Replace
                ? $"Die {read.ExistingAnimeCount} vorhandenen Animes und alle Tags werden durch die Sicherung{from} ({count} Animes) ersetzt.\n"
                    + "Vorher wird eine Kopie der Datenbank neben der Datenbankdatei angelegt.\n\nFortfahren?"
                : $"Die Sicherung{from} enthält {count} Animes. Fehlende werden ergänzt, vorhandene bleiben unverändert.\n\nFortfahren?";
        }
        #endregion

        public SettingsViewModel(
            ThemeService themeService,
            AppSettingsService settingsService,
            IDialogService dialogService,
            IBackupService backupService
        ) {
            this._themeService = themeService;
            this._dialogService = dialogService;
            this._backupService = backupService;
            this._settingsService = settingsService;
            this._mode = settingsService.Current.ThemeMode;

            ThemePalette custom = settingsService.Current.GetCustomPalette();
            this.BackgroundColor = new ColorEditorViewModel("Hintergrund", "Fenster und Seiten", custom.Background);
            this.SurfaceColor = new ColorEditorViewModel("Fläche", "Karten und Seitenleiste", custom.Surface);
            this.TextColor = new ColorEditorViewModel("Text", "Schriftfarbe", custom.Text);
            this.AccentColor = new ColorEditorViewModel("Akzent", "Hervorhebungen und Hauptaktionen", custom.Accent);
            foreach (ColorEditorViewModel editor in new[] { this.BackgroundColor, this.SurfaceColor, this.TextColor, this.AccentColor }) {
                editor.ColorChanged += this.OnCustomColorChanged;
            }

            // Mehrere Änderungen kurz hintereinander (Schieberegler) werden nur einmal gespeichert
            this._saveTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(400) };
            this._saveTimer.Tick += (sender, e) => {
                this._saveTimer.Stop();
                this._settingsService.Save();
            };
        }

        /// <summary>Übernimmt die Farben des hellen Designs als Ausgangspunkt für Custom.</summary>
        public void UseLightAsBase() {
            this.LoadColors(ThemePalette.Light);
        }

        /// <summary>Übernimmt die Farben des dunklen Designs als Ausgangspunkt für Custom.</summary>
        public void UseDarkAsBase() {
            this.LoadColors(ThemePalette.Dark);
        }

        private void LoadColors(ThemePalette palette) {
            this._isLoadingColors = true;
            this.BackgroundColor.Color = palette.Background;
            this.SurfaceColor.Color = palette.Surface;
            this.TextColor.Color = palette.Text;
            this.AccentColor.Color = palette.Accent;
            this._isLoadingColors = false;
            this.ApplyAndSave();
        }

        private void OnCustomColorChanged(object? sender, EventArgs e) {
            if (!this._isLoadingColors) {
                this.ApplyAndSave();
            }
        }

        private ThemePalette BuildCustomPalette() {
            return new ThemePalette(this.BackgroundColor.Color, this.SurfaceColor.Color, this.TextColor.Color, this.AccentColor.Color);
        }

        private void ApplyAndSave() {
            ThemePalette custom = this.BuildCustomPalette();
            this._themeService.Apply(this.Mode, custom);
            this._settingsService.Current.ThemeMode = this.Mode;
            this._settingsService.Current.SetCustomPalette(custom);
            this._saveTimer.Stop();
            this._saveTimer.Start();
        }
    }
}
