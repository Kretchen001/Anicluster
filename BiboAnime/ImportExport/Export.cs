using BiboAnime.databaseLiteDB;
using BiboAnime.datatypes;

namespace BiboAnime {

    public static class Export {

        public static bool exportAnimeList(string filePath) {

            databaseController dbController = new databaseController();

            List<AnimeData> animesToExport = dbController.getAllAnimes();

            List<string> animeLines = new List<string>();
            for (int i = 0; i < animesToExport.Count(); i += 1) {
                animeLines.Add(convertToCsvLine(animesToExport[i])); //convertToCsvLine(AnimeData animeData)
            }

            File.WriteAllLines(filePath, animeLines);

            return false;
        }

        public static bool exportAnimeTags(string filePath) {
            try {
                databaseController dbController = new databaseController();

                List<AnimeTag> animeTagsToExport = dbController.getAllAnimeTags();

                List<string> tagLines = new List<string>();
                for (int i = 0; i < animeTagsToExport.Count(); i += 1) {
                    tagLines.Add(convertToCsvLine(animeTagsToExport[i])); //convertToCsvLine(AnimeTag animeTag)
                }

                File.WriteAllLines(filePath, tagLines);

                return true;
            }
            catch (Exception) {
                return false;
            }
        }

        private static string convertToCsvLine(AnimeData animeData) {
            string line = animeData.id + ";"
                + animeData.name + ";"
                + animeData.originalName + ";"
                + animeData.favorite.ToString() + ";"
                + animeData.rating.general.ToString() + ";"
                + animeData.rating.story.ToString() + ";"
                + animeData.rating.animation.ToString() + ";"
                + animeData.rating.sound.ToString() + ";"
                + animeData.rating.specialEffect.ToString() + ";"
                + animeData.rating.germanDub.ToString() + ";"
                + animeData.urlAnimePlanet + ";"
                + animeData.status.ToString() + ";"
                + animeData.staffeln.ToString() + ";"
                + animeData.episodesTotal.ToString() + ";"
                + animeData.movies.ToString() + ";"
                + animeData.ovh.ToString() + ";"
                + animeData.thirdPartyRecommendation.ToString() + ";"
                + animeData.tier.ToString()
                ;
            return line;
        }

        private static string convertToCsvLine(AnimeTag animeTag) {
            string line = animeTag.id + ";"
                + animeTag.tagDesignator;
            return line;
        }
    }
}
