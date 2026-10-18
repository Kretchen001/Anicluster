using Anicluster.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace Anicluster.Views {

    /// <summary>
    /// Wiederverwendbarer Bewertungsbalken für ein <see cref="ScoreViewModel"/>.
    /// </summary>
    public partial class ScoreBarView : UserControl {

        public ScoreBarView() {
            this.InitializeComponent();
        }

        private void ProgressBar_MouseWheel(object sender, MouseWheelEventArgs e) {
            (this.DataContext as ScoreViewModel)?.Adjust(e.Delta > 0 ? 1 : -1);
        }

        private void ProgressBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            (this.DataContext as ScoreViewModel)?.PromptForValue();
        }
    }
}
