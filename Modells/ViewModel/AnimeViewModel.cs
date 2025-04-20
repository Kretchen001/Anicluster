using Modells.Anime;

namespace Modells.ViewModel {

    public class AnimeViewModel : ValueComparableObject {

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsFavorite { get; set; }
        public Tier Tier { get; set; }
        public State State { get; set; }

        public AnimeViewModel(int id, string name, bool isFavorite, Tier tier, State state) {
            this.Id = id;
            this.Name = name;
            this.IsFavorite = isFavorite;
            this.Tier = tier;
            this.State = state;
        }
    }
}
