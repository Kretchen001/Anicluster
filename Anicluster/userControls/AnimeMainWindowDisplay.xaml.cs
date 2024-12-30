using Modells.ViewModel;
using System.Windows.Controls;

namespace Anicluster.userControls {

    /// <summary>
    /// Interaktionslogik für AnimeMainWindowDisplay.xaml
    /// </summary>
    public partial class AnimeMainWindowDisplay : UserControl {

        public AnimeViewModel Anime { get; set; }

        public AnimeMainWindowDisplay(AnimeViewModel anime) {
            this.Anime = anime;
            InitializeComponent();
            this.DataContext = this;
        }
    }
}
