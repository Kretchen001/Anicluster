using System.Windows.Media;

namespace Anicluster.Theming {

    /// <summary>
    /// Die vier Grundfarben eines Designs. Alle weiteren Farben (Rahmen, Hover, gedämpfter Text, ...)
    /// leitet der <see cref="ThemeService"/> daraus ab, dadurch verhalten sich Hell, Dunkel und Custom gleich.
    /// </summary>
    /// <param name="Background">Hintergrund der Fenster und Seiten</param>
    /// <param name="Surface">Flächen darauf, z. B. Karten und Seitenleiste</param>
    /// <param name="Text">Textfarbe</param>
    /// <param name="Accent">Akzentfarbe für Hervorhebungen und Hauptaktionen</param>
    public record ThemePalette(Color Background, Color Surface, Color Text, Color Accent) {

        public static ThemePalette Light { get; } = new ThemePalette(
            Color.FromRgb(0xF3, 0xF4, 0xF6),
            Color.FromRgb(0xFF, 0xFF, 0xFF),
            Color.FromRgb(0x1F, 0x29, 0x37),
            Color.FromRgb(0x4F, 0x46, 0xE5)
        );

        public static ThemePalette Dark { get; } = new ThemePalette(
            Color.FromRgb(0x13, 0x15, 0x1A),
            Color.FromRgb(0x1E, 0x21, 0x28),
            Color.FromRgb(0xE6, 0xE8, 0xEC),
            Color.FromRgb(0x8B, 0x92, 0xFF)
        );

        /// <summary>Ist der Hintergrund dunkel? Steuert z. B. die Titelleiste der Fenster.</summary>
        public bool IsDark => ColorHelper.Luminance(this.Background) < 0.5;
    }
}
