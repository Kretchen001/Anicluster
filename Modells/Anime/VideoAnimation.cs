namespace Modells.Anime {
    public class VideoAnimation : ValueComparableObject {

        public int Id { get; set; }
        public VideoType VideoType { get; set; }
        public string? Comment { get; set; }

        public VideoAnimation() { }

        public VideoAnimation(VideoType videoType, string? comment) {
            VideoType = videoType;
            Comment = comment;
        }

        public VideoAnimation(int id, VideoType videoType, string? comment) {
            Id = id;
            VideoType = videoType;
            Comment = comment;
        }
    }
}
