using BiboAnime.datatypes;
using LiteDB;

namespace BiboAnime.databaseLiteDB {

    public class DatabaseController {

        public static int ResetDataBaseAnimes() {
            using (LiteDatabase db = new LiteDatabase(databaseConfigs.LocationOfDataBase)) {
                var col = db.GetCollection<AnimeData>("Animes");
                return col.DeleteAll(); // how many was deleted
            }
        }

        public static void AddAnimeToDB(AnimeData animeToAdd) {
            using (LiteDatabase db = new LiteDatabase(databaseConfigs.LocationOfDataBase)) {
                var col = db.GetCollection<AnimeData>("Animes");
                col.Insert(animeToAdd);
                col.EnsureIndex(x => x.Id);
            }
        }

        public static void AddImportedAnimes(List<AnimeData> animesToImport) {
            using (LiteDatabase db = new LiteDatabase(databaseConfigs.LocationOfDataBase)) {
                var col = db.GetCollection<AnimeData>("Animes");
                foreach (var item in animesToImport) {
                    if (col.Exists(x => x.Id.Equals(item.Id))) {
                        item.Id = Guid.NewGuid(); // damit keine Guid doppelt vorkommt
                    }
                    col.Insert(item);
                    col.EnsureIndex(x => x.Id);
                }
            }
        }

        public static bool DeleteAnimeFromDB(AnimeData animeToDelete) {
            using (LiteDatabase db = new LiteDatabase(databaseConfigs.LocationOfDataBase)) {
                var col = db.GetCollection<AnimeData>("Animes");
                return col.Delete(animeToDelete.Id);
            }
        }

        public static void UpdateAnime(AnimeData animeToUpdate) {
            using (LiteDatabase db = new LiteDatabase(databaseConfigs.LocationOfDataBase)) {
                var col = db.GetCollection<AnimeData>("Animes");
                col.Update(animeToUpdate.Id, animeToUpdate);
            }
        }

        public static List<AnimeData> GetAllAnimes() {
            using (LiteDatabase db = new LiteDatabase(databaseConfigs.LocationOfDataBase)) {
                var col = db.GetCollection<AnimeData>("Animes");
                var result = col.Query()
                    .ToList();
                return result;
            }
        }

        public static List<AnimeRow> GetAllAnimesAsRow() {
            using (LiteDatabase db = new LiteDatabase(databaseConfigs.LocationOfDataBase)) {
                var col = db.GetCollection<AnimeData>("Animes");
                List<AnimeData> result = col.Query()
                    .ToList();
                List<AnimeRow> rows = new List<AnimeRow>();
                for (int i = 0; i < result.Count; i += 1) {
                    string tagString = "";
                    for (int j = 0; j < result[i].Tags.Count; j += 1) {
                        if (j != (result[i].Tags.Count - 1)) {
                            tagString += result[i].Tags[j].TagDesignator + ", ";
                        }
                        else {
                            tagString += result[i].Tags[j].TagDesignator;
                        }
                    }
                    rows.Add(new AnimeRow {
                        Id = i + 1,
                        Favorite = result[i].Favorite,
                        Name = result[i].Name,
                        Status = result[i].Status,
                        Tags = tagString,
                        GeneralRating = result[i].Rating.General,
                    });
                }
                return rows;
            }
        }

        public static AnimeData GetAnimeById(Guid id) {
            using (LiteDatabase db = new LiteDatabase(databaseConfigs.LocationOfDataBase)) {
                var col = db.GetCollection<AnimeData>("Animes");
                var result = col.Query()
                    .Where(x => x.Id.Equals(id))
                    .Single();
                return result;
            }
        }

        public static AnimeData GetAnimeByName(string name) {
            using (LiteDatabase db = new LiteDatabase(databaseConfigs.LocationOfDataBase)) {
                var col = db.GetCollection<AnimeData>("Animes");
                var result = col.Query()
                    .Where(x => x.Name == name)
                    .Single();
                return result;
            }
        }

        public static List<AnimeData> GetByOneTag(AnimeTag tag) {
            using (LiteDatabase db = new LiteDatabase(databaseConfigs.LocationOfDataBase)) {
                var col = db.GetCollection<AnimeData>("Animes");
                var result = col.Query()
                    .Where(x => x.Tags.Contains(tag))
                    .ToList();
                return result;
            }
        }

        public static List<AnimeData> GetByListOfTags(List<AnimeTag> tags) {
            using (LiteDatabase db = new LiteDatabase(databaseConfigs.LocationOfDataBase)) {
                var col = db.GetCollection<AnimeData>("Animes");
                List<AnimeData> result = new List<AnimeData>();
                for (int i = 0; i < tags.Count; i += 1) {
                    var tempResult = col.Query()
                        .Where(x => x.Tags.Contains(tags[i]))
                        .ToList();
                    for (int j = 0; j < tempResult.Count; j += 1) {
                        result.Add(tempResult[j]);
                    }
                }
                return result;
            }
        }

        public static List<AnimeData> GetByTier(AnimeTier animeTier) {
            using (LiteDatabase db = new LiteDatabase(databaseConfigs.LocationOfDataBase)) {
                var col = db.GetCollection<AnimeData>("Animes");
                List<AnimeData> result = new List<AnimeData>();
                result = col.Query()
                    .Where(x => x.Tier == animeTier)
                    .ToList();
                return result;
            }
        }
        

