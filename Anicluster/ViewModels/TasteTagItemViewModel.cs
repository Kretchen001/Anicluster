using Anicluster.TasteProfile;
using System.Globalization;

namespace Anicluster.ViewModels {

    /// <summary>
    /// Ein Tag in der Auswertung des Geschmacksprofils, aufbereitet für die Anzeige (Balken, Faktor, Erklärung).
    /// </summary>
    public class TasteTagItemViewModel {

        public int TagId { get; }

        public string Name { get; }

        /// <summary><c>true</c> = Tag aus der Liste „Eher nicht“, <c>false</c> = aus der Liste „Das magst du“</summary>
        public bool IsAvoided { get; }

        /// <summary>Faktor gegenüber dem Durchschnitt, z. B. „×2,1“</summary>
        public string LiftText { get; }

        /// <summary>Länge des Balkens von 0 bis 100</summary>
        public double BarValue { get; }

        /// <summary>Erklärung unter dem Balken</summary>
        public string DetailText { get; }

        public TasteTagItemViewModel(TagTaste taste, int likedAnimeCount, bool isAvoided) {
            this.TagId = taste.TagId;
            this.Name = taste.Name;
            this.IsAvoided = isAvoided;
            this.LiftText = "×" + taste.Lift.ToString("0.0", CultureInfo.CurrentCulture);
            if (isAvoided) {
                // Trefferquote: Wie viele der Animes mit diesem Tag gehören zu den Lieblingen?
                this.BarValue = 100.0 * taste.LikedCount / taste.TotalCount;
                this.DetailText = $"Nur {taste.LikedCount} von {taste.TotalCount} Animes mit diesem Tag gehören zu deinen Lieblingen.";
            }
            else {
                this.BarValue = 100.0 * taste.LikedShare;
                this.DetailText = $"Bei {Percent(taste.LikedShare)} % deiner {likedAnimeCount} Lieblinge, "
                    + $"in der ganzen Bibliothek nur bei {Percent(taste.LibraryShare)} %.";
            }
        }

        private static int Percent(double share) {
            return (int)Math.Round(share * 100);
        }
    }
}
