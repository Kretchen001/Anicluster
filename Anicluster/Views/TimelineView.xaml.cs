using Anicluster.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Anicluster.Views {

    /// <summary>
    /// Zeitstrahl. Der Code-Behind leitet den Doppelklick auf eine Zeile weiter und hält die Jahresleiste und die Namen
    /// synchron zum Hauptbereich: Scrollen, Umschalt/Strg + Mausrad und das Beibehalten der Mitte beim Zoomen.
    /// </summary>
    public partial class TimelineView : UserControl {

        /// <summary>Zoomfaktor pro Rasterschritt des Mausrads</summary>
        private const double WheelZoomFactor = 1.15;

        private TimelineViewModel? _subscribedViewModel;

        private TimelineViewModel? ViewModel => this.DataContext as TimelineViewModel;

        public TimelineView() {
            this.InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            this._subscribedViewModel = this.ViewModel;
            if (this._subscribedViewModel is not null) {
                this._subscribedViewModel.ZoomChanged += this.OnZoomChanged;
                this.ScrollToToday();
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e) {
            if (this._subscribedViewModel is not null) {
                this._subscribedViewModel.ZoomChanged -= this.OnZoomChanged;
                this._subscribedViewModel = null;
            }
        }

        /// <summary>Zeigt zu Beginn den Bereich um „Heute“ (rechts im Bild), damit die neuesten Animes sichtbar sind.</summary>
        private void ScrollToToday() {
            if (this.ViewModel is null) {
                return;
            }
            this.UpdateLayout();
            this.MainScroll.ScrollToHorizontalOffset(this.ViewModel.TodayLeft - this.MainScroll.ViewportWidth * 0.8);
        }

        /// <summary>Beim Zoomen bleibt die Mitte des sichtbaren Bereichs an ihrem Datum.</summary>
        private void OnZoomChanged(object? sender, double ratio) {
            double center = this.MainScroll.HorizontalOffset + this.MainScroll.ViewportWidth / 2;
            this.UpdateLayout();
            this.MainScroll.ScrollToHorizontalOffset(center * ratio - this.MainScroll.ViewportWidth / 2);
        }

        /// <summary>Jahresleiste und Namen folgen dem Hauptbereich.</summary>
        private void MainScroll_ScrollChanged(object sender, ScrollChangedEventArgs e) {
            if (e.HorizontalChange != 0) {
                this.HeaderScroll.ScrollToHorizontalOffset(e.HorizontalOffset);
            }
            if (e.VerticalChange != 0) {
                this.NamesScroll.ScrollToVerticalOffset(e.VerticalOffset);
            }
        }

        /// <summary>Mausrad über den Namen scrollt den Hauptbereich senkrecht.</summary>
        private void NamesScroll_PreviewMouseWheel(object sender, MouseWheelEventArgs e) {
            this.MainScroll.ScrollToVerticalOffset(this.MainScroll.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        /// <summary>Strg + Mausrad zoomt, Umschalt + Mausrad scrollt waagerecht, das Mausrad allein senkrecht.</summary>
        private void MainScroll_PreviewMouseWheel(object sender, MouseWheelEventArgs e) {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) {
                this.ViewModel?.ZoomBy(e.Delta > 0 ? WheelZoomFactor : 1 / WheelZoomFactor);
                e.Handled = true;
            }
            else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)) {
                this.MainScroll.ScrollToHorizontalOffset(this.MainScroll.HorizontalOffset - e.Delta);
                e.Handled = true;
            }
        }

        private void Row_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (e.ClickCount == 2 && sender is FrameworkElement { DataContext: TimelineRowViewModel row }) {
                row.RequestEdit();
                e.Handled = true;
            }
        }
    }
}
