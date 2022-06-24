using BiboAnime.datatypes;

namespace Anicluster.windows.filter {

    public class TagSelection : AnimeTag {

        public bool inklisive { get; set; } = false;
        public bool exklusive { get; set; } = false;

        public TagSelection(AnimeTag animeTag) : base(animeTag.id, animeTag.tagDesignator) {
        }
    }
}
