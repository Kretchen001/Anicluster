using Anicluster.Dialogs;
using Modells.Anime;
using System.Collections.ObjectModel;
using System.Windows;

namespace Anicluster.ViewModels {

    /// <summary>
    /// Dialog zum Bearbeiten des Veröffentlichungszeitraums inklusive Unterbrechungen.
    /// Arbeitet auf Kopien, das Ergebnis steht nach dem Übernehmen in <see cref="Result"/>.
    /// </summary>
    public class PublishingTimeEditorViewModel : DialogViewModelBase {

        /// <summary>Platzhalter-Datum des Modells für "noch nicht gesetzt"</summary>
        private static readonly DateTime UnsetDate = new DateTime(1, 1, 1);

        private readonly IDialogService _dialogService;
        private readonly int _publishingTimeId;
        private DateTime? _startDate;
        private DateTime? _endDate;
        private DateTime? _newInterruptionStart;
        private DateTime? _newInterruptionEnd;

        public override string Title => "Veröffentlichungszeitraum";

        public override bool FillsOwnerWindow => true;

        public DateTime? StartDate {
            get => this._startDate;
            set => this.SetProperty(ref this._startDate, value);
        }

        public DateTime? EndDate {
            get => this._endDate;
            set => this.SetProperty(ref this._endDate, value);
        }

        public DateTime? NewInterruptionStart {
            get => this._newInterruptionStart;
            set => this.SetProperty(ref this._newInterruptionStart, value);
        }

        public DateTime? NewInterruptionEnd {
            get => this._newInterruptionEnd;
            set => this.SetProperty(ref this._newInterruptionEnd, value);
        }

        public ObservableCollection<Interruption> Interruptions { get; } = [];

        /// <summary>Der übernommene Zeitraum, <c>null</c> solange nicht übernommen wurde.</summary>
        public PublishingTime? Result { get; private set; }

        public PublishingTimeEditorViewModel(IDialogService dialogService, PublishingTime publishingTime) {
            this._dialogService = dialogService;
            this._publishingTimeId = publishingTime.Id;
            this._startDate = publishingTime.StartDate == UnsetDate ? null : publishingTime.StartDate;
            this._endDate = publishingTime.EndDate == UnsetDate ? null : publishingTime.EndDate;
            foreach (Interruption interruption in publishingTime.Interruptions) {
                this.Interruptions.Add(new Interruption(interruption.Start, interruption.End, interruption.Comment) { Id = interruption.Id });
            }
        }

        /// <summary>Fügt die Unterbrechung aus den Eingabefeldern hinzu.</summary>
        public void AddInterruption() {
            if (this.NewInterruptionStart is null || this.NewInterruptionEnd is null || this.NewInterruptionEnd < this.NewInterruptionStart) {
                this._dialogService.ShowMessage(
                    "Bitte Start- und Enddatum der Unterbrechung angeben, das Ende darf nicht vor dem Start liegen.",
                    "Fehlerhafte Eingabe",
                    MessageBoxImage.Warning
                );
                return;
            }
            this.Interruptions.Add(new Interruption(this.NewInterruptionStart.Value, this.NewInterruptionEnd.Value, null));
        }

        public void Accept() {
            if (this.StartDate is not null && this.EndDate is not null && this.EndDate < this.StartDate) {
                this._dialogService.ShowMessage("Enddatum kann nicht vorm Startdatum liegen!", "Fehlerhafte Eingabe", MessageBoxImage.Warning);
                return;
            }
            this.Result = new PublishingTime() {
                Id = this._publishingTimeId,
                StartDate = this.StartDate ?? UnsetDate,
                EndDate = this.EndDate ?? UnsetDate,
                Interruptions = new List<Interruption>(this.Interruptions)
            };
            this.RequestClose(true);
        }

        public void Cancel() {
            this.RequestClose(false);
        }
    }
}
