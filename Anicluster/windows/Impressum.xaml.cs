using System.Windows;

namespace Anicluster.windows {
    /// <summary>
    /// Interaktionslogik für Impressum.xaml
    /// </summary>
    public partial class Impressum : Window {
        public Impressum() {
            InitializeComponent();

            CenterWindowOnScreen();
        }

        private void CenterWindowOnScreen() {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            double windowWidth = this.Width;
            double windowHeight = this.Height;
            if ((windowHeight > screenHeight) || (windowWidth > screenWidth)) {
                WindowState = WindowState.Maximized;
            }
            else {
                this.Left = (screenWidth / 2) - (windowWidth / 2);
                this.Top = (screenHeight / 2) - (windowHeight / 2);
            }
        }
    }
}
