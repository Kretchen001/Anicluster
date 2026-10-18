using Anicluster.Dialogs;
using System.Windows;

namespace Anicluster.ViewModels {

    /// <summary>
    /// Dialog zur Eingabe einer Zahl zwischen 0 und 100.
    /// </summary>
    public class NumberInputViewModel : DialogViewModelBase {

        private readonly IDialogService _dialogService;
        private string _inputText = string.Empty;

        public override string Title => "Zahleneingabe";

        public string InputText {
            get => this._inputText;
            set => this.SetProperty(ref this._inputText, value);
        }

        /// <summary>Die bestätigte Eingabe, bis dahin der Startwert.</summary>
        public int Result { get; private set; }

        public NumberInputViewModel(IDialogService dialogService, int initialValue = 0) {
            this._dialogService = dialogService;
            this.Result = initialValue;
        }

        /// <summary>Prüft die Eingabe und schließt den Dialog, wenn sie gültig ist.</summary>
        public void Confirm() {
            if (int.TryParse(this.InputText, out int number) && number >= ScoreViewModel.MinValue && number <= ScoreViewModel.MaxValue) {
                this.Result = number;
                this.RequestClose(true);
            }
            else {
                this._dialogService.ShowMessage("Bitte geben Sie eine gültige Zahl unter 100 ein.", string.Empty, MessageBoxImage.Warning);
            }
        }
    }
}
