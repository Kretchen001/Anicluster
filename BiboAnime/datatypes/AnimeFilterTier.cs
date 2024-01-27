namespace BiboAnime.datatypes {

    public class AnimeFilterTier {

        public int Id { get; set; }
        public AnimeTier Tier { get; set; }
        public string Name { get; set; }
        public Rating Rating { get; set; }
        public List<AnimeTag> AnimeTag { get; set; }
    }
}