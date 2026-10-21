using System.Windows.Threading;

namespace Anicluster.ViewModels {

    /// <summary>
    /// Zustand für einen dezenten Ladehinweis. Der Hinweis erscheint erst, wenn ein Vorgang länger als
    /// <see cref="DelayMilliseconds"/> dauert, schnelle Vorgänge bleiben unsichtbar. Mehrere gleichzeitige Vorgänge werden gezählt,
    /// der Hinweis verschwindet, sobald der letzte beendet ist.
    /// </summary>
    public class LoadingNotice : PropertyBaseChange {

        /// <summary>So lange darf ein Vorgang dauern, bevor der Hinweis erscheint</summary>
        public const int DelayMilliseconds = 350;

        private readonly DispatcherTimer _timer;
        private int _activeCount;
        private bool _isVisible;

        /// <summary>Soll der Hinweis gerade angezeigt werden?</summary>
        public bool IsVisible {
            get => this._isVisible;
            private set => this.SetProperty(ref this._isVisible, value);
        }

        public string Text => "Lädt …";

        public LoadingNotice() {
            this._timer = new DispatcherTimer {
                Interval = TimeSpan.FromMilliseconds(DelayMilliseconds)
            };
            this._timer.Tick += this.OnTimerTick;
        }

        /// <summary>Meldet den Beginn eines Vorgangs. Jedes <see cref="Begin"/> braucht ein <see cref="End"/>.</summary>
        public void Begin() {
            this._activeCount++;
            if (this._activeCount == 1) {
                this._timer.Start();
            }
        }

        /// <summary>Meldet das Ende eines Vorgangs.</summary>
        public void End() {
            if (this._activeCount == 0) {
                return;
            }
            this._activeCount--;
            if (this._activeCount == 0) {
                this._timer.Stop();
                this.IsVisible = false;
            }
        }

        private void OnTimerTick(object? sender, EventArgs e) {
            this._timer.Stop();
            if (this._activeCount > 0) {
                this.IsVisible = true;
            }
        }
    }
}
