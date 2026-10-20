using Anicluster.Dialogs;

namespace Anicluster.ViewModels {

    /// <summary>
    /// Ein Bewertungswert von 0 bis 100. Wird von allen Bewertungsbalken verwendet
    /// (Geschichte, Animation, Spezialeffekte, Soundtrack, Musikstücke, Syncros).
    /// </summary>
    public class ScoreViewModel : PropertyBaseChange {

        public const int MinValue = 0;
        public const int MaxValue = 100;

        private readonly IDialogService _dialogService;
        private int _value;

        /// <summary>Aktueller Wert, wird automatisch auf 0 bis 100 begrenzt.</summary>
        public int Value {
            get => this._value;
            set {
                if (this.SetProperty(ref this._value, Math.Clamp(value, MinValue, MaxValue))) {
                    this.ValueChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>Wird ausgelöst, wenn sich <see cref="Value"/> geändert hat.</summary>
        public event EventHandler? ValueChanged;

        public ScoreViewModel(IDialogService dialogService, int initialValue = 0) {
            this._dialogService = dialogService;
            this._value = Math.Clamp(initialValue, MinValue, MaxValue);
        }

        /// <summary>Verändert den Wert relativ, z. B. per Mausrad.</summary>
        public void Adjust(int delta) {
            this.Value += delta;
        }

        /// <summary>Fragt den Wert per Zahleneingabe-Dialog ab.</summary>
        public void PromptForValue() {
            int? input = this._dialogService.RequestNumber(this.Value);
            if (input.HasValue) {
                this.Value = input.Value;
            }
        }
    }
}
