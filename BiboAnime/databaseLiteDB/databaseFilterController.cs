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

        public List<AnimeData> dbFilterByTag(List<AnimeTag> filterTags) {
            using (var db = new LiteDatabase(databaseConfigs.locationOfDataBase)) {
                var col = db.GetCollection<AnimeData>("Animes");
                List<AnimeData> result = col.Query().ToList();
                List<AnimeData> returnResult = new List<AnimeData>();
                for (int i = 0; i < filterTags.Count; i += 1) {
                    AnimeTag tag = filterTags[i];
                    List<AnimeData> tempResult = col.Query()
                        .Where(x => x.tags.Contains(tag))
                        .ToList();
                    for (int j = 0; j < tempResult.Count; j += 1) {
                        for (int k = 0; k < result.Count; k += 1) {
                            if (result[k].id == tempResult[j].id) {
                                returnResult.Add(tempResult[j]);
                            }
                        }
                    }
                }
                return returnResult;
            }
        }

        public List<AnimeData> dbFilterWithoutTag(List<AnimeTag> filterTags) {
            using (var db = new LiteDatabase(databaseConfigs.locationOfDataBase)) {
                var col = db.GetCollection<AnimeData>("Animes");
                List<AnimeData> result = col.Query().ToList();
                List<AnimeData> returnResult = new List<AnimeData>();
                for (int i = 0; i < filterTags.Count; i += 1) {
                    AnimeTag tag = filterTags[i];
                    List<AnimeData> tempResult = col.Query()
                        .Where(x => x.tags.Contains(tag))
                        .ToList();
                    for (int j = 0; j < tempResult.Count; j += 1) {
                        for (int k = 0; k < result.Count; k += 1) {
                            if (result[j] == tempResult[k]) {
                                returnResult.Add(result[j]);
                            }
                        }
                    }
                }
                return returnResult;
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

        public List<AnimeData> dbFilterIsFav(bool fav) {
            using (var db = new LiteDatabase(databaseConfigs.locationOfDataBase)) {
                var col = db.GetCollection<AnimeData>("Animes");
                List<AnimeData> result = col.Query()
                    .Where(x => x.favorite == fav)
                    .ToList();
                return result;
            }
        }

        /// <summary>
        /// By calling, set concrete the questionary rating-parameters. The rest is 0.
        /// </summary>
        /// <param name="minGeneral">Rating -> General</param>
        /// <param name="minStory">Rating -> Story</param>
        /// <param name="minSound">Rating -> Sound</param>
        /// <param name="minAnimation">Rating -> Animation</param>
        /// <param name="minSpecialEffect">Rating -> Special Effect</param>
        /// <param name="minGermanDub">Rating -> German Dub</param>
        /// <returns>The List of hole Anime´s that have the min Ratings.</returns>
        public List<AnimeData> dbFilterRatingMinABCDE(int minGeneral = 0, int minStory = 0, int minSound = 0, int minAnimation = 0, int minSpecialEffect = 0, int minGermanDub = 0) {
            using (var db = new LiteDatabase(databaseConfigs.locationOfDataBase)) {
                var col = db.GetCollection<AnimeData>("Animes");
                List<AnimeData> result = new List<AnimeData>();
                if (minGeneral == 0) {
                    result = col.Query()
                        .Where(x => x.rating.story >= minStory
                            && x.rating.sound >= minSound
                            && x.rating.animation >= minAnimation
                            && x.rating.specialEffect >= minSpecialEffect
                            && x.rating.germanDub >= minGermanDub)
                        .ToList();
                }
                else {
                    result = col.Query()
                        .Where(x => x.rating.general >= minGeneral)
                        .ToList();
                }
                return result;
            }
        }

        public List<AnimeData> dbFilterRatingCheckGermanDub(bool dub) {
            using (var db = new LiteDatabase(databaseConfigs.locationOfDataBase)) {
                var col = db.GetCollection<AnimeData>("Animes");
                List<AnimeData> result = new List<AnimeData>();
                if (dub) {
                    result = col.Query()
                    .Where(x => x.rating.germanDub > 0)
                    .ToList();
                }
                else {
                    result = col.Query()
                    .Where(x => x.rating.germanDub == 0)
                    .ToList();
                }
                return result;
            }
        }

        public List<AnimeData> dbFilterMinMaxStaffeln(bool max, int amount) {
            using (var db = new LiteDatabase(databaseConfigs.locationOfDataBase)) {
                var col = db.GetCollection<AnimeData>("Animes");
                List<AnimeData> result = new List<AnimeData>();
                if (max) {
                    result = col.Query()
                    .Where(x => (x.staffeln.Count <= amount) && (x.staffeln.Count != 0))
                    .ToList();
                }
                else {
                    result = col.Query()
                    .Where(x => (x.staffeln.Count >= amount) && (x.staffeln.Count != 0))
                    .ToList();
                }
                return result;
            }
        }

        public List<AnimeData> dbFilterMinMaxEpisodesTotal(bool max, int amount) {
            using (var db = new LiteDatabase(databaseConfigs.locationOfDataBase)) {
                var col = db.GetCollection<AnimeData>("Animes");
                List<AnimeData> result = new List<AnimeData>();
                if (max) {
                    result = col.Query()
                    .Where(x => (x.episodesTotal <= amount) && (x.episodesTotal != 0))
                    .ToList();
                }
                else {
                    result = col.Query()
                    .Where(x => (x.episodesTotal >= amount) && (x.episodesTotal != 0))
                    .ToList();
                }
                return result;
            }
        }
    }
}