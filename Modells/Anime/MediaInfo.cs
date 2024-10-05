namespace Modells.Anime {
    public class MediaInfo {

        public string? Author { get; set; }
        public string? Producer { get; set; }
        public string? Publisher { get; set; }
        public PublishingTime PublishingTime { get; set; } = new PublishingTime();
    }
}
