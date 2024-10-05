namespace Modells.Anime {
    public class Season {

        public List<VideoAnimation> Episodes { get; set; } = [];
        public string? Comment { get; set; }
        public PublishingTime PublishingTime { get; set; } = new PublishingTime();
    }
}
