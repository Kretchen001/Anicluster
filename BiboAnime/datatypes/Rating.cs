namespace BiboAnime.datatypes {

    public class Rating {

        public int story { get; set; }
        public int sound { get; set; }
        public int animation { get; set; }
        public int specialEffect { get; set; }
        public int germanDub { get; set; }
        public int general { get; set; }

        public Rating(int story, int sound, int animation, int specialEffect, int germanDub) {
            this.story = story;
            this.sound = sound;
            this.animation = animation;
            this.specialEffect = specialEffect;
            this.germanDub = germanDub;

            if (germanDub == 0) {
                this.general = (int)((story * 0.4) + (sound * 0.2) + (animation * 0.3) + (specialEffect * 0.1));
            }
            else {
                this.general = (int)((story * 0.4) + (sound * 0.1) + (animation * 0.3) + (specialEffect * 0.1) + (germanDub * 0.1));
            }
        }
    }
}