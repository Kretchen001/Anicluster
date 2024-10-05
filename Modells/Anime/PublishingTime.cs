namespace Modells.Anime {
    public class PublishingTime {

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<Interruption> Interruptions { get; set; } = [];
    }
}
