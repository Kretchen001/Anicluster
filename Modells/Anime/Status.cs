namespace Modells.Anime {
    public class Status {

        public int Id { get; set; }
        public State State { get; set; } = State.Wishlist;
        public string? Comment { get; set; }

        public Status() { }

        public Status(int id, State state, string? comment) {
            this.Id = id;
            this.State = state;
            this.Comment = comment;
        }
    }
}
