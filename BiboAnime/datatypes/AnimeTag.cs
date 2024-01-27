using LiteDB;

namespace BiboAnime.datatypes {

    public class AnimeTag {

        [BsonId]
        public Guid Id { get; set; }

        public string TagDesignator { get; set; }

        public AnimeTag() {
            // null constructor
        }

        public AnimeTag(Guid id, string tagDesignator) {
            this.Id = id;
            this.TagDesignator = tagDesignator;
        }

        public bool EqualTag(AnimeTag a, AnimeTag b) {
            return
                a.Id == b.Id &&
                a.TagDesignator == b.TagDesignator;
        }

        public bool EqualsList(List<AnimeTag> a, List<AnimeTag> b) {
            if (a.Count == b.Count) {
                a = a.OrderBy(x => x.TagDesignator).ToList();
                b = b.OrderBy(x => x.TagDesignator).ToList();

                for (int i = 0; i < a.Count; i += 1) {
                    if (!EqualTag(a[i], b[i])) {
                        return false;
                    }
                }
                return true;
            }
            else {
                return false;
            }
        }
    }
}