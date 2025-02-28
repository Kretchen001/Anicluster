namespace Modells.Anime {
    public class Season {

        public int Id { get; set; }
        /// <summary>Number of the Season in a Anime</summary>
        public int Number { get; set; }
        public List<VideoAnimation> Episodes { get; set; } = [];
        public string? Comment { get; set; }
        public PublishingTime PublishingTime { get; set; } = new PublishingTime();

        public Season() { }

        public Season(int Id, int Number, string? Comment) {
            this.Id = Id;
            this.Number = Number;
            this.Comment = Comment;
        }
    }
}
