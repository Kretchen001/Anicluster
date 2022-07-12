using BiboAnime.databaseLiteDB;
using BiboAnime.datatypes;

namespace BiboAnime {

    public static class Import {

        public static bool importAnimeList() {

            return false;
        }

        public static void importAnimeTags(string filePath, bool importToDataBase) {

            databaseController dbController = new databaseController();

            List<string> importedTagList = File.ReadAllLines(filePath).ToList();

            if (importToDataBase) {
                dbController.resetDataBaseTags();
                dbController.addImportedTags(craftTagOutOfLine(importedTagList));
            }
            else {
            }
        }

        public static bool checkTagListValid(string filePath) {

            try {
                List<string> importedTagList = File.ReadAllLines(filePath).ToList();

                for (int i = 0; i < importedTagList.Count; i += 1) {
                    if (!(importedTagList[i][0..importedTagList[i].IndexOf(';')].All(char.IsNumber))) {
                        //one wrong
                        return false;
                    }
                }
                // all correct!
                return true;
            }
            catch (Exception) { 
                return false;
            }
        }

        private static List<AnimeTag> craftTagOutOfLine(List<string> csvLines) {

            List<AnimeTag> tagList = new List<AnimeTag>();

            for (int i = 0; i < csvLines.Count; i += 1) {
                Guid id = Guid.Parse(csvLines[i][0..csvLines[i].IndexOf(';')]);
                string tagDesignator = csvLines[i][csvLines[i].IndexOf(';')..csvLines[i].Length];
                AnimeTag animeTag = new AnimeTag() {
                    id = id,
                    tagDesignator = tagDesignator
                };
                tagList.Add(animeTag);
            }

            return tagList;
        }
    }
}