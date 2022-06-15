using BiboAnime.datatypes;
using LiteDB;

namespace BiboAnime.databaseLiteDB {

    public class databaseFilterController {

        public List<AnimeData> dbFilterTier(AnimeTier tierToFilter) {
            using (var db = new LiteDatabase(databaseConfigs.locationOfDataBase)) {
                var col = db.GetCollection<AnimeData>("Animes");
                var result = col.Query()
                    .Where(x => x.tier == tierToFilter)
                    .ToList();
                return result;
            }
        }

        public List<AnimeData> dbFilterTagAndTier(AnimeTier filterTier, List<AnimeTag> filterTags) {
            using (var db = new LiteDatabase(databaseConfigs.locationOfDataBase)) {
                var col = db.GetCollection<AnimeData>("Animes");
                List<AnimeData> result = col.Query()
                    .Where(x => x.tier == filterTier)   // filter tier | tags later
                    .ToList();
                List<AnimeData> filters = new List<AnimeData>();
                for (int i = 0; i < result.Count; i += 1) {
                    for (int j = 0; j < result[i].tags.Count; j += 1) {
                        for (int k = 0; k < filterTags.Count; k += 1) {
                            if (result[i].tags[j].tagDesignator == filterTags[k].tagDesignator) {
                                filters.Add(result[i]);
                            }
                        }
                    }
                }
                return filters;
            }
        }
    }
}