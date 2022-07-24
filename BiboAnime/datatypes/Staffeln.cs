namespace BiboAnime.datatypes {

    public class Staffel {

        public int counter { get; set; } = 0;
        public int episodes { get; set; } = 0;

        public bool EqualStaffel(Staffel a, Staffel b) {
            return
                a.counter == b.counter &&
                a.episodes == b.episodes;
        }

        public bool EqualsList(List<Staffel> a, List<Staffel> b) {
            if (a.Count == b.Count) {
                a = a.OrderBy(x => x.counter).ToList();
                b = b.OrderBy(x => x.counter).ToList();

                for (int i = 0; i < a.Count; i += 1) {
                    if (!EqualStaffel(a[i], b[i])) {
                        return false;
                    }
                }
                return true;
            }
            else {
                return false;
            }
        }
    }
}
