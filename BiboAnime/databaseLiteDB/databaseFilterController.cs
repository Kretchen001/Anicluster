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

        /// <summary>
        /// Check, which Animes has the Dummy-Tier, that implicite that there is no Rating from the User.
        /// </summary>
        /// <returns></returns>
        public List<AnimeData> dbFilterAnimeWithoutTier() {
            using (var db = new LiteDatabase(databaseConfigs.locationOfDataBase)) {
                var col = db.GetCollection<AnimeData>("Animes");
                var result = col.Query()
                    .Where(x => x.tier == AnimeTier.Dummy)
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

        public List<AnimeData> dbFilterRatingMinGeneral(int minGeneralRating) {
            using (var db = new LiteDatabase(databaseConfigs.locationOfDataBase)) {
                var col = db.GetCollection<AnimeData>("Animes");
                List<AnimeData> result = col.Query()
                    .Where(x => x.rating.general >= minGeneralRating)   // filter tier | tags later
                    .ToList();
                return result;
            }
        }

        /*
         * Wrapper-functions for the Rating-Filter 
         * -> minStory
         * -> minSound
         * -> minAnimation
         * -> minSpecialEffect
         * -> minGermanDub
         */
        public List<AnimeData> dbFilterRatingOnlyMinStory(int minStory) {
            return dbFilterRatingMinABCDE(minStory, 0, 0, 0, 0);
        }
        public List<AnimeData> dbFilterRatingOnlyMinSound(int minSound) {
            return dbFilterRatingMinABCDE(0, minSound, 0, 0, 0);
        }
        public List<AnimeData> dbFilterRatingOnlyMinAnimation(int minAnimation) {
            return dbFilterRatingMinABCDE(0, 0, minAnimation, 0, 0);
        }
        public List<AnimeData> dbFilterRatingOnlyMinSpecialEffects(int minSpecialEffects) {
            return dbFilterRatingMinABCDE(0, 0, 0, minSpecialEffects, 0);
        }
        public List<AnimeData> dbFilterRatingOnlyMinGermanDub(int minGermanDub) {
            return dbFilterRatingMinABCDE(0, 0, 0, 0, minGermanDub);
        }

        public List<AnimeData> dbFilterRatingMinABCDE(int minStory, int minSound, int minAnimation, int minSpecialEffect, int minGermanDub) {
            using (var db = new LiteDatabase(databaseConfigs.locationOfDataBase)) {
                var col = db.GetCollection<AnimeData>("Animes");
                List<AnimeData> result = col.Query()
                    .Where(x => x.rating.story >= minStory 
                        && x.rating.sound >= minSound
                        && x.rating.animation >= minAnimation
                        && x.rating.specialEffect >= minSpecialEffect
                        && x.rating.germanDub >= minGermanDub)
                    .ToList();
                return result;
            }
        }
        
        public List<AnimeData> dbFilterRatingHasGermanDub() {
            using (var db = new LiteDatabase(databaseConfigs.locationOfDataBase)) {
                var col = db.GetCollection<AnimeData>("Animes");
                List<AnimeData> result = col.Query()
                    .Where(x => x.rating.germanDub > 0)
                    .ToList();
                return result;
            }
        }
    }
}