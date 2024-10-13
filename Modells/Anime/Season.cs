namespace Modells.Anime {
    public class Season {

        /// <summary>Number of the Season in a Anime</summary>
        public int Number { get; set; }
        public List<VideoAnimation> Episodes { get; set; } = [];
        public string? Comment { get; set; }
        public PublishingTime PublishingTime { get; set; } = new PublishingTime();
    }
}
