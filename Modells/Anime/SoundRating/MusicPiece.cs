namespace Modells.Anime.SoundRating {
    public class MusicPiece {

        public int Id { get; set; }
        public MusicPieceType Type { get; set; }
        public string? Name { get; set; }
        public string? Comment { get; set; }
        public int General { get; set; } = -1;
    }
}