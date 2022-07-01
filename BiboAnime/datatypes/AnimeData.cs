namespace BiboAnime.datatypes {

    public class AnimeData : IEquatable<AnimeData> {

        public int id { get; set; }
        public bool favorite { get; set; }
        public string name { get; set; }
        public string originalName { get; set; }
        public Rating rating { get; set; }
        public List<AnimeTag> tags { get; set; }
        public string urlAnimePlanet { get; set; }
        public AnimeStatus status { get; set; }
        public List<Staffel> staffeln { get; set; }
        public int episodesTotal { get; set; }
        public int movies { get; set; }
        public int ovh { get; set; }
        public string thirdPartyRecommendation { get; set; }
        public AnimeTier tier { get; set; }

        public bool Equals(AnimeData other) {
            return this.id == other.id &&
                this.favorite == other.favorite &&
                this.name == other.name;
        }
    }

    public class ItemEqualityComparer : IEqualityComparer<AnimeData> {
        public bool Equals(AnimeData x, AnimeData y) {
            // Two items are equal if their keys are equal.
            return x.id == y.id;
        }

        public int GetHashCode(AnimeData obj) {
            return obj.id.GetHashCode();
        }
    }
}