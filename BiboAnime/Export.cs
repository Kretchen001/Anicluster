using BiboAnime.databaseLiteDB;
using BiboAnime.datatypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiboAnime {

    public static class Export {

        public static bool exportAnimeList() {

            databaseController dbController = new databaseController();

            List<AnimeData> animesToExport = dbController.getAllAnimes();

            List<string> line = new List<string>();
            for (int i = 0; i < animesToExport.Count(); i += 1) {
                line.Add(convertToCsvLine(animesToExport[i]));
            }
            return false;
        }

        public static bool exportAnimeTags() {

            return false;
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
    }
}
