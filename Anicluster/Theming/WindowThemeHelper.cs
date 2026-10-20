using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Anicluster.Theming {

    /// <summary>
    /// Passt die Titelleiste der Fenster an das Design an (dunkle Titelleiste unter Windows 10/11).
    /// Fenster melden sich einmal per <see cref="Attach"/> an, danach folgen sie jedem Designwechsel.
    /// </summary>
    internal static class WindowThemeHelper {

        private const int UseImmersiveDarkModeBeforeWindows10Update = 19;
        private const int UseImmersiveDarkMode = 20;

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

        /// <summary>Aktuell aktives Design: dunkel?</summary>
        public static bool IsDark { get; set; }

        /// <summary>Wendet die Titelleiste an, sobald das Fenster ein Handle hat.</summary>
        public static void Attach(Window window) {
            window.SourceInitialized += (sender, e) => Refresh(window);
        }

        public static void Refresh(Window window) {
            IntPtr handle = new WindowInteropHelper(window).Handle;
            if (handle == IntPtr.Zero) {
                return;
            }
            try {
                int value = IsDark ? 1 : 0;
                if (DwmSetWindowAttribute(handle, UseImmersiveDarkMode, ref value, sizeof(int)) != 0) {
                    DwmSetWindowAttribute(handle, UseImmersiveDarkModeBeforeWindows10Update, ref value, sizeof(int));
                }
            }
            catch (Exception) {
                // Ältere Windows-Versionen ohne diese Funktion behalten einfach die normale Titelleiste
            }
        }
    }
}
