using Modells.Anime.SoundRating;

namespace Modells.Anime {
    public class Rating {

        public int Id { get; set; }
        public int General {
            get {
                return CalcGeneral();
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

        public Rating(int id, int story, int animation, int specialEffects, Acoustic acoustic, bool isRated) {
            Id = id;
            Story = story;
            Animation = animation;
            SpecialEffects = specialEffects;
            Acoustic = acoustic;
            IsRated = isRated;
        }

        private int CalcGeneral() {
            return ((Story * 40 + Animation * 25 + SpecialEffects * 10 + Acoustic.General * 25) / 100);
        }

        public static int CalcGeneral(int story, int animation, int specialEffects, int acousticGeneral) {
            return ((story * 40 + animation * 25 + specialEffects * 10 + acousticGeneral * 25) / 100);
        }
    }
}
