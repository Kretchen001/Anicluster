using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Anicluster.windows.filter {
    /// <summary>
    /// Interaktionslogik für TagUserControll.xaml
    /// </summary>
    public partial class TagUserControll : UserControl {

        public TagSelection tagSelection { get; set; }
        public TagUserControll(TagSelection tagSelection) {
            InitializeComponent();

            this.tagSelection = tagSelection;

            labelTagDesignator.Content = tagSelection.tagDesignator;
        }
    }
}
