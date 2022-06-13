using BiboAnime.datatypes;
using LiteDB;

namespace BiboAnime.databaseLiteDB {

    public class databaseFilterController {

        public List<AnimeFilterTier> dbFilterTier(AnimeTier tierToFilter) {
            using (var db = new LiteDatabase(databaseConfigs.locationOfDataBase)) {
                var col = db.GetCollection<AnimeData>("Animes");
                var result = col.Query()
                    .Where(x => x.tier == tierToFilter)
                    .ToList();
                List<AnimeFilterTier> filters = new List<AnimeFilterTier>();
                for (int i = 0; i < result.Count; i += 1) {
                    List<AnimeTag> tags = new List<AnimeTag>();
                    for (int j = 0; j < result[i].tags.Count; j += 1) {

                    }
                    Rating rating = new Rating(
                        result[i].rating.story,
                        result[i].rating.sound,
                        result[i].rating.animation,
                        result[i].rating.specialEffect,
                        result[i].rating.germanDub);

                    filters.Add(new AnimeFilterTier() {
                        id = i,
                        tier = result[i].tier,
                        name = result[i].name,
                        rating = rating,
                        animeTag = tags
                    });
                }
                return filters;
            }
        }
    }
}
