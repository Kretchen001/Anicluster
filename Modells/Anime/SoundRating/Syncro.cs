namespace Modells.Anime.SoundRating {
    public class Syncro {

        public int General { get; set; } = -1;
        public SyncroLanuage Language { get; set; } = SyncroLanuage.jp;
        /// <summary>this Syncro should be recognized in the rating?</summary>
        public bool IsAssessed { get; set; } = false;
        public string? Comment { get; set; }
    }
}
