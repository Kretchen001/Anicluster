using BiboAnime.datatypes;

namespace Anicluster.windows.filter {

    public class TagSelection : AnimeTag {

        public bool inclusive { get; set; } = false;
        public bool exclusive { get; set; } = false;

        public TagSelection(AnimeTag animeTag) : base(animeTag.id, animeTag.tagDesignator) {
        }
    }
}
