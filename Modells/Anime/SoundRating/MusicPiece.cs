namespace Modells.Anime.SoundRating {

    public class MusicPiece : ValueComparableObject {

        public int Id { get; set; }
        public MusicPieceType Type { get; set; }
        public string? Name { get; set; }
        public string? Comment { get; set; }
        public int General { get; set; } = -1;

        public MusicPiece() { }

        public MusicPiece(int id, MusicPieceType type, string? name, string? comment, int general) {
            this.Id = id;
            this.Type = type;
            this.Name = name;
            this.Comment = comment;
            this.General = general;
        }
    }
}