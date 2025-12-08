namespace Modells.Anime {
    public class VideoAnimation : ValueComparableObject {

        public int Id { get; set; }
        public VideoType VideoType { get; set; }
        public string? Comment { get; set; }

        public VideoAnimation() { }

        public VideoAnimation(VideoType videoType, string? comment) {
            this.VideoType = videoType;
            this.Comment = comment;
        }

        public VideoAnimation(int id, VideoType videoType, string? comment) {
            this.Id = id;
            this.VideoType = videoType;
            this.Comment = comment;
        }
    }
}
