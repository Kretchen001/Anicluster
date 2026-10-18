using Anicluster.Dialogs;
using Modells.Anime;
using System.Collections.ObjectModel;
using System.Windows;

namespace Anicluster.ViewModels {

    /// <summary>
    /// Dialog zum Verwalten der Staffeln eines Animes (hinzufügen, entfernen, per Drag &amp; Drop umsortieren).
    /// Arbeitet auf Kopien, das Ergebnis steht nach dem Übernehmen in <see cref="Result"/>.
    /// </summary>
    public class SeasonEditorViewModel : DialogViewModelBase {

        private readonly IDialogService _dialogService;
        private string _episodeCountText = string.Empty;
        private string _startYearText = string.Empty;
        private string _endYearText = string.Empty;
        private string _comment = string.Empty;
        private Season? _selectedSeason;

        public override string Title => "Staffel Generator";

        public ObservableCollection<Season> Seasons { get; } = [];

        public string EpisodeCountText {
            get => this._episodeCountText;
            set => this.SetProperty(ref this._episodeCountText, value);
        }

        public string StartYearText {
            get => this._startYearText;
            set => this.SetProperty(ref this._startYearText, value);
        }

        public string EndYearText {
            get => this._endYearText;
            set => this.SetProperty(ref this._endYearText, value);
        }

        public string Comment {
            get => this._comment;
            set => this.SetProperty(ref this._comment, value);
        }

        public Season? SelectedSeason {
            get => this._selectedSeason;
            set => this.SetProperty(ref this._selectedSeason, value);
        }

        /// <summary>Die übernommenen Staffeln, leer solange nicht übernommen wurde.</summary>
        public List<Season> Result { get; private set; } = [];

        public SeasonEditorViewModel(IDialogService dialogService, IEnumerable<Season> seasons) {
            this._dialogService = dialogService;
            foreach (Season season in seasons) {
                this.Seasons.Add(new Season(season.Id, season.Number, season.Comment) {
                    Episodes = new List<VideoAnimation>(season.Episodes),
                    PublishingTime = season.PublishingTime
                });
            }
        }

        /// <summary>Legt aus den Eingabefeldern eine neue Staffel an.</summary>
        public void AddSeason() {
            if (!int.TryParse(this.EpisodeCountText, out int episodeCount) || episodeCount < 0) {
                this._dialogService.ShowMessage("Bitte eine gültige Anzahl der Episoden eingeben!", "Fehlerhafte Eingabe", MessageBoxImage.Error);
                return;
            }
            int startYear = ParseOrDefault(this.StartYearText);
            int endYear = ParseOrDefault(this.EndYearText);
            if (startYear > endYear) {
                this._dialogService.ShowMessage("Start- und Endjahr sind in der Konstellation unmöglich!", "Fehlerhafte Eingabe", MessageBoxImage.Error);
                return;
            }
            if (startYear < 1 || endYear > 9999) {
                this._dialogService.ShowMessage("Start- und Endjahr müssen zwischen 1 und 9999 liegen!", "Fehlerhafte Eingabe", MessageBoxImage.Error);
                return;
            }

            List<VideoAnimation> episodes = [];
            for (int i = 0; i < episodeCount; i += 1) {
                episodes.Add(new VideoAnimation(VideoType.Episode, null));
            }
            this.Seasons.Add(new Season() {
                Number = this.Seasons.Count + 1,
                Episodes = episodes,
                PublishingTime = new PublishingTime() {
                    StartDate = new DateTime(startYear, 1, 1),
                    EndDate = new DateTime(endYear, 1, 1)
                },
                Comment = string.IsNullOrWhiteSpace(this.Comment) ? null : this.Comment
            });
        }

        /// <summary>Entfernt die markierte Staffel nach Rückfrage und nummeriert neu.</summary>
        public void RemoveSelectedSeason() {
            if (this.SelectedSeason is null) {
                return;
            }
            bool confirmed = this._dialogService.Confirm(
                "Beim Löschen wird die Staffel entfernt, der Rest jedoch nicht nachgezogen! " +
                "Beim Ersetzen bitte die passende an die entsprechende Stelle ziehen.",
                "Warnung"
            );
            if (confirmed) {
                this.Seasons.Remove(this.SelectedSeason);
                this.UpdateNumbers();
            }
        }

        /// <summary>Verschiebt eine Staffel (Drag &amp; Drop) an die angegebene Position.</summary>
        public void MoveSeason(Season season, int targetIndex) {
            this.Seasons.Remove(season);
            this.Seasons.Insert(Math.Clamp(targetIndex, 0, this.Seasons.Count), season);
            this.UpdateNumbers();
        }

        public void Accept() {
            this.Result = new List<Season>(this.Seasons);
            this.RequestClose(true);
        }

        public void Cancel() {
            this.RequestClose(false);
        }

        /// <summary>
        /// Vergibt die Nummern entsprechend der Reihenfolge neu. <see cref="Season"/> meldet Änderungen nicht selbst,
        /// deshalb wird die Liste neu aufgebaut, damit die View die neuen Nummern anzeigt.
        /// </summary>
        private void UpdateNumbers() {
            List<Season> ordered = this.Seasons.ToList();
            for (int i = 0; i < ordered.Count; i += 1) {
                ordered[i].Number = i + 1;
            }
            this.Seasons.Clear();
            foreach (Season season in ordered) {
                this.Seasons.Add(season);
            }
        }

        private static int ParseOrDefault(string input) {
            return int.TryParse(input, out int result) ? result : 1;
        }
    }
}
