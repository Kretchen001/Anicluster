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

        public bool Equals(AnimeTag a, AnimeTag b) {
            return
                a.id == b.id &&
                a.tagDesignator == b.tagDesignator;
        }
    }
}