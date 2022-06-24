namespace BiboAnime.datatypes {

    public class AnimeTag {

        public int id { get; set; }
        public string tagDesignator { get; set; }

        public AnimeTag() {
            // null constructor
        }

        public AnimeTag(int id, string tagDesignator) {
            this.id = id;
            this.tagDesignator = tagDesignator;
        }
    }
}