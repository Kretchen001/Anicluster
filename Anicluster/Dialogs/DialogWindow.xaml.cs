using Anicluster.Theming;
using System.Windows;

namespace Anicluster.Dialogs {

    /// <summary>
    /// Generisches Fenster für Dialoge. Zeigt das ViewModel aus dem DataContext über das passende DataTemplate an.
    /// </summary>
    public partial class DialogWindow : Window {

        public DialogWindow() {
            this.InitializeComponent();
            WindowThemeHelper.Attach(this);
        }
    }
}