        public bool CheckIfAnimeExist(string name) {
            using (LiteDatabase db = new LiteDatabase(databaseConfigs.LocationOfDataBase)) {
                var col = db.GetCollection<AnimeData>("Animes");
                var result = col.Query()
                    .Where(x => x.Name == name)
                    .ToList();
                return (result.Count != 0);
            }
        }

        //-------------------------------------------------------------------------------------------------
        //------------------------------------TAGS---------------------------------------------------------
        //-------------------------------------------------------------------------------------------------

        /// <summary>
        /// Add the given AnimeTag to the database.
        /// </summary>
        public void AddOneTag(AnimeTag tag) {
            using (LiteDatabase db = new LiteDatabase(databaseConfigs.LocationOfDataBase)) {
                var col = db.GetCollection<AnimeTag>("Tags");
                AnimeTag tempTag = new AnimeTag() { TagDesignator = tag.TagDesignator }; // without id
                if (CheckIfTagExistsByDesignator(tempTag, db)) {
                    return;
                }
                col.Insert(tempTag);
                col.EnsureIndex(x => x.Id);
            }
        }

        public void AddImportedTags(List<AnimeTag> tagList) {
            using (LiteDatabase db = new LiteDatabase(databaseConfigs.LocationOfDataBase)) {
                var col = db.GetCollection<AnimeTag>("Tags");
                for (int i = 0; i < tagList.Count; i += 1) {
                    AnimeTag tempTag = new AnimeTag() { TagDesignator = tagList[i].TagDesignator };
                    col.Insert(tempTag);
                    col.EnsureIndex(x => x.Id);
                }
            }
        }

        public static AnimeTag GetTagByDesignator(string designator) {
            using (LiteDatabase db = new LiteDatabase(databaseConfigs.LocationOfDataBase)) {
                var col = db.GetCollection<AnimeTag>("Tags");
                return col.Query()
                    .Where(x => x.TagDesignator == designator)
                    .Limit(1)
                    .Single();
            }
        }

        public int ResetDataBaseTags() {
            using (LiteDatabase db = new LiteDatabase(databaseConfigs.LocationOfDataBase)) {
                var col = db.GetCollection<AnimeTag>("Tags");
                return col.DeleteAll(); // how many was deleted
            }
        }

        public static List<AnimeTag> GetAllAnimeTags() {
            using (LiteDatabase db = new LiteDatabase(databaseConfigs.LocationOfDataBase)) {
                var col = db.GetCollection<AnimeTag>("Tags");
                List<AnimeTag> result = new List<AnimeTag>();
                result = col.Query()
                    .ToList();
                return result;
            }
        }

        public static List<AnimeTag> GetAllUsedTags() {
            using (LiteDatabase db = new LiteDatabase(databaseConfigs.LocationOfDataBase)) {
                var col = db.GetCollection<AnimeData>("Animes");
                List<AnimeData> listOfAnimes = new List<AnimeData>();
                listOfAnimes = col.Query()
                    .ToList();

                List<AnimeTag> listOfTags = new List<AnimeTag>();
                for (int i = 0; i < listOfAnimes.Count; i += 1) {
                    for (int j = 0; j < listOfAnimes[i].Tags.Count; j += 1) {
                        if (!listOfTags.Contains(listOfAnimes[i].Tags[j])) {
                            listOfTags.Add(listOfAnimes[i].Tags[j]);
                        }
                    }
                }

                return listOfTags;
            }
        }

        public static int GetTagDbCountForIdPlusOne() {
            using (LiteDatabase db = new LiteDatabase(databaseConfigs.LocationOfDataBase)) {
                var col = db.GetCollection<AnimeTag>("Tags");
                return (col.Count() + 1);
            }
        }

        /// <summary>
        /// Function to check, if a tag EXACTLY exists already. Look only for the designator, ignore the id.
        /// </summary>
        /// <param name="animeTag">the tag, which is checked</param>
        /// <returns>true -> Tag exists</returns>
        public static bool CheckIfTagExistsByDesignator(AnimeTag animeTag) {
            using (LiteDatabase db = new LiteDatabase(databaseConfigs.LocationOfDataBase)) {
                var col = db.GetCollection<AnimeTag>("Tags");
                var result = col.Query()
                    .Where(x => x.TagDesignator == animeTag.TagDesignator);
                if (result.Count() == 0) {
                    return false;
                }
                else {
                    return true;
                }
                //return (result is null);
            }
        }

        /// <summary>
        /// Same Function as checkIfTagExistsByDesignator, only with a given Database. (no IOException).
        /// </summary>
        /// <param name="animeTag"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static bool CheckIfTagExistsByDesignator(AnimeTag animeTag, LiteDatabase db) {
            var col = db.GetCollection<AnimeTag>("Tags");
            var result = col.Query()
                .Where(x => x.TagDesignator == animeTag.TagDesignator);
            if (result.Count() == 0) {
                return false;
            }
            else {
                return true;
            }
        }

        public static bool DeleteOneTag(AnimeTag tagToDelete) {
            using (LiteDatabase db = new LiteDatabase(databaseConfigs.LocationOfDataBase)) {
                var col = db.GetCollection<AnimeTag>("Tags");
                return (col.Delete(tagToDelete.Id));
            }
        }
    }
}