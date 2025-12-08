using Modells.Anime;
using System.Collections.ObjectModel;
using System.Windows;

namespace Anicluster.windows {

    /// <summary>
    /// Interaktionslogik für PublishingTimeGenerator.xaml
    /// </summary>
    public partial class PublishingTimeGenerator : Window {

        public PublishingTime PublishingTime { get; set; } = new PublishingTime();
        private ObservableCollection<Interruption> Interruptions { get; set; } = new ObservableCollection<Interruption>();

        public PublishingTimeGenerator(PublishingTime? pubTime = null) {
            this.InitializeComponent();
            if (pubTime is not null) {
                this.PublishingTime = pubTime;
                this.DateTimePickerStart.SelectedDate = this.PublishingTime.StartDate;
                this.DateTimePickerEnd.SelectedDate = this.PublishingTime.EndDate;
                foreach (Interruption x in pubTime.Interruptions) {
                    this.Interruptions.Add(x);
                }
            }
            else {
                this.DateTimePickerStart.SelectedDate = new DateTime(2000, 1, 1);
                this.DateTimePickerEnd.SelectedDate = new DateTime(2000, 1, 1);
            }
            this.DgInterruptions.ItemsSource = this.Interruptions;
        }

        private void DateTimePickerStart_SelectedDateChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
            this.PublishingTime.StartDate = this.DateTimePickerStart.SelectedDate!.Value;
        }

        private void DateTimePickerEnd_SelectedDateChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
            if (this.DateTimePickerEnd.SelectedDate!.Value >= this.DateTimePickerStart.SelectedDate!.Value) {
                this.PublishingTime.EndDate = this.DateTimePickerEnd.SelectedDate!.Value;
            }
            else {
                MessageBox.Show("Enddatum kann nicht vorm Startdatum liegen!", "Fehlerhafte Eingabe");
                this.DateTimePickerStart.SelectedDate = this.DateTimePickerEnd.SelectedDate!.Value;
                this.PublishingTime.EndDate = this.DateTimePickerEnd.SelectedDate!.Value;
            }
        }

        private void BtnAddInterruption_Click(object sender, RoutedEventArgs e) {
            if (this.DateTimePickerNewInterruptionStart.SelectedDate is not null  // start selected
                && this.DateTimePickerNewInterruptionEnd.SelectedDate is not null // end selected
                && this.DateTimePickerNewInterruptionEnd.SelectedDate >= this.DateTimePickerNewInterruptionStart.SelectedDate) { // end greater then start

                this.Interruptions.Add(new Interruption(
                    start: this.DateTimePickerNewInterruptionStart.SelectedDate.Value,
                    end: this.DateTimePickerNewInterruptionEnd.SelectedDate.Value,
                    comment: null
                ));
            }
        }

        private void Window_Closed(object sender, EventArgs e) {
            this.PublishingTime.Interruptions.Clear();
            foreach (Interruption x in this.Interruptions) {
                this.PublishingTime.Interruptions.Add(x);
            }
        }
    }
}
