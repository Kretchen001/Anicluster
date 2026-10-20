using System.Windows;
using System.Windows.Media;

namespace Anicluster.Theming {

    /// <summary>
    /// Wendet ein Design auf die ganze Anwendung an. Die Farben liegen als Pinsel in den Anwendungs-Ressourcen
    /// (Schlüssel wie "SurfaceBrush"), die Styles verwenden sie per DynamicResource und folgen deshalb jedem Wechsel sofort.
    /// </summary>
    public class ThemeService {

        public ThemeMode Mode { get; private set; } = ThemeMode.Light;

        /// <summary>Die gerade verwendete Palette</summary>
        public ThemePalette ActivePalette { get; private set; } = ThemePalette.Light;

        public event EventHandler? ThemeChanged;

        /// <summary>Wendet das Design an. <paramref name="customPalette"/> wird nur im Modus Custom verwendet.</summary>
        public void Apply(ThemeMode mode, ThemePalette customPalette) {
            this.Mode = mode;
            this.ActivePalette = mode switch {
                ThemeMode.Dark => ThemePalette.Dark,
                ThemeMode.Custom => customPalette,
                _ => ThemePalette.Light
            };
            this.ApplyPalette(this.ActivePalette);
            WindowThemeHelper.IsDark = this.ActivePalette.IsDark;
            foreach (Window window in Application.Current.Windows) {
                WindowThemeHelper.Refresh(window);
            }
            this.ThemeChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Leitet aus den vier Grundfarben alle Pinsel ab und trägt sie in die Ressourcen ein.</summary>
        private void ApplyPalette(ThemePalette palette) {
            Color background = palette.Background;
            Color surface = palette.Surface;
            Color text = palette.Text;
            Color accent = palette.Accent;
            bool isDark = palette.IsDark;

            Color surfaceAlt = ColorHelper.Blend(surface, text, 0.05);
            Color surfaceHover = ColorHelper.Blend(surface, text, 0.09);
            Color line = ColorHelper.Blend(surface, text, 0.14);
            Color textMuted = ColorHelper.Blend(text, surface, 0.42);
            Color accentText = ColorHelper.ReadableOn(accent);
            Color accentSoft = ColorHelper.Blend(surface, accent, isDark ? 0.22 : 0.13);
            Color danger = isDark ? Color.FromRgb(0xF2, 0x77, 0x7A) : Color.FromRgb(0xD1, 0x34, 0x38);
            Color success = isDark ? Color.FromRgb(0x4C, 0xC9, 0x8A) : Color.FromRgb(0x1E, 0x8E, 0x5A);

            this.SetBrush("BackgroundBrush", background);
            this.SetBrush("SurfaceBrush", surface);
            this.SetBrush("SurfaceAltBrush", surfaceAlt);
            this.SetBrush("SurfaceHoverBrush", surfaceHover);
            this.SetBrush("LineBrush", line);
            this.SetBrush("TextBrush", text);
            this.SetBrush("TextMutedBrush", textMuted);
            this.SetBrush("AccentBrush", accent);
            this.SetBrush("AccentTextBrush", accentText);
            this.SetBrush("AccentSoftBrush", accentSoft);
            this.SetBrush("DangerBrush", danger);
            this.SetBrush("SuccessBrush", success);
            this.SetBrush("EdgeBrush", ColorHelper.Blend(surface, text, isDark ? 0.34 : 0.30));
            this.SetBrush("ScrollThumbBrush", ColorHelper.Blend(surface, text, 0.28));
            this.SetBrush("HoverOverlayBrush", ColorHelper.WithAlpha(text, 0x16));
            this.SetBrush("PressedOverlayBrush", ColorHelper.WithAlpha(text, 0x2A));
            this.SetBrush("ShadowBrush", Color.FromArgb(isDark ? (byte)0xB0 : (byte)0x40, 0, 0, 0));

            // Die Standard-Controls von WPF (Kalender, Tooltips, Auswahlfarben ...) lesen ihre Farben aus den Systemfarben
            this.SetSystemBrush(SystemColors.WindowBrushKey, surface);
            this.SetSystemBrush(SystemColors.WindowTextBrushKey, text);
            this.SetSystemBrush(SystemColors.ControlBrushKey, surfaceAlt);
            this.SetSystemBrush(SystemColors.ControlTextBrushKey, text);
            this.SetSystemBrush(SystemColors.ControlLightBrushKey, surfaceAlt);
            this.SetSystemBrush(SystemColors.ControlLightLightBrushKey, surface);
            this.SetSystemBrush(SystemColors.ControlDarkBrushKey, line);
            this.SetSystemBrush(SystemColors.GrayTextBrushKey, textMuted);
            this.SetSystemBrush(SystemColors.HighlightBrushKey, accent);
            this.SetSystemBrush(SystemColors.HighlightTextBrushKey, accentText);
            this.SetSystemBrush(SystemColors.InactiveSelectionHighlightBrushKey, accentSoft);
            this.SetSystemBrush(SystemColors.InactiveSelectionHighlightTextBrushKey, text);
            this.SetSystemBrush(SystemColors.MenuBrushKey, surface);
            this.SetSystemBrush(SystemColors.MenuTextBrushKey, text);
        }

        private void SetBrush(string key, Color color) {
            Application.Current.Resources[key] = ColorHelper.CreateBrush(color);
        }

        private void SetSystemBrush(ResourceKey key, Color color) {
            Application.Current.Resources[key] = ColorHelper.CreateBrush(color);
        }
    }
}
