using Anicluster.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Anicluster.Views {

    /// <summary>
    /// Geschmacksprofil. Der Code-Behind leitet nur Klicks an das <see cref="TasteProfileViewModel"/> weiter.
    /// </summary>
    public partial class TasteProfileView : UserControl {

        private TasteProfileViewModel? ViewModel => this.DataContext as TasteProfileViewModel;

        public TasteProfileView() {
            this.InitializeComponent();
        }

        private void BtnShowTag_Click(object sender, RoutedEventArgs e) {
            if (sender is FrameworkElement { DataContext: TasteTagItemViewModel tag }) {
                this.ViewModel?.ShowInLibrary(tag);
            }
        }

        private void BtnShowSuggestions_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.ShowSuggestionsInLibrary();
        }

        /// <summary>Doppelklick auf einen Vorschlag öffnet den Anime.</summary>
        private void Suggestion_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (e.ClickCount == 2 && sender is FrameworkElement { DataContext: TasteSuggestionViewModel suggestion }) {
                suggestion.RequestEdit();
                e.Handled = true;
            }
        }
    }
}
