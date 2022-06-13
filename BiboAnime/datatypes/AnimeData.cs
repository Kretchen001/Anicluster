namespace BiboAnime.datatypes {

    public class AnimeData {

        public int id { get; set; }
        public bool favorite { get; set; }
        public string name { get; set; }
        public string originalName { get; set; }
        public Rating rating { get; set; }
        public List<string> tags { get; set; }
        public string urlAnimePlanet { get; set; }
        public AnimeStatus status { get; set; }
        public int staffeln { get; set; }
        public int episodesTotal { get; set; }
        public int movies { get; set; }
        public int ovh { get; set; }
        public string thirdPartyRecommendation { get; set; }
        public AnimeTier tier { get; set; }
    }
}