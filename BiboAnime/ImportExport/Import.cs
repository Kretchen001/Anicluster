using BiboAnime.databaseLiteDB;

namespace BiboAnime {

    public static class Import {

        public static bool importAnimeList() {

            return false;
        }

        public static bool importAnimeTags(string filePath, bool importToDataBase) {

            databaseController dbController = new databaseController();

            List<string> importedTagList = File.ReadAllLines(filePath).ToList();

            if (importToDataBase) {

            }
            else {

            }

            return false;
        }
    }
}