using Modells.Anime.SoundRating;

namespace Modells.Anime {
    public class Rating : ValueComparableObject {

        public int Id { get; set; }
        public int General {
            get {
                return this.CalcGeneral();
            }
        }
        public int Story { get; set; }
        public int Animation { get; set; }
        public int SpecialEffects { get; set; }
        public Acoustic Acoustic { get; set; } = new Acoustic();
        public bool IsRated { get; set; } = false;

        public Rating() { }

        public Rating(int story, int animation, int specialEffects, Acoustic acoustic, bool isRated) {
            this.Story = story;
            this.Animation = animation;
            this.SpecialEffects = specialEffects;
            this.Acoustic = acoustic;
            this.IsRated = isRated;
        }

        public Rating(int id, int story, int animation, int specialEffects, Acoustic acoustic, bool isRated) {
            this.Id = id;
            this.Story = story;
            this.Animation = animation;
            this.SpecialEffects = specialEffects;
            this.Acoustic = acoustic;
            this.IsRated = isRated;
        }

        private int CalcGeneral() {
            return ((this.Story * 40 + this.Animation * 25 + this.SpecialEffects * 10 + this.Acoustic.General * 25) / 100);
        }

        public static int CalcGeneral(int story, int animation, int specialEffects, int acousticGeneral) {
            return ((story * 40 + animation * 25 + specialEffects * 10 + acousticGeneral * 25) / 100);
        }
    }
}
