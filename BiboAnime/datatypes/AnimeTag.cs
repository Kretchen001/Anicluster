namespace BiboAnime.datatypes {

    public class AnimeTag {

        public Guid id { get; set; }
        public string tagDesignator { get; set; }

        public AnimeTag() {
            // null constructor
        }

        public AnimeTag(Guid id, string tagDesignator) {
            this.id = id;
            this.tagDesignator = tagDesignator;
        }

        public bool EqualTag(AnimeTag a, AnimeTag b) {
            return
                a.id == b.id &&
                a.tagDesignator == b.tagDesignator;
        }

        public bool EqualsList(List<AnimeTag> a, List<AnimeTag> b) {
            if (a.Count == b.Count) {
                a = a.OrderBy(x => x.tagDesignator).ToList();
                b = b.OrderBy(x => x.tagDesignator).ToList();

                for (int i = 0; i < a.Count; i += 1) {
                    if (!EqualTag(a[i], b[i])) {
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