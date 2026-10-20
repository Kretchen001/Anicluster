namespace Modells.Anime.SoundRating {
    
    public class Acoustic : ValueComparableObject {

        public int Id { get; set; }
        public int General {
            get {
                int temp = (this.Soundtrack != -1 ? this.Soundtrack : 0);
                int s0 = 0, s1 = 0, s2 = 0;
                int c0 = 0, c1 = 0, c2 = 0;
                if (this.Opening.Count == 0) { c0 = 1; }
                foreach (MusicPiece x in this.Opening) {
                    s0 += x.General;
                    c0 += 1;
                }
                if (this.Ending.Count == 0) { c1 = 1; }
                foreach (MusicPiece x in this.Ending) {
                    s1 += x.General;
                    c1 += 1;
                }
                foreach (Syncro x in this.Syncro) {
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
                int temp = this.soundtrack;
                if (this.Sounds.Count >= 1) {
                    temp = 0;
                    foreach (MusicPiece x in this.Sounds) {
                        temp += x.General;
                    }
                    temp /= this.Sounds.Count;
                }
                return temp;
            }
            set { this.soundtrack = value; }
        }
        public List<MusicPiece> Sounds { get; set; } = [];
        public List<Syncro> Syncro { get; set; } = [];
        public string? Comment { get; set; }

        public Acoustic() { }

        public Acoustic(List<MusicPiece> opening, List<MusicPiece> ending, int soundtrack, List<Syncro> syncros, string? comment) {
            this.Opening = opening;
            this.Ending = ending;
            this.Soundtrack = soundtrack;
            this.Syncro = syncros;
            this.Comment = comment;
        }

        public Acoustic(int id, List<MusicPiece> opening, List<MusicPiece> ending, List<MusicPiece> sounds, int soundtrack, List<Syncro> syncros, string? comment) {
            this.Id = id;
            this.Opening = opening;
            this.Ending = ending;
            this.Sounds = sounds;
            this.Soundtrack = soundtrack;
            this.Syncro = syncros;
            this.Comment = comment;
        }
    }
}
