namespace BiboAnime.datatypes {

    public class Rating {

        public int Story { get; set; }
        public int Sound { get; set; }
        public int Animation { get; set; }
        public int SpecialEffect { get; set; }
        public int GermanDub { get; set; }
        public int General { get; set; }

        public Rating(int story, int sound, int animation, int specialEffect, int germanDub) {
            this.Story = story;
            this.Sound = sound;
            this.Animation = animation;
            this.SpecialEffect = specialEffect;
            this.GermanDub = germanDub;

            if (germanDub == 0) {
                this.General = (int)((story * 0.4) + (sound * 0.2) + (animation * 0.3) + (specialEffect * 0.1));
            }
            else {
                this.General = (int)((story * 0.4) + (sound * 0.1) + (animation * 0.3) + (specialEffect * 0.1) + (germanDub * 0.1));
            }
        }

        public bool Equals(Rating a, Rating b) {
            return 
                a.Animation == b.Animation &&
                a.Sound == b.Sound &&
                a.Story == b.Story &&
                a.SpecialEffect == b.SpecialEffect &&
                a.General == b.General &&
                a.GermanDub == b.GermanDub;
        }
    }
}