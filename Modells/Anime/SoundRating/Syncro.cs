namespace Modells.Anime.SoundRating {

    /// <summary>Eine Synchronisation (Sprache) mit Bewertung.</summary>
    public class Syncro : ValueComparableObject {

        public int General { get; set; } = -1;
        public SyncroLanuage Language { get; set; } = SyncroLanuage.jp;

        /// <summary>Soll diese Syncro in der Bewertung berücksichtigt werden?</summary>
        public bool IsAssessed { get; set; } = false;

        public string? Comment { get; set; }

        public Syncro() { }

        public Syncro(int general, SyncroLanuage language, bool isAssessed, string? comment) {
            this.General = general;
            this.Language = language;
            this.IsAssessed = isAssessed;
            this.Comment = comment;
        }
    }
}
