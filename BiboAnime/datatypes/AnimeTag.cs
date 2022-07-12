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
    }
}