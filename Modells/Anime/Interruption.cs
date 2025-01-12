namespace Modells.Anime {

    public class Interruption {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string? Comment { get; set; }

        public Interruption() { }

        public Interruption(DateTime start, DateTime end, string? comment) {
            this.Start = start;
            this.End = end;
            this.Comment = comment;
        }
    }
}
