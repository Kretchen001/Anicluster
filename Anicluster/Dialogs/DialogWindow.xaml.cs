using Anicluster.Theming;
using System.Windows;

namespace Anicluster.Dialogs {

    /// <summary>
    /// Generisches Fenster für Dialoge. Zeigt das ViewModel aus dem DataContext über das passende DataTemplate an.
    /// </summary>
    public partial class DialogWindow : Window {

        /// <summary>Anteil der Größe des übergeordneten Fensters, den ein mitwachsender Dialog einnimmt</summary>
        private const double OwnerSizeFactor = 0.92;

        /// <summary>
        /// Soll das Fenster mit dem übergeordneten Fenster mitwachsen? Es ist dann nie kleiner als sein Inhalt
        /// und bleibt auf dem Bildschirm. Ohne diese Einstellung passt sich das Fenster seinem Inhalt an.
        /// </summary>
        public bool FillsOwnerWindow { get; set; }

        public DialogWindow() {
            this.InitializeComponent();
            WindowThemeHelper.Attach(this);
            this.Loaded += this.DialogWindow_Loaded;
        }

        private void DialogWindow_Loaded(object sender, RoutedEventArgs e) {
            if (!this.FillsOwnerWindow || this.Owner is not Window owner) {
                return;
            }
            // Bis hierher hat SizeToContent die Größe des Inhalts ermittelt, sie ist die Untergrenze
            double minWidth = this.ActualWidth;
            double minHeight = this.ActualHeight;
            Rect screen = SystemParameters.WorkArea;
            Rect area = owner.WindowState == WindowState.Normal
                ? new Rect(owner.Left, owner.Top, owner.ActualWidth, owner.ActualHeight)
                : screen;
            double width = Math.Min(Math.Max(minWidth, area.Width * OwnerSizeFactor), screen.Width);
            double height = Math.Min(Math.Max(minHeight, area.Height * OwnerSizeFactor), screen.Height);
            this.MinWidth = minWidth;
            this.MinHeight = minHeight;
            this.SizeToContent = SizeToContent.Manual;
            this.Width = width;
            this.Height = height;
            // Mittig über dem übergeordneten Fenster, aber vollständig auf dem Bildschirm
            this.Left = Math.Max(screen.Left, Math.Min(area.Left + (area.Width - width) / 2, screen.Right - width));
            this.Top = Math.Max(screen.Top, Math.Min(area.Top + (area.Height - height) / 2, screen.Bottom - height));
        }
    }
}
