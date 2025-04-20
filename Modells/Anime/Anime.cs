using Newtonsoft.Json;

namespace Modells.Anime {

    public class Anime : ValueComparableObject {

        /// <summary></summary>
        public int Id { get; set; } = -1;
        /// <summary></summary>
        public string Name { get; set; } = "";
        /// <summary></summary>
        public string? OriginalName { get; set; }
        /// <summary></summary>
        public string? Url { get; set; }
        /// <summary></summary>
        public bool Favorite { get; set; } = false;
        /// <summary></summary>
        public Rating Rating { get; set; } = new Rating();
        /// <summary></summary>
        public Tier Tier { get; set; } = Tier.NotDefined;
        /// <summary></summary>
        public List<Season> Seasons { get; set; } = [];
        /// <summary>Mean is OVAs, Movies etc.</summary>
        public List<VideoAnimation> Ovas { get; set; } = [];

        /// <summary></summary>
        public Status Status { get; set; } = new Status();
        /// <summary></summary>
        public List<Tag> Tags { get; set; } = [];
        /// <summary></summary>
        public MediaInfo MediaInfo { get; set; } = new MediaInfo();
        /// <summary></summary>
        public string? RecommendedFrom { get; set; }
        /// <summary>thematic anime before this one, identified by his id</summary>
        public int Predecessor { get; set; } = -1;
        /// <summary>thematic anime after this one, identified by his id</summary>
        public int Successor { get; set; } = -1;
        /// <summary>related anime to this one, identified by his id</summary>
        public int Related { get; set; } = -1;
        /// <summary></summary>
        public string? Comment { get; set; }

        /// <summary></summary>
        public Anime() { }

        /// <summary></summary>
        public Anime DeepClone() {
            string temp = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<Anime>(temp)!;
        }
    }
}
