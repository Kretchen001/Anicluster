using Modells.Anime.SoundRating;

namespace Modells.Anime {
    public class Rating {

        public int General {
            get {
                return ((Story * 40 + Animation * 25 + SpecialEffects * 10 + Acoustic.General * 25) / 100);
            }
        }
        public int Story { get; set; }
        public int Animation { get; set; }
        public int SpecialEffects { get; set; }
        public Acoustic Acoustic { get; set; } = new Acoustic();
        public bool IsRated { get; set; } = false;

        public Rating() {}

        public Rating(int story, int animation, int specialEffects, Acoustic acoustic, bool isRated) {
            Story = story;
            Animation = animation;
            SpecialEffects = specialEffects;
            Acoustic = acoustic;
            IsRated = isRated;
        }
    }
}
