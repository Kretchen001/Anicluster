namespace Modells.Anime {
    public class Tag {

        public int Id { get; set; }
        public string Designation { get; set; } = string.Empty;

        public Tag() {}

        public Tag(string designation) {
            Designation = designation;
        }
    }
}
