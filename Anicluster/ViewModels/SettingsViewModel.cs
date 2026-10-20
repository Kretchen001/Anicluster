using Anicluster.Settings;
using Anicluster.Theming;
using System.Windows.Threading;

namespace Anicluster.ViewModels {

    /// <summary>
    /// Einstellungsseite: Design wählen (Hell, Dunkel, Custom) und im Custom-Design die vier Grundfarben anpassen.
    /// Änderungen wirken sofort in der ganzen Anwendung (Live-Vorschau) und werden kurz verzögert gespeichert.
    /// </summary>
    public class SettingsViewModel : PropertyBaseChange {

        private readonly ThemeService _themeService;
        private readonly AppSettingsService _settingsService;
        private readonly DispatcherTimer _saveTimer;
        private ThemeMode _mode;
        private bool _isLoadingColors;

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

        public SettingsViewModel(ThemeService themeService, AppSettingsService settingsService) {
            this._themeService = themeService;
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
