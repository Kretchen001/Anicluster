namespace Modells.Anime {

    public class Tag : ValueComparableObject {

        public int Id { get; set; }
        public string Designation { get; set; } = string.Empty;

        public Tag() { }

        public Tag(string designation) {
            this.Designation = designation;
        }
    }
}
