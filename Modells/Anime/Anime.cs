using Newtonsoft.Json;

namespace Modells.Anime {

    public class Anime : ValueComparableObject {

        /// <summary></summary>
        public int Id { get => _id; set => _id = SetProperty(ref _id, value, OnPropertyChanged); }
        private int _id;
        /// <summary></summary>
        public string Name { get => _name; set => _name = SetProperty(ref _name, value, OnPropertyChanged); }
        private string _name = "";

        /// <summary>Original Name des Animes</summary>
        public string? OriginalName { get => _originalName; set => _originalName = SetProperty(ref _originalName, value, OnPropertyChanged); }
        private string? _originalName;

        /// <summary>URL des Animes (wenn vorhanden)</summary>
        public string? Url { get => _url; set => _url = SetProperty(ref _url, value, OnPropertyChanged); }
        private string? _url;

        /// <summary>Favorisiert der Benutzer diesen Anime?</summary>
        public bool Favorite { get => _favorite; set => _favorite = SetProperty(ref _favorite, value, OnPropertyChanged); }
        private bool _favorite;

        /// <summary>Bewertung des Animes</summary>
        public Rating Rating { get => _rating; set => _rating = SetProperty(ref _rating, value, OnPropertyChanged); }
        private Rating _rating = new Rating();

        /// <summary>Die Tier-Einstufung des Animes</summary>
        public Tier Tier { get => _tier; set => _tier = SetProperty(ref _tier, value, OnPropertyChanged); }
        private Tier _tier = Tier.NotDefined;

        /// <summary>Liste der Jahreszeiten (Seasons) des Animes</summary>
        public List<Season> Seasons { get => _seasons; set => _seasons = SetProperty(ref _seasons, value, OnPropertyChanged); }
        private List<Season> _seasons = new List<Season>();

        /// <summary>Liste von OVAs, Filmen und anderen Video-Formaten</summary>
        public List<VideoAnimation> Ovas { get => _ovas; set => _ovas = SetProperty(ref _ovas, value, OnPropertyChanged); }
        private List<VideoAnimation> _ovas = new List<VideoAnimation>();

        /// <summary>Status des Animes, z. B. "lizenziert", "läuft", etc.</summary>
        public Status Status { get => _status; set => _status = SetProperty(ref _status, value, OnPropertyChanged); }
        private Status _status = new Status();

        /// <summary>Liste von Tags, die den Anime kategorisieren</summary>
        public List<Tag> Tags { get => _tags; set => _tags = SetProperty(ref _tags, value, OnPropertyChanged); }
        private List<Tag> _tags = new List<Tag>();

        /// <summary>Zusätzliche Medieninformationen zum Anime</summary>
        public MediaInfo MediaInfo { get => _mediaInfo; set => _mediaInfo = SetProperty(ref _mediaInfo, value, OnPropertyChanged); }
        private MediaInfo _mediaInfo = new MediaInfo();

        /// <summary>Wer hat diesen Anime empfohlen? (Optional)</summary>
        public string? RecommendedFrom { get => _recommendedFrom; set => _recommendedFrom = SetProperty(ref _recommendedFrom, value, OnPropertyChanged); }
        private string? _recommendedFrom;

        /// <summary>Der Vorgänger-Anime (wenn vorhanden), durch die ID identifiziert</summary>
        public int Predecessor { get => _predecessor; set => _predecessor = SetProperty(ref _predecessor, value, OnPropertyChanged); }
        private int _predecessor = -1;

        /// <summary>Der Nachfolger-Anime (wenn vorhanden), durch die ID identifiziert</summary>
        public int Successor { get => _successor; set => _successor = SetProperty(ref _successor, value, OnPropertyChanged); }
        private int _successor = -1;

        /// <summary>Verwandte Animes, durch ID identifiziert</summary>
        public int Related { get => _related; set => _related = SetProperty(ref _related, value, OnPropertyChanged); }
        private int _related = -1;

        /// <summary>Zusätzliche Kommentare oder Notizen zum Anime</summary>
        public string? Comment { get => _comment; set => _comment = SetProperty(ref _comment, value, OnPropertyChanged); }
        private string? _comment;

        #region ctor
        /// <summary>empty ctor</summary>
        public Anime() { }
        #endregion

        /// <summary></summary>
        public Anime DeepClone() {
            string temp = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<Anime>(temp)!;
        }

        private bool isChanged = false;  // Flag, um die erste Änderung zu erkennen
        public void ResetFirstChangeBool() {
            this.isChanged = false;
        }
        
        // Das Event, das nach der ersten Änderung ausgelöst wird
        public event EventHandler FirstChangeOccurred;
        
        // Generische Methode, um Eigenschaften automatisch zu überwachen
        private T SetProperty<T>(ref T field, T value, Action? onChanged = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                onChanged?.Invoke();  // Event auslösen, wenn eine Änderung erfolgt
            }
            return field;
        }

        // Ändern der Eigenschaften mit einer Überprüfung der ersten Änderung
        private void OnPropertyChanged() {
            if (!isChanged) {
                this.isChanged = true;  // Markiere das Objekt als geändert
                FirstChangeOccurred?.Invoke(this, EventArgs.Empty); // Event auslösen
            }
        }
    }
}
