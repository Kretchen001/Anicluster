using BiboAnime.datatypes;
using LiteDB;

namespace BiboAnime.databaseLiteDB {

    public class databaseController {

        private string locationOfDataBase {
            set { }
            get {
                if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "/databaseLiteDB/database.liteDB")) {
                    if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "databaseLiteDB")) {
                        Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "databaseLiteDB");
                    }
                    File.Create(AppDomain.CurrentDomain.BaseDirectory + "/databaseLiteDB/database.liteDB");
                }
                return (AppDomain.CurrentDomain.BaseDirectory + "/databaseLiteDB/database.liteDB"); 
            }
        } 

        public int resetDataBase() {
            using (var db = new LiteDatabase(locationOfDataBase)) {
                var col = db.GetCollection<AnimeData>("Animes");
                return col.DeleteAll(); // how many was deleted
            }
        }

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

        /// <summary>
        /// Gets the number of rows already in the database PLUS 1!
        /// </summary>
        /// <returns>Count + 1</returns>
        public int getDbCountForIdPlusOne() {
            using (var db = new LiteDatabase(locationOfDataBase)) {
                var col = db.GetCollection<AnimeData>("Animes");
                return (col.Count() + 1);
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
                var col = db.GetCollection<AnimeData>("Animes");
                var result = col.Query()
                    .ToList();
                List<AnimeRow> rows = new List<AnimeRow>();
                for (int i = 0; i < result.Count; i += 1) {
                    string tagString = "";
                    for (int j = 0; j < result[i].tags.Count; j += 1) {
                        if (j != (result[i].tags.Count - 1)) {
                            tagString += result[i].tags[j] + ", ";
                        }
                        else {
                            tagString += result[i].tags[j];
                        }
                    }
                    rows.Add(new AnimeRow {
                        id = result[i].id,
                        favorite = result[i].favorite,
                        name = result[i].name,
                        status = result[i].status,
                        tags = tagString,
                    });
                }
                return rows;
            }
        }

        public AnimeData getAnimeById(int id) { 
            using (var db = new LiteDatabase(locationOfDataBase)) {
                var col = db.GetCollection<AnimeData>("Animes");
                var result = col.Query()
                    .Where(x => x.id == id)
                    .Single();
                return result;
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