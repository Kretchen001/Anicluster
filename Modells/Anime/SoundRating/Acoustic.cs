namespace Modells.Anime.SoundRating {
    public class Acoustic {

        public int General {
            get {
                int temp = Opening + Ending + (Soundtrack * 4);
                int s = 0, c = 0;
                foreach (Syncro x in Syncro) {
                    if (x.IsAssessed) {
                        s += x.General;
                        c += 1;
                    }
                }
                temp += (s / c) * 4;
                return (temp / 10);
            }
        }
        public int Opening { get; set; } = -1;
        public int Ending { get; set; } = -1;
        public int Soundtrack { get; set; } = -1;
        public List<Syncro> Syncro { get; set; } = [];
        public string? Comment { get; set; }

        public Acoustic() {}
    }
}
