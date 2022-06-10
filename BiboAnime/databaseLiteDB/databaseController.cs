using BiboAnime.datatypes;
using LiteDB;

namespace BiboAnime.databaseLiteDB {

    public class databaseController {

        private string locationOfDataBase = (AppDomain.CurrentDomain.BaseDirectory + "/databaseLiteDB/database.liteDB");

        public void addAnimeToDB(AnimeData animeToAdd) {
            using (var db = new LiteDatabase(locationOfDataBase)) {
                var col = db.GetCollection<AnimeData>("Animes");
                col.Insert(animeToAdd);
                col.EnsureIndex(x => x.id);
            }
        }

        public void updateAnime(AnimeData animeToUpdate) {
            using (var db = new LiteDatabase(locationOfDataBase)) {
                var col = db.GetCollection<AnimeData>("Animes");
                col.Update(animeToUpdate.id, animeToUpdate);
            }
        }

        public List<AnimeData> getAllAnimes() {
            using (var db = new LiteDatabase(locationOfDataBase)) {
                var col = db.GetCollection<AnimeData>("Animes");
                var result = col.Query()
                    .ToList();
                return result;
            }
        }

        public List<AnimeRow> getAllAnimesAsRow() {
            using (var db = new LiteDatabase(locationOfDataBase)) {
                var col = db.GetCollection<AnimeRow>("Animes");
                var result = col.Query()
                    .ToList();
                List<AnimeRow> rows = new List<AnimeRow>();
                for (int i = 0; i < result.Count; i += 1) {
                    rows.Add(new AnimeRow {
                        id = result[i].id,
                        favorite = result[i].favorite,
                        name = result[i].name,
                        status = result[i].status,
                        tags = result[i].tags,
                    });
                }
                return rows;
            }
        }

        public List<AnimeData> getByOneTag(string tag) {
            using (var db = new LiteDatabase(locationOfDataBase)) {
                var col = db.GetCollection<AnimeData>("Animes");
                var result = col.Query()
                    .Where(x => x.tags.Contains(tag))
                    .ToList();
                return result;
            }
        }

        public List<AnimeData> getByListOfTags(List<string> tags) {
            using (var db = new LiteDatabase(locationOfDataBase)) {
                var col = db.GetCollection<AnimeData>("Animes");
                List<AnimeData> result = new List<AnimeData>();
                for (int i = 0; i < tags.Count; i += 1) {
                    var tempResult = col.Query()
                        .Where(x => x.tags.Contains(tags[i]))
                        .ToList();
                    for (int j = 0; j < tempResult.Count; j += 1) {
                        result.Add(tempResult[j]);
                    }
                }
                return result;
            }
        }

        public List<AnimeData> getByTier(AnimeTier animeTier) {
            using (var db = new LiteDatabase(locationOfDataBase)) {
                var col = db.GetCollection<AnimeData>("Animes");
                List<AnimeData> result = new List<AnimeData>();
                result = col.Query()
                    .Where(x => x.tier == animeTier)
                    .ToList();
                return result;
            }
        }
    }   
}