using Anicluster.AnimeNetwork;
using Anicluster.Theming;
using System.Windows.Media;

namespace Anicluster.Settings {

    /// <summary>Darstellung der Anime-Liste in der Bibliothek.</summary>
    public enum LibraryViewMode {
        Cards,
        Table,
        Images
    }

    /// <summary>
    /// Persönliche Einstellungen des Benutzers (werden als JSON gespeichert).
    /// </summary>
    public class AppSettings {

        public ThemeMode ThemeMode { get; set; } = ThemeMode.Light;

        public LibraryViewMode LibraryViewMode { get; set; } = LibraryViewMode.Cards;

        // Anime-Netzwerk: zuletzt gewählte Netzwerkart und Regler (Mindestähnlichkeit in Prozent je Netzwerkart)
        public AnimeNetworkMode AnimeNetworkMode { get; set; } = AnimeNetworkMode.Relations;
        public int AnimeNetworkTagSimilarity { get; set; } = 25;
        public int AnimeNetworkRatingSimilarity { get; set; } = 90;
        public int AnimeNetworkNeighborCount { get; set; } = 5;
        public bool AnimeNetworkShowUnlinked { get; set; }

        // Farben des Custom-Designs als Hex-Text, vorbelegt mit dem hellen Design
        public string CustomBackground { get; set; } = ColorHelper.ToHex(ThemePalette.Light.Background);
        public string CustomSurface { get; set; } = ColorHelper.ToHex(ThemePalette.Light.Surface);
        public string CustomText { get; set; } = ColorHelper.ToHex(ThemePalette.Light.Text);
        public string CustomAccent { get; set; } = ColorHelper.ToHex(ThemePalette.Light.Accent);

        /// <summary>Die Custom-Palette, ungültige Farbwerte fallen auf das helle Design zurück.</summary>
        public ThemePalette GetCustomPalette() {
            return new ThemePalette(
                ColorHelper.FromHex(this.CustomBackground, ThemePalette.Light.Background),
                ColorHelper.FromHex(this.CustomSurface, ThemePalette.Light.Surface),
                ColorHelper.FromHex(this.CustomText, ThemePalette.Light.Text),
                ColorHelper.FromHex(this.CustomAccent, ThemePalette.Light.Accent)
            );
        }

        public void SetCustomPalette(ThemePalette palette) {
            this.CustomBackground = ColorHelper.ToHex(palette.Background);
            this.CustomSurface = ColorHelper.ToHex(palette.Surface);
            this.CustomText = ColorHelper.ToHex(palette.Text);
            this.CustomAccent = ColorHelper.ToHex(palette.Accent);
        }
    }
}
