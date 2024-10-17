using Anicluster.windows;
using System.Windows;
using System.Windows.Input;

namespace Anicluster {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        public MainWindow() {
            InitializeComponent();
            CommandBindings.Add(new CommandBinding(ApplicationCommands.New, OpenAddNewAnime)); // bound Strg + N
        }

        private void OpenAddNewAnime(object sender, ExecutedRoutedEventArgs? e) {
            AddNewAnime addNewAnime = new AddNewAnime();
            addNewAnime.ShowDialog();
        }

        private void MenuItemNewAnime_Click(object sender, RoutedEventArgs e) {
            OpenAddNewAnime(sender, null);
        }
    }
}
