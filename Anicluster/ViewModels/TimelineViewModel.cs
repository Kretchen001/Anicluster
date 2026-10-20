using Anicluster.Database.Services;
using Anicluster.Timeline;
using Modells.Anime;
using System.Collections.ObjectModel;

namespace Anicluster.ViewModels {

    /// <summary>
    /// Der Zeitstrahl: alle Animes mit eingetragenem Veröffentlichungszeitraum als Balken auf einer gemeinsamen Zeitachse.
    /// Die Pixelpositionen berechnet <see cref="TimelineLayout"/>, dieses ViewModel hält Zoom, Suche und Farbwahl.
    /// </summary>
    public class TimelineViewModel : PropertyBaseChange {

        /// <summary>Höhe einer Zeile in Pixeln (muss zur View passen)</summary>
        public const double RowHeight = 32.0;

        /// <summary>Höhe der Zeitachse oben in Pixeln (muss zur View passen)</summary>
        public const double HeaderHeight = 30.0;

        public const double MinPixelsPerYear = 20.0;
        public const double MaxPixelsPerYear = 400.0;

        private readonly List<TimelineEntry> _entries;
        private double _pixelsPerYear = 120.0;
        private string _searchText = string.Empty;
        private bool _colorByTier = true;
        private double _width;
        private double _todayLeft;
        private int _withoutPeriodCount;

        /// <summary>Die sichtbaren Zeilen, früheste Veröffentlichung zuerst</summary>
        public ObservableCollection<TimelineRowViewModel> Rows { get; } = [];

        /// <summary>Die Jahresmarken der Zeitachse</summary>
        public ObservableCollection<TimelineTick> Ticks { get; } = [];

        /// <summary>Wird ausgelöst, wenn ein Anime (Id) zum Anzeigen/Bearbeiten geöffnet werden soll.</summary>
        public event EventHandler<int>? EditRequested;

        /// <summary>Wird nach einer Änderung des Zooms ausgelöst; der Wert ist das Verhältnis neuer zu alter Zoom.</summary>
        public event EventHandler<double>? ZoomChanged;

        public double PixelsPerYear {
            get => this._pixelsPerYear;
            set {
                double clamped = Math.Clamp(value, MinPixelsPerYear, MaxPixelsPerYear);
                double old = this._pixelsPerYear;
                if (this.SetProperty(ref this._pixelsPerYear, clamped)) {
                    this.Rebuild();
                    this.ZoomChanged?.Invoke(this, clamped / old);
                }
            }
        }

        public string SearchText {
            get => this._searchText;
            set {
                if (this.SetProperty(ref this._searchText, value)) {
                    this.Rebuild();
                }
            }
        }

        public bool ColorByTier {
            get => this._colorByTier;
            set {
                if (this.SetProperty(ref this._colorByTier, value)) {
                    this.OnPropertyChanged(nameof(this.ColorByState));
                    this.Rebuild();
                }
            }
        }

        public bool ColorByState {
            get => !this._colorByTier;
            set => this.ColorByTier = !value;
        }

        /// <summary>Gesamtbreite der Zeitachse in Pixeln</summary>
        public double Width {
            get => this._width;
            private set => this.SetProperty(ref this._width, value);
        }

        /// <summary>Gesamthöhe aller Zeilen in Pixeln</summary>
        public double BodyHeight => this.Rows.Count * RowHeight;

        /// <summary>Position der Heute-Linie in Pixeln</summary>
        public double TodayLeft {
            get => this._todayLeft;
            private set => this.SetProperty(ref this._todayLeft, value);
        }

        public bool HasRows => this.Rows.Count > 0;

        public bool HasNoRows => !this.HasRows;

        /// <summary>Kurzinfo neben dem Titel, z. B. „24 Animes · 3 ohne Zeitraum“</summary>
        public string InfoText {
            get {
                string text = $"{this.Rows.Count} {(this.Rows.Count == 1 ? "Anime" : "Animes")}";
                return this._withoutPeriodCount > 0 ? $"{text} · {this._withoutPeriodCount} ohne Zeitraum" : text;
            }
        }

        /// <summary>Hinweis, wenn nichts angezeigt werden kann</summary>
        public string EmptyText => this._searchText.Trim().Length > 0
            ? "Kein Anime mit diesem Namen hat einen Veröffentlichungszeitraum."
            : "Noch kein Anime hat einen Veröffentlichungszeitraum. Trage ihn im Editor bei den Medieninfos oder den Staffeln ein.";

        public TimelineViewModel(IAnimeService animeService) {
            this._entries = animeService.SelectTimelineEntries();
            this.Rebuild();
        }

        /// <summary>Vergrößert (Faktor über 1) oder verkleinert (unter 1) den Zoom, z. B. per Strg + Mausrad.</summary>
        public void ZoomBy(double factor) {
            this.PixelsPerYear *= factor;
        }

        private void Rebuild() {
            TimelineLayoutResult layout = TimelineLayout.Calculate(this._entries, this._pixelsPerYear, DateTime.Today, this._searchText);
            this.Rows.Clear();
            foreach (TimelineRowLayout rowLayout in layout.Rows) {
                TimelineRowViewModel row = new TimelineRowViewModel(rowLayout, this._colorByTier);
                row.EditRequested += (sender, e) => this.EditRequested?.Invoke(this, row.Id);
                this.Rows.Add(row);
            }
            this.Ticks.Clear();
            foreach (TimelineTick tick in layout.Ticks) {
                this.Ticks.Add(tick);
            }
            this._withoutPeriodCount = layout.WithoutPeriodCount;
            this.Width = layout.Width;
            this.TodayLeft = layout.TodayLeft;
            this.OnPropertyChanged(nameof(this.BodyHeight));
            this.OnPropertyChanged(nameof(this.HasRows));
            this.OnPropertyChanged(nameof(this.HasNoRows));
            this.OnPropertyChanged(nameof(this.InfoText));
            this.OnPropertyChanged(nameof(this.EmptyText));
        }
    }
}
