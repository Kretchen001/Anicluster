using BiboAnime.databaseLiteDB;
using BiboAnime.datatypes;
using LiteDB;

namespace PictureCheck {

    internal class Program {

        static string locationOfDataBaseTest {
            set { }
            get {
                if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "/databaseLiteDB/databaseTest.liteDB")) {
                    if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "databaseLiteDB")) {
                        Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "databaseLiteDB");
                    }
                    File.Create(AppDomain.CurrentDomain.BaseDirectory + "/databaseLiteDB/databaseTest.liteDB").Close();
                }
                return (AppDomain.CurrentDomain.BaseDirectory + "/databaseLiteDB/databaseTest.liteDB");
            }
        }
        static string locationOfDataBasePicTest {
            set { }
            get {
                if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "/databaseLiteDB/databasePicTest.liteDB")) {
                    if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "databaseLiteDB")) {
                        Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "databaseLiteDB");
                    }
                    File.Create(AppDomain.CurrentDomain.BaseDirectory + "/databaseLiteDB/databasePicTest.liteDB").Close();
                }
                return (AppDomain.CurrentDomain.BaseDirectory + "/databaseLiteDB/databasePicTest.liteDB");
            }
        }

        private static byte[] pictureAsFile(string path) {
            if (File.Exists(path)) {
                return File.ReadAllBytes(path);
            }
            else {
                return [];
            }
        }

        static void Main (string[] args) {
            Guid guid = Guid.NewGuid();
            string ground = AppDomain.CurrentDomain.BaseDirectory + "..\\..\\..\\..";
            string pathToImage = ground + "\\LiteDB\\demoBilder\\anime-landscape-person-traveling_23-2151038185.webp";
            AnimeData anime = new AnimeData() {
                id = guid,
            };
            byte[] b = pictureAsFile(pathToImage);
            AnimePoster animePoster = new AnimePoster() {
                Id = guid,
                Picture = b
            };

            using (LiteDatabase db = new LiteDatabase(locationOfDataBaseTest)) {
                ILiteCollection<AnimeData> col = db.GetCollection<AnimeData>("Animes");
                col.Insert(anime);
                col.EnsureIndex(x => x.id);
            }
            using (LiteDatabase db = new LiteDatabase(locationOfDataBasePicTest)) {
                ILiteCollection<AnimePoster> col = db.GetCollection<AnimePoster>("Poster");
                col.Insert(animePoster);
                col.EnsureIndex(x => x.Id);
            }

            Thread.Sleep(500);

            using (LiteDatabase db = new LiteDatabase(locationOfDataBasePicTest)) {
                ILiteCollection<AnimePoster> col = db.GetCollection<AnimePoster>("Poster");

                AnimePoster poster = col.FindById(guid);
                File.WriteAllBytes(ground + "\\LiteDB\\demoBilder\\___.jpg", poster.Picture);
            }
        }
    }
}
