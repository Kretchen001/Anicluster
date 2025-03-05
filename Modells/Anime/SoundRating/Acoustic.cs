namespace Modells.Anime.SoundRating {
    public class Acoustic {

        public int Id { get; set; }
        public int General {
            get {
                int temp = (Soundtrack != -1 ? Soundtrack : 0);
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
                foreach (Syncro x in Syncro) {
                    if (x.IsAssessed) {
                        s2 += x.General;
                        c2 += 1;
                    }
                }
                c2 = c2 != 0 ? c2 : 1; // check if any Syncro IsAssessed

                int temp2 = (s0 / c0); // Opening
                temp2 += (s1 / c1); // Ending
                temp2 += (s2 / c2); // Syncro
                if (temp2 != 0) {
                    int faktor = 0;
                    faktor += c0 >= 1 ? faktor + 1 : faktor;
                    faktor += c1 >= 1 ? faktor + 1 : faktor;
                    faktor += c2 >= 1 ? faktor + 1 : faktor;
                    temp = (temp * faktor + temp2) / (faktor + 1);
                }
                return (temp);
            }
        }
        public List<MusicPiece> Opening { get; set; } = [];
        public List<MusicPiece> Ending { get; set; } = [];
        private int soundtrack = -1;
        public int Soundtrack {
            get {
                int temp = soundtrack;
                if (Sounds.Count >= 1) {
                    temp = 0;
                    foreach (MusicPiece x in Sounds) {
                        temp += x.General;
                    }
                    temp /= Sounds.Count;
                }
                return temp;
            }
            set { soundtrack = value; }
        }
        public List<MusicPiece> Sounds { get; set; } = [];
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

        public Acoustic(int id, List<MusicPiece> opening, List<MusicPiece> ending, List<MusicPiece> sounds, int soundtrack, List<Syncro> syncros, string? comment) {
            Id = id;
            Opening = opening;
            Ending = ending;
            Sounds = sounds;
            Soundtrack = soundtrack;
            Syncro = syncros;
            Comment = comment;
        }
    }
}
