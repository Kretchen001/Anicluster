using System.Windows.Controls;

namespace Anicluster.Views {

    /// <summary>
    /// Auswahl des Tiers (S bis E). Rein deklarativ über Bindings an die Property "Tier" des DataContext.
    /// </summary>
    public partial class TierPickerView : UserControl {

        public TierPickerView() {
            this.InitializeComponent();
        }
    }
}
