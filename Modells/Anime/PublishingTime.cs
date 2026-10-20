namespace Modells.Anime {
    public class PublishingTime : ValueComparableObject {

        public int Id { get; set; }
        public DateTime StartDate { get; set; } = new DateTime(1, 1, 1);
        public DateTime EndDate { get; set; } = new DateTime(1, 1, 1);
        public List<Interruption> Interruptions { get; set; } = [];

        public string DisplayStartYear => this.StartDate.Year.ToString("D4");
        public string DisplayEndYear => this.EndDate.Year.ToString("D4");

        public PublishingTime() { }
    }
}
