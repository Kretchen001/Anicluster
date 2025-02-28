namespace Modells.Anime {
    public class MediaInfo {

        public int Id { get; set; }
        public string? Author { get; set; }
        public string? Producer { get; set; }
        public string? Publisher { get; set; }
        public PublishingTime PublishingTime { get; set; } = new PublishingTime();

        public MediaInfo() { }
        public MediaInfo(int id, string? author, string? producer, string? publisher, PublishingTime pubTime) {
            this.Id = id;
            this.Author = author;
            this.Producer = producer;
            this.Publisher = publisher;
            this.PublishingTime = pubTime;
        }
    }
}
