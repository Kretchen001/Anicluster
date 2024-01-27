namespace BiboAnime.datatypes {

    public class Staffel {

        public int Counter { get; set; } = 0;
        public int Episodes { get; set; } = 0;

        public bool EqualStaffel(Staffel a, Staffel b) {
            return
                a.Counter == b.Counter &&
                a.Episodes == b.Episodes;
        }

        public bool EqualsList(List<Staffel> a, List<Staffel> b) {
            if (a.Count == b.Count) {
                a = a.OrderBy(x => x.Counter).ToList();
                b = b.OrderBy(x => x.Counter).ToList();

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
