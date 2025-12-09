using Newtonsoft.Json;

namespace Modells.Anime {

    public class Anime : ValueComparableObject {

        /// <summary></summary>
        public int Id { get => this._id; set => this._id = this.SetProperty(ref this._id, value, this.OnPropertyChanged); }
        private int _id;
        /// <summary></summary>
        public string Name { get => this._name; set => this._name = this.SetProperty(ref this._name, value, this.OnPropertyChanged); }
        private string _name = "";

        /// <summary>Original Name des Animes</summary>
        public string? OriginalName { get => this._originalName; set => this._originalName = this.SetProperty(ref this._originalName, value, this.OnPropertyChanged); }
        private string? _originalName;

        /// <summary>URL des Animes (wenn vorhanden)</summary>
        public string? Url { get => this._url; set => this._url = this.SetProperty(ref this._url, value, this.OnPropertyChanged); }
        private string? _url;

        /// <summary>Favorisiert der Benutzer diesen Anime?</summary>
        public bool Favorite { get => this._favorite; set => this._favorite = this.SetProperty(ref this._favorite, value, this.OnPropertyChanged); }
        private bool _favorite;

        /// <summary>Bewertung des Animes</summary>
        public Rating Rating { get => this._rating; set => this._rating = this.SetProperty(ref this._rating, value, this.OnPropertyChanged); }
        private Rating _rating = new Rating();

        /// <summary>Die Tier-Einstufung des Animes</summary>
        public Tier Tier { get => this._tier; set => this._tier = this.SetProperty(ref this._tier, value, this.OnPropertyChanged); }
        private Tier _tier = Tier.NotDefined;

        /// <summary>Liste der Jahreszeiten (Seasons) des Animes</summary>
        public List<Season> Seasons { get => this._seasons; set => this._seasons = this.SetProperty(ref this._seasons, value, this.OnPropertyChanged); }
        private List<Season> _seasons = new List<Season>();

        /// <summary>Liste von OVAs, Filmen und anderen Video-Formaten</summary>
        public List<VideoAnimation> Ovas { get => this._ovas; set => this._ovas = this.SetProperty(ref this._ovas, value, this.OnPropertyChanged); }
        private List<VideoAnimation> _ovas = new List<VideoAnimation>();

        /// <summary>Status des Animes, z. B. "lizenziert", "läuft", etc.</summary>
        public Status Status { get => this._status; set => this._status = this.SetProperty(ref this._status, value, this.OnPropertyChanged); }
        private Status _status = new Status();

        /// <summary>Liste von Tags, die den Anime kategorisieren</summary>
        public List<Tag> Tags { get => this._tags; set => this._tags = this.SetProperty(ref this._tags, value, this.OnPropertyChanged); }
        private List<Tag> _tags = new List<Tag>();

        /// <summary>Zusätzliche Medieninformationen zum Anime</summary>
        public MediaInfo MediaInfo { get => this._mediaInfo; set => this._mediaInfo = this.SetProperty(ref this._mediaInfo, value, this.OnPropertyChanged); }
        private MediaInfo _mediaInfo = new MediaInfo();

        /// <summary>Wer hat diesen Anime empfohlen? (Optional)</summary>
        public string? RecommendedFrom { get => this._recommendedFrom; set => this._recommendedFrom = this.SetProperty(ref this._recommendedFrom, value, this.OnPropertyChanged); }
        private string? _recommendedFrom;

        /// <summary>Der Vorgänger-Anime (wenn vorhanden), durch die ID identifiziert</summary>
        public int Predecessor { get => this._predecessor; set => this._predecessor = this.SetProperty(ref this._predecessor, value, this.OnPropertyChanged); }
        private int _predecessor = -1;

        /// <summary>Der Nachfolger-Anime (wenn vorhanden), durch die ID identifiziert</summary>
        public int Successor { get => this._successor; set => this._successor = this.SetProperty(ref this._successor, value, this.OnPropertyChanged); }
        private int _successor = -1;

        /// <summary>Verwandte Animes, durch ID identifiziert</summary>
        public int Related { get => this._related; set => this._related = this.SetProperty(ref this._related, value, this.OnPropertyChanged); }
        private int _related = -1;

        /// <summary>Zusätzliche Kommentare oder Notizen zum Anime</summary>
        public string? Comment { get => this._comment; set => this._comment = this.SetProperty(ref this._comment, value, this.OnPropertyChanged); }
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

        [JsonIgnore]
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
            if (!this.isChanged) {
                this.isChanged = true;  // Markiere das Objekt als geändert
                FirstChangeOccurred?.Invoke(this, EventArgs.Empty); // Event auslösen
            }
        }
    }
}
