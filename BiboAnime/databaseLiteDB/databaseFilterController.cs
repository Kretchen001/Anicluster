using BiboAnime.datatypes;
using LiteDB;

namespace BiboAnime.databaseLiteDB {

    public class databaseFilterController {

        public List<AnimeData> DbFilterTier(AnimeTier tierToFilter) {
            using (LiteDatabase db = new LiteDatabase(databaseConfigs.LocationOfDataBase)) {
                var col = db.GetCollection<AnimeData>("Animes");
                var result = col.Query()
                    .Where(x => x.Tier == tierToFilter)
                    .ToList();
                return result;
            }
        }

        public List<AnimeData> DbFilterByTag(List<AnimeTag> filterTags) {
            using (LiteDatabase db = new LiteDatabase(databaseConfigs.LocationOfDataBase)) {
                var col = db.GetCollection<AnimeData>("Animes");
                List<AnimeData> result = col.Query().ToList();
                List<AnimeData> returnResult = new List<AnimeData>();
                for (int i = 0; i < filterTags.Count; i += 1) {
                    AnimeTag tag = filterTags[i];
                    List<AnimeData> tempResult = col.Query()
                        .Where(x => x.Tags.Contains(tag))
                        .ToList();
                    for (int j = 0; j < tempResult.Count; j += 1) {
                        for (int k = 0; k < result.Count; k += 1) {
                            if (result[k].Id == tempResult[j].Id) {
                                returnResult.Add(tempResult[j]);
                            }
                        }
                    }
                }
                return returnResult;
            }
        }

        public List<AnimeData> DbFilterWithoutTag(List<AnimeTag> filterTags) {
            using (LiteDatabase db = new LiteDatabase(databaseConfigs.LocationOfDataBase)) {
                var col = db.GetCollection<AnimeData>("Animes");
                List<AnimeData> returnResult = new List<AnimeData>();
                for (int i = 0; i < filterTags.Count; i += 1) {
                    AnimeTag tag = filterTags[i];
                    returnResult.AddRange(col.Query().Where(x => !x.Tags.Contains(tag)).ToList());
                }
                return returnResult;
            }
        }

        public List<AnimeData> DbFilterTagAndTier(AnimeTier filterTier, List<AnimeTag> filterTags) {
            using (LiteDatabase db = new LiteDatabase(databaseConfigs.LocationOfDataBase)) {
                var col = db.GetCollection<AnimeData>("Animes");
                List<AnimeData> result = col.Query()
                    .Where(x => x.Tier == filterTier)   // filter tier | tags later
                    .ToList();
                List<AnimeData> filters = new List<AnimeData>();
                for (int i = 0; i < result.Count; i += 1) {
                    for (int j = 0; j < result[i].Tags.Count; j += 1) {
                        for (int k = 0; k < filterTags.Count; k += 1) {
                            if (result[i].Tags[j].TagDesignator == filterTags[k].TagDesignator) {
                                filters.Add(result[i]);
                            }
                        }
                    }
                }
                return filters;
            }
        }

        public List<AnimeData> DbFilterRatingMinGeneral(int minGeneralRating) {
            using (LiteDatabase db = new LiteDatabase(databaseConfigs.LocationOfDataBase)) {
                var col = db.GetCollection<AnimeData>("Animes");
                List<AnimeData> result = col.Query()
                    .Where(x => x.Rating.General >= minGeneralRating)   // filter tier | tags later
                    .ToList();
                return result;
            }
        }

        public List<AnimeData> DbFilterIsFav(bool fav) {
            using (LiteDatabase db = new LiteDatabase(databaseConfigs.LocationOfDataBase)) {
                var col = db.GetCollection<AnimeData>("Animes");
                List<AnimeData> result = col.Query()
                    .Where(x => x.Favorite == fav)
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
        public List<AnimeData> DbFilterRatingMinABCDE(int minGeneral = 0, int minStory = 0, int minSound = 0, int minAnimation = 0, int minSpecialEffect = 0, int minGermanDub = 0) {
            using (LiteDatabase db = new LiteDatabase(databaseConfigs.LocationOfDataBase)) {
                var col = db.GetCollection<AnimeData>("Animes");
                List<AnimeData> result = new List<AnimeData>();
                if (minGeneral == 0) {
                    result = col.Query()
                        .Where(x => x.Rating.Story >= minStory
                            && x.Rating.Sound >= minSound
                            && x.Rating.Animation >= minAnimation
                            && x.Rating.SpecialEffect >= minSpecialEffect
                            && x.Rating.GermanDub >= minGermanDub)
                        .ToList();
                }
                else {
                    result = col.Query()
                        .Where(x => x.Rating.General >= minGeneral)
                        .ToList();
                }
                return result;
            }
        }

        public List<AnimeData> DbFilterRatingCheckGermanDub(bool dub) {
            using (LiteDatabase db = new LiteDatabase(databaseConfigs.LocationOfDataBase)) {
                var col = db.GetCollection<AnimeData>("Animes");
                List<AnimeData> result = new List<AnimeData>();
                if (dub) {
                    result = col.Query()
                    .Where(x => x.Rating.GermanDub > 0)
                    .ToList();
                }
                else {
                    result = col.Query()
                    .Where(x => x.Rating.GermanDub == 0)
                    .ToList();
                }
                return result;
            }
        }

        public List<AnimeData> DbFilterMinMaxStaffeln(bool max, int amount) {
            using (LiteDatabase db = new LiteDatabase(databaseConfigs.LocationOfDataBase)) {
                var col = db.GetCollection<AnimeData>("Animes");
                List<AnimeData> result = new List<AnimeData>();
                if (max) {
                    result = col.Query()
                    .Where(x => (x.Staffeln.Count <= amount) && (x.Staffeln.Count != 0))
                    .ToList();
                }
                else {
                    result = col.Query()
                    .Where(x => (x.Staffeln.Count >= amount) && (x.Staffeln.Count != 0))
                    .ToList();
                }
                return result;
            }
        }

        public List<AnimeData> DbFilterMinMaxEpisodesTotal(bool max, int amount) {
            using (LiteDatabase db = new LiteDatabase(databaseConfigs.LocationOfDataBase)) {
                var col = db.GetCollection<AnimeData>("Animes");
                List<AnimeData> result = new List<AnimeData>();
                if (max) {
                    result = col.Query()
                    .Where(x => (x.EpisodesTotal <= amount) && (x.EpisodesTotal != 0))
                    .ToList();
                }
                else {
                    result = col.Query()
                    .Where(x => (x.EpisodesTotal >= amount) && (x.EpisodesTotal != 0))
                    .ToList();
                }
                return result;
            }
        }

        public List<AnimeData> DbFilterCheckRecommendation(bool exist, bool existnot) {
            using (LiteDatabase db = new LiteDatabase(databaseConfigs.LocationOfDataBase)) {
                var col = db.GetCollection<AnimeData>("Animes");
                List<AnimeData> result = new List<AnimeData>();
                if (exist) {
                    return result = col.Query()
                    .Where(x => x.ThirdPartyRecommendation.Length > 0)
                    .ToList();
                }
                if (existnot) {
                    return result = col.Query()
                    .Where(x => x.ThirdPartyRecommendation.Length == 0)
                    .ToList();
                }
                return result;
            }
        }
    }
}