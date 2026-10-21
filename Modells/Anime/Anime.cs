using Newtonsoft.Json;

namespace Modells.Anime {

    /// <summary>
    /// Ein Anime mit allen zugehörigen Daten (Bewertung, Staffeln, Tags, Medieninfos, ...).
    /// Reine Datenklasse, die Änderungsverfolgung übernimmt das ViewModel.
    /// </summary>
    public class Anime : ValueComparableObject {

        public int Id { get; set; }

        public string Name { get; set; } = "";

        /// <summary>Originalname des Animes</summary>
        public string? OriginalName { get; set; }

        /// <summary>URL des Animes (wenn vorhanden)</summary>
        public string? Url { get; set; }

        /// <summary>Favorisiert der Benutzer diesen Anime?</summary>
        public bool Favorite { get; set; }

        /// <summary>Bewertung des Animes</summary>
        public Rating Rating { get; set; } = new Rating();

        /// <summary>Die Tier-Einstufung des Animes</summary>
        public Tier Tier { get; set; } = Tier.NotDefined;

        /// <summary>Liste der Staffeln des Animes</summary>
        public List<Season> Seasons { get; set; } = new List<Season>();

        /// <summary>Liste von OVAs, Filmen und anderen Video-Formaten</summary>
        public List<VideoAnimation> Ovas { get; set; } = new List<VideoAnimation>();

        /// <summary>Status des Animes, z. B. "Wunschliste" oder "Begonnen"</summary>
        public Status Status { get; set; } = new Status();

        /// <summary>Liste von Tags, die den Anime kategorisieren</summary>
        public List<Tag> Tags { get; set; } = new List<Tag>();

        /// <summary>Zusätzliche Medieninformationen zum Anime</summary>
        public MediaInfo MediaInfo { get; set; } = new MediaInfo();

        /// <summary>Wer hat diesen Anime empfohlen? (optional)</summary>
        public string? RecommendedFrom { get; set; }

        /// <summary>Id des Vorgänger-Animes, -1 wenn keiner vorhanden</summary>
        public int Predecessor { get; set; } = -1;

        /// <summary>Id des Nachfolger-Animes, -1 wenn keiner vorhanden</summary>
        public int Successor { get; set; } = -1;

        /// <summary>Id eines sonst zusammenhängenden Animes, -1 wenn keiner vorhanden</summary>
        public int Related { get; set; } = -1;

        /// <summary>Zusätzliche Kommentare oder Notizen zum Anime</summary>
        public string? Comment { get; set; }

        /// <summary>Ausführliche Beschreibung (zum Beispiel aus einer Online-Quelle übernommen und vom Benutzer änderbar)</summary>
        public string? Description { get; set; }

        /// <summary>Id des Eintrags bei AniList, <c>null</c> wenn der Anime dort nicht verknüpft ist</summary>
        public int? AniListId { get; set; }

        /// <summary>Id des Eintrags bei MyAnimeList (auch die Id bei Jikan), <c>null</c> wenn nicht verknüpft</summary>
        public int? MyAnimeListId { get; set; }

        /// <summary>Id des Eintrags bei Kitsu, <c>null</c> wenn nicht verknüpft</summary>
        public int? KitsuId { get; set; }

        /// <summary>Das Cover des Animes, <c>null</c> wenn keins hinterlegt ist</summary>
        public AnimeImage? Image { get; set; }

        public Anime() { }

        /// <summary>Erzeugt eine tiefe Kopie über Serialisierung.</summary>
        public Anime DeepClone() {
            string temp = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<Anime>(temp)!;
        }
    }
}
