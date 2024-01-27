using LiteDB;
using JSeri = System.Text.Json.JsonSerializer;

namespace BiboAnime.datatypes {

    public class AnimeData : IEquatable<AnimeData> {

        [BsonId]
        public Guid Id { get; set; }

        public bool Favorite { get; set; } = false;
        public string Name { get; set; }
        public string OriginalName { get; set; }
        public Rating Rating { get; set; }
        public List<AnimeTag> Tags { get; set; }
        public string UrlAnimePlanet { get; set; }
        public AnimeStatus Status { get; set; }
        public List<Staffel> Staffeln { get; set; }
        public int EpisodesTotal { get; set; }
        public int Movies { get; set; }
        public int Ovh { get; set; }
        public string ThirdPartyRecommendation { get; set; }
        public AnimeTier Tier { get; set; }

        public bool Equals(AnimeData other) {
            return this.Id == other.Id &&
                this.Favorite == other.Favorite &&
                this.Name == other.Name;
        }

        public AnimeData DeepClone() {
            return JSeri.Deserialize<AnimeData>(JSeri.Serialize(this, this.GetType()));
        }
    }

    public class ItemEqualityComparer : IEqualityComparer<AnimeData> {
        public bool Equals(AnimeData x, AnimeData y) {
            // Two items are equal if their keys are equal.
            return x.Id == y.Id;
        }

        public int GetHashCode(AnimeData obj) {
            return obj.Id.GetHashCode();
        }
    }
}