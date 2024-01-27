using System.Windows;
using System.Windows.Controls;

namespace Anicluster.windows.filter {
    /// <summary>
    /// Interaktionslogik für TagUserControll.xaml
    /// </summary>
    public partial class TagUserControll : UserControl {

        public TagSelection tagSelection { get; set; }

        public bool InclusiveState {
            get { return tagSelection.inclusive; }
            set { tagSelection.inclusive = value; }
        }

        public bool ExclusiveState {
            get { return tagSelection.exclusive; }
            set { tagSelection.exclusive = value; }
        }

        public TagUserControll(TagSelection tagSelection) {
            InitializeComponent();

            this.tagSelection = tagSelection;

            textBloxkTagDesignator.Text = tagSelection.TagDesignator;
        }

        private void clickInclusive(object sender, RoutedEventArgs e) {
            InclusiveState = !InclusiveState;
            if (InclusiveState) {
                tagSelection.exclusive = false;
                checkBoxExclusive.IsChecked = false;
            }
            ((FilterWindow)((Grid)((TreeView)((TreeViewItem) this.Parent).Parent).Parent).Parent).updateFilterData();
        }

        private void clickExclusive(object sender, RoutedEventArgs e) {
            ExclusiveState = !ExclusiveState;
            if (ExclusiveState) {
                tagSelection.inclusive = false;
                checkBoxInclusive.IsChecked = false;
            }
            ((FilterWindow)((Grid)((TreeView)((TreeViewItem)this.Parent).Parent).Parent).Parent).updateFilterData();
        }
    }
}