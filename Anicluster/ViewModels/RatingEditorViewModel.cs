using Anicluster.Dialogs;
using Modells.Anime;
using Modells.Anime.SoundRating;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Anicluster.ViewModels {

    /// <summary>
    /// Dialog zum Bewerten eines Animes (Geschichte, Animation, Spezialeffekte und Akustik).
    /// Arbeitet auf Kopien, das Ergebnis steht nach dem Übernehmen in <see cref="Result"/>.
    /// </summary>
    public class RatingEditorViewModel : DialogViewModelBase {

        private readonly IDialogService _dialogService;
        private readonly int _ratingId;
        private readonly int _acousticId;
        private string? _commentAcoustic;

        public override string Title => "Bewertung";

        public override bool FillsOwnerWindow => true;

        public ScoreViewModel Story { get; }
        public ScoreViewModel Animation { get; }
        public ScoreViewModel SpecialEffects { get; }
        public ScoreViewModel Soundtrack { get; }

        public ObservableCollection<MusicPieceViewModel> Openings { get; } = [];
        public ObservableCollection<MusicPieceViewModel> Endings { get; } = [];

        /// <summary>Einzelne Soundtracks. Gibt es welche, ist der Soundtrack-Wert deren Durchschnitt (wie in <see cref="Acoustic.Soundtrack"/>).</summary>
        public ObservableCollection<MusicPieceViewModel> SoundtrackPieces { get; } = [];
        public ObservableCollection<SyncroViewModel> Syncros { get; } = [];

        public string? CommentAcoustic {
            get => this._commentAcoustic;
            set => this.SetProperty(ref this._commentAcoustic, value);
        }

        /// <summary>Formel mit den aktuellen Eingaben (LaTeX für WpfMath)</summary>
        public string FormulaText { get; private set; } = string.Empty;

        /// <summary>Aufschlüsselung der Akustik-Berechnung</summary>
        public string AcousticSummaryText { get; private set; } = string.Empty;

        /// <summary>Die übernommene Bewertung, <c>null</c> solange nicht übernommen wurde.</summary>
        public Rating? Result { get; private set; }

        /// <param name="rating">Vorhandene Bewertung, <c>null</c> für eine neue</param>
        public RatingEditorViewModel(IDialogService dialogService, Rating? rating = null) {
            this._dialogService = dialogService;
            this._ratingId = rating?.Id ?? 0;
            this._acousticId = rating?.Acoustic.Id ?? 0;
            this._commentAcoustic = rating?.Acoustic.Comment;

            this.Story = this.CreateScore(rating?.Story ?? 0);
            this.Animation = this.CreateScore(rating?.Animation ?? 0);
            this.SpecialEffects = this.CreateScore(rating?.SpecialEffects ?? 0);
            // Ein nie gesetzter Soundtrack liefert -1, das wird als 0 dargestellt
            this.Soundtrack = this.CreateScore(Math.Max(rating?.Acoustic.Soundtrack ?? 0, 0));

            if (rating is not null) {
                foreach (MusicPiece piece in rating.Acoustic.Opening) {
                    this.AddPiece(this.Openings, new MusicPieceViewModel(this._dialogService, piece.Type, piece));
                }
                foreach (MusicPiece piece in rating.Acoustic.Ending) {
                    this.AddPiece(this.Endings, new MusicPieceViewModel(this._dialogService, piece.Type, piece));
                }
                foreach (MusicPiece piece in rating.Acoustic.Sounds) {
                    this.AddPiece(this.SoundtrackPieces, new MusicPieceViewModel(this._dialogService, piece.Type, piece));
                }
                foreach (Syncro syncro in rating.Acoustic.Syncro) {
                    this.AddSyncro(new SyncroViewModel(this._dialogService, syncro));
                }
            }

            this.Openings.CollectionChanged += this.OnCollectionChanged;
            this.Endings.CollectionChanged += this.OnCollectionChanged;
            this.SoundtrackPieces.CollectionChanged += this.OnCollectionChanged;
            this.Syncros.CollectionChanged += this.OnCollectionChanged;
            this.UpdatePreview();
        }

        public void AddOpening() {
            this.AddPiece(this.Openings, new MusicPieceViewModel(this._dialogService, MusicPieceType.Opening));
        }

        public void AddEnding() {
            this.AddPiece(this.Endings, new MusicPieceViewModel(this._dialogService, MusicPieceType.Ending));
        }

        public void AddSoundtrack() {
            this.AddPiece(this.SoundtrackPieces, new MusicPieceViewModel(this._dialogService, MusicPieceType.Other));
        }

        public void AddSyncro() {
            this.AddSyncro(new SyncroViewModel(this._dialogService));
        }

        /// <summary>Berechnet den Soundtrack-Wert als Durchschnitt der einzeln bewerteten Soundtracks.</summary>
        public void CalculateSoundtrack() {
            if (this.SoundtrackPieces.Count == 0) {
                this._dialogService.ShowMessage("Keine Soundtracks zum Berechnen eingetragen!");
                return;
            }
            this.Soundtrack.Value = this.SoundtrackPieces.Sum(x => x.Score.Value) / this.SoundtrackPieces.Count;
        }

        /// <summary>Übernimmt die Bewertung und schließt den Dialog.</summary>
        public void Accept() {
            this.Result = this.BuildRating();
            this.RequestClose(true);
        }

        public void Cancel() {
            this.RequestClose(false);
        }

        private ScoreViewModel CreateScore(int initialValue) {
            ScoreViewModel score = new ScoreViewModel(this._dialogService, initialValue);
            score.ValueChanged += (sender, e) => this.UpdatePreview();
            return score;
        }

        private void AddPiece(ObservableCollection<MusicPieceViewModel> target, MusicPieceViewModel piece) {
            piece.RemoveRequested += (sender, e) => target.Remove(piece);
            piece.Score.ValueChanged += (sender, e) => this.UpdatePreview();
            target.Add(piece);
        }

        private void AddSyncro(SyncroViewModel syncro) {
            syncro.RemoveRequested += (sender, e) => this.Syncros.Remove(syncro);
            syncro.Score.ValueChanged += (sender, e) => this.UpdatePreview();
            syncro.PropertyChanged += (sender, e) => this.UpdatePreview();
            this.Syncros.Add(syncro);
        }

        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
            this.UpdatePreview();
        }

        /// <summary>Baut aus dem aktuellen Eingabestand eine Bewertung (die Ids der Vorlage bleiben erhalten).</summary>
        private Rating BuildRating() {
            Acoustic acoustic = new Acoustic(
                id: this._acousticId,
                opening: this.Openings.Select(x => x.ToModel()).ToList(),
                ending: this.Endings.Select(x => x.ToModel()).ToList(),
                sounds: this.SoundtrackPieces.Select(x => x.ToModel()).ToList(),
                soundtrack: this.Soundtrack.Value,
                syncros: this.Syncros.Select(x => x.ToModel()).ToList(),
                comment: this.CommentAcoustic
            );
            return new Rating(
                id: this._ratingId,
                story: this.Story.Value,
                animation: this.Animation.Value,
                specialEffects: this.SpecialEffects.Value,
                acoustic: acoustic,
                isRated: true
            );
        }

        /// <summary>Aktualisiert die Formel und die Akustik-Aufschlüsselung.</summary>
        private void UpdatePreview() {
            // Mit einzelnen Soundtracks ist deren Durchschnitt der Soundtrack-Wert
            if (this.SoundtrackPieces.Count > 0) {
                int average = Average(this.SoundtrackPieces.Select(x => x.Score.Value).ToList());
                if (this.Soundtrack.Value != average) {
                    this.Soundtrack.Value = average; // löst die Aktualisierung erneut aus
                    return;
                }
            }
            Rating rating = this.BuildRating();
            this.FormulaText = $"\\frac{{{rating.Story} \\cdot 40 + {rating.Animation} \\cdot 25 + {rating.SpecialEffects} \\cdot 10 + {rating.Acoustic.General} \\cdot 25}}{{100}} = {rating.General}";

            int opening = Average(this.Openings.Select(x => x.Score.Value).ToList());
            int ending = Average(this.Endings.Select(x => x.Score.Value).ToList());
            int syncro = Average(this.Syncros.Where(x => x.IsAssessed).Select(x => x.Score.Value).ToList());
            int soundtrack = Math.Max(rating.Acoustic.Soundtrack, 0);
            int count = new[] { soundtrack, opening, ending, syncro }.Count(x => x != 0);
            this.AcousticSummaryText = $"({soundtrack} + {opening} + {ending} + {syncro})/{count} = {rating.Acoustic.General}";

            this.OnPropertyChanged(nameof(this.FormulaText));
            this.OnPropertyChanged(nameof(this.AcousticSummaryText));
        }

        private static int Average(List<int> values) {
            return values.Count == 0 ? 0 : values.Sum() / values.Count;
        }
    }
}
