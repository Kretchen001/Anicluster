namespace BiboAnime.datatypes {

    public class AnimeFilterTier {

        public int id { get; set; }
        public AnimeTier tier { get; set; }
        public string name { get; set; }
        public Rating rating { get; set; }
        public List<AnimeTag> animeTag { get; set; }
    }
}