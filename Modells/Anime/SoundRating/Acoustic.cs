namespace Modells.Anime.SoundRating {
    public class Acoustic {

        public int General {
            get {
                int temp = (Soundtrack != -1 ? Soundtrack : 0) * 4;
                int s0 = 0, s1 = 0, s2 = 0;
                int c0 = 0, c1 = 0, c2 = 0;
                if (Opening.Count == 0) { c0 = 1; }
                foreach (MusicPiece x in Opening) {
                    s0 += x.General;
                    c0 += 1;
                }
                if (Ending.Count == 0) { c1 = 1; }
                foreach (MusicPiece x in Ending) {
                    s1 += x.General;
                    c1 += 1;
                }
                if (Syncro.Count == 0) { c2 = 1; }
                foreach (Syncro x in Syncro) {
                    if (x.IsAssessed) {
                        s2 += x.General;
                        c2 += 1;
                    }
                }
                temp += (s0 / c0); // Opening
                temp += (s1 / c1); // Ending
                temp += (s2 / c2) * 4; // Syncro
                return (temp / 10);
            }
        }
        public List<MusicPiece> Opening { get; set; } = [];
        public List<MusicPiece> Ending { get; set; } = [];
        public int Soundtrack { get; set; } = -1;
        public List<Syncro> Syncro { get; set; } = [];
        public string? Comment { get; set; }

        public Acoustic() { }

        public Acoustic(List<MusicPiece> opening, List<MusicPiece> ending, int soundtrack, List<Syncro> syncros, string? comment) {
            Opening = opening;
            Ending = ending;
            Soundtrack = soundtrack;
            Syncro = syncros;
            Comment = comment;
        }
    }
}
