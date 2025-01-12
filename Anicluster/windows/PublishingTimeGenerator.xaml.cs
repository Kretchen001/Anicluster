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
            InitializeComponent();
            if (pubTime is not null) {
                PublishingTime = pubTime;
                DateTimePickerStart.SelectedDate = PublishingTime.StartDate;
                DateTimePickerEnd.SelectedDate = PublishingTime.EndDate;
                foreach (Interruption x in pubTime.Interruptions) {
                    Interruptions.Add(x);
                }
            }
            else {
                DateTimePickerStart.SelectedDate = new DateTime(2000, 1, 1);
                DateTimePickerEnd.SelectedDate = new DateTime(2000, 1, 1);
            }
            DgInterruptions.ItemsSource = Interruptions;
        }

        private void DateTimePickerStart_SelectedDateChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
            PublishingTime.StartDate = DateTimePickerStart.SelectedDate!.Value;
        }

        private void DateTimePickerEnd_SelectedDateChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
            if (DateTimePickerEnd.SelectedDate!.Value >= DateTimePickerStart.SelectedDate!.Value) {
                PublishingTime.EndDate = DateTimePickerEnd.SelectedDate!.Value;
            }
            else {
                MessageBox.Show("Enddatum kann nicht vorm Startdatum liegen!", "Fehlerhafte Eingabe");
                DateTimePickerStart.SelectedDate = DateTimePickerEnd.SelectedDate!.Value;
                PublishingTime.EndDate = DateTimePickerEnd.SelectedDate!.Value;
            }
        }

        private void BtnAddInterruption_Click(object sender, RoutedEventArgs e) {
            if (DateTimePickerNewInterruptionStart.SelectedDate is not null  // start selected
                && DateTimePickerNewInterruptionEnd.SelectedDate is not null // end selected
                && DateTimePickerNewInterruptionEnd.SelectedDate >= DateTimePickerNewInterruptionStart.SelectedDate) { // end greater then start

                Interruptions.Add(new Interruption(
                    start: DateTimePickerNewInterruptionStart.SelectedDate.Value,
                    end: DateTimePickerNewInterruptionEnd.SelectedDate.Value,
                    comment: null
                ));
            }
        }

        private void Window_Closed(object sender, EventArgs e) {
            PublishingTime.Interruptions.Clear();
            foreach (Interruption x in Interruptions) {
                PublishingTime.Interruptions.Add(x);
            }
        }
    }
}
