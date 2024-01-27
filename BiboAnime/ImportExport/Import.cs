using BiboAnime.databaseLiteDB;
using BiboAnime.datatypes;
using Newtonsoft.Json;

namespace BiboAnime {

    public static class Import {

        public static bool ImportAnimeList(string filePath, bool importAddToDatabase) {
            try {
                string json = File.ReadAllText(filePath);
                List<AnimeData> animesToImport = JsonConvert.DeserializeObject<List<AnimeData>>(json)!;
                
                DatabaseController dbController = new DatabaseController();
                if (importAddToDatabase) { // hinzufügen
                    DatabaseController.AddImportedAnimes(animesToImport);
                    return true;
                }
                else { // sonst ersetzten
                    DatabaseController.ResetDataBaseAnimes();
                    DatabaseController.AddImportedAnimes(animesToImport);
                    return true;
                }
            }
            catch {
                return false;
            }
        }
        public static bool ImportAnimeTags(string filePath, bool importAddToDatabase) {
            try {
                string json = File.ReadAllText(filePath);
                List<AnimeTag> animesToImport = JsonConvert.DeserializeObject<List<AnimeTag>>(json)!;

                DatabaseController dbController = new DatabaseController();
                if (importAddToDatabase) { // hinzufügen
                    dbController.AddImportedTags(animesToImport);
                    return true;
                }
                else { // sonst ersetzten
                    dbController.ResetDataBaseTags();
                    dbController.AddImportedTags(animesToImport);
                    return true;
                }
            }
            catch {
                return false;
            }
        }

        #region Ungenutzter Code
        /*
        public static void importAnimeTags(string filePath, bool importToDatabase) {

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
        */
        #endregion
    }
}