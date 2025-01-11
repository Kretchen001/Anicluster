using Modells.Anime;
using System.Collections.ObjectModel;
using System.Windows;

namespace Anicluster.windows {
    /// <summary>
    /// Interaktionslogik für PublishingTimeGenerator.xaml
    /// </summary>
    public partial class PublishingTimeGenerator : Window {

        public PublishingTime PublishingTime { get; set; } = new PublishingTime();

        public PublishingTimeGenerator(PublishingTime? pubTime = null) {
            InitializeComponent();
            if (pubTime is not null) {
                PublishingTime = pubTime;
            }
            this.DataContext = PublishingTime;
            DateTimePickerStart.SelectedDate = new DateTime(2000,1,1);
            DateTimePickerEnd.SelectedDate = new DateTime(2000, 1, 1);
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

        private void DateTimePickerNewInterruptionStart_SelectedDateChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {

        }

        private void DateTimePickerNewInterruptionEnd_SelectedDateChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {

        }
    }
}
