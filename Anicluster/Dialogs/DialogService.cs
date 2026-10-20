using Anicluster.ViewModels;
using System.Windows;

namespace Anicluster.Dialogs {

    /// <summary>
    /// Standard-Implementierung: Jedes Dialog-ViewModel wird in einem <see cref="DialogWindow"/> angezeigt,
    /// die passende View wird über die DataTemplates in <c>Resources/DataTemplates.xaml</c> gefunden.
    /// </summary>
    public class DialogService : IDialogService {

        public bool? ShowDialog(DialogViewModelBase viewModel) {
            // Der Owner muss vor dem Erzeugen des neuen Fensters ermittelt werden
            Window? owner = this.GetActiveWindow();
            DialogWindow window = new DialogWindow {
                DataContext = viewModel,
                Owner = owner
            };

            void OnCloseRequested(object? sender, bool dialogResult) {
                window.DialogResult = dialogResult;
            }

            viewModel.CloseRequested += OnCloseRequested;
            try {
                return window.ShowDialog();
            }
            finally {
                viewModel.CloseRequested -= OnCloseRequested;
            }
        }

        public int? RequestNumber(int initialValue = 0) {
            NumberInputViewModel viewModel = new NumberInputViewModel(this, initialValue);
            return this.ShowDialog(viewModel) == true ? viewModel.Result : null;
        }

        public void ShowMessage(string message, string title = "", MessageBoxImage image = MessageBoxImage.None) {
            Window? owner = this.GetActiveWindow();
            if (owner is null) {
                MessageBox.Show(message, title, MessageBoxButton.OK, image);
            }
            else {
                MessageBox.Show(owner, message, title, MessageBoxButton.OK, image);
            }
        }

        public bool Confirm(string message, string title) {
            Window? owner = this.GetActiveWindow();
            MessageBoxResult result = owner is null
                ? MessageBox.Show(message, title, MessageBoxButton.OKCancel, MessageBoxImage.Warning)
                : MessageBox.Show(owner, message, title, MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            return result == MessageBoxResult.OK;
        }

        /// <summary>Das aktuell aktive Fenster, ersatzweise das Hauptfenster.</summary>
        private Window? GetActiveWindow() {
            return Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive)
                ?? Application.Current.MainWindow;
        }
    }
}
