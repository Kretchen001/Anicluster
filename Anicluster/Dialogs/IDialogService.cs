using Anicluster.ViewModels;
using System.Windows;

namespace Anicluster.Dialogs {

    /// <summary>
    /// Zeigt Dialoge und Meldungen an, ohne dass die ViewModels Fenster kennen müssen.
    /// </summary>
    public interface IDialogService {

        /// <summary>Zeigt das ViewModel modal in einem Dialogfenster. <c>null</c>, wenn der Dialog über das Fenster-X geschlossen wurde.</summary>
        bool? ShowDialog(DialogViewModelBase viewModel);

        /// <summary>Fragt eine Zahl zwischen 0 und 100 ab. <c>null</c>, wenn abgebrochen wurde.</summary>
        int? RequestNumber(int initialValue = 0);

        void ShowMessage(string message, string title = "", MessageBoxImage image = MessageBoxImage.None);

        /// <summary>Fragt per OK/Abbrechen nach. <c>true</c> bei OK.</summary>
        bool Confirm(string message, string title);
    }
}
