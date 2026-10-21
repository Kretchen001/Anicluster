using Anicluster.AnimeNetwork;
using Anicluster.Database.Services;
using Anicluster.Settings;
using Anicluster.TagNetwork;
using Modells.Anime;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace Anicluster.ViewModels {

    /// <summary>
    /// Das Anime-Netzwerk: Animes als Knoten in drei Netzwerkarten (Vorgänger/Nachfolger/Verwandt, ähnliche Tags, ähnliche Bewertung).
    /// Verbundene Animes rücken zusammen, Cluster werden als Gruppe hervorgehoben. Die Berechnung der Verbindungen und Cluster übernimmt
    /// der <see cref="AnimeNetworkBuilder"/>, dieses ViewModel hält die Einstellungen, die Knoten und die Physik-Simulation.
    /// </summary>
    public class AnimeNetworkViewModel : PropertyBaseChange, IDisposable {

        private const double MinDisplayScale = 0.35;
        private const double TargetCoverage = 0.14;
        private const double MinZoom = 1.0;
        private const double MaxZoom = 8.0;
        /// <summary>Faktor pro Mausrad-Stufe (multiplikativ, damit sich jede Stufe gleich anfühlt)</summary>
        private const double ZoomFactor = 1.25;
        private const double ViewportInset = 2.0;
        private const int MinCaptionMembers = 3;
        private const int MaxNeighborCount = 15;

        private readonly ForceSimulation _simulation = new ForceSimulation();
        private readonly DispatcherTimer _timer;
        private readonly IAnimeService _animeService;
        private readonly AppSettingsService _settingsService;
        private readonly List<AnimeListEntry> _animes;
        private readonly List<AnimeRelationEntry> _relations;
        private readonly List<AnimeRatingEntry> _ratings = [];
        private readonly Dictionary<int, AnimeRatingEntry> _ratingsById = [];
        private readonly Dictionary<int, string> _tagNames = [];
        private readonly Dictionary<int, AnimeListEntry> _entriesById = [];
        private readonly Dictionary<int, AnimeNodeViewModel> _nodesById = [];
        private readonly Dictionary<int, List<AnimeEdgeViewModel>> _edgesByNode = [];
        private readonly Random _random = new Random(11);
        private AnimeNodeViewModel? _hovered;
        private AnimeNodeViewModel? _selected;
        private bool _hasLayout;
        private double _viewportWidth;
        private double _viewportHeight;
        private double _worldWidth = 900;
        private double _worldHeight = 600;
        private double _zoom = MinZoom;
        private AnimeNetworkMode _mode;
        private int _neighborCount;
        private bool _showUnlinked;
        private LibraryStatusFilter _statusFilter = LibraryStatusFilter.All;
        private string _searchText = string.Empty;
        private string _searchResultText = string.Empty;
        private int _visibleCount;
        private string _infoText = string.Empty;
        private string _hintText = string.Empty;
        private IReadOnlyList<AnimeEdgeViewModel> _edges = [];
        private IReadOnlyList<AnimeClusterViewModel> _clusters = [];

        /// <summary>Alle Animes als Knoten. Welche davon sichtbar sind, steht in <see cref="AnimeNodeViewModel.IsVisibleInGraph"/>.</summary>
        public ObservableCollection<AnimeNodeViewModel> Nodes { get; } = [];

        /// <summary>Die Verbindungen der aktuellen Netzwerkart (wird bei jeder Neuberechnung ersetzt)</summary>
        public IReadOnlyList<AnimeEdgeViewModel> Edges {
            get => this._edges;
            private set => this.SetProperty(ref this._edges, value);
        }

        /// <summary>Die Cluster der aktuellen Netzwerkart (wird bei jeder Neuberechnung ersetzt)</summary>
        public IReadOnlyList<AnimeClusterViewModel> Clusters {
            get => this._clusters;
            private set => this.SetProperty(ref this._clusters, value);
        }

        /// <summary>Die mit dem gewählten Anime verbundenen Animes, die stärksten zuerst</summary>
        public ObservableCollection<AnimeNeighborViewModel> Neighbors { get; } = [];

        /// <summary>Wird bei jedem Schritt der Simulation und bei Änderungen der Hervorhebung ausgelöst, damit die Linien neu gezeichnet werden.</summary>
        public event EventHandler? Redraw;

        /// <summary>Ein Anime soll zum Bearbeiten geöffnet werden (Id des Animes).</summary>
        public event EventHandler<int>? EditRequested;

        /// <summary>Die Bibliothek soll nach dem Namen eines Animes durchsucht werden.</summary>
        public event EventHandler<string>? SearchInLibraryRequested;

        #region Fläche und Zoom
        /// <summary>Größe der Fläche, auf der simuliert wird. Bei vielen Knoten ist sie größer als der sichtbare Bereich.</summary>
        public double WorldWidth {
            get => this._worldWidth;
            private set => this.SetProperty(ref this._worldWidth, value);
        }

        public double WorldHeight {
            get => this._worldHeight;
            private set => this.SetProperty(ref this._worldHeight, value);
        }

        /// <summary>Vergrößerung der Darstellung von 1 (alles im sichtbaren Bereich) bis 8. Darüber hinaus wird gescrollt.</summary>
        public double Zoom {
            get => this._zoom;
            set {
                if (this.SetProperty(ref this._zoom, Math.Clamp(value, MinZoom, MaxZoom))) {
                    this.OnPropertyChanged(nameof(this.ViewWidth));
                    this.OnPropertyChanged(nameof(this.ViewHeight));
                    this.OnPropertyChanged(nameof(this.ZoomText));
                }
            }
        }

        public string ZoomText => $"{Math.Round(this.Zoom * 100)} %";

        /// <summary>Größe der dargestellten Fläche in Pixeln (sichtbarer Bereich mal Zoom)</summary>
        public double ViewWidth => Math.Max(0.0, this._viewportWidth - ViewportInset) * this.Zoom;

        public double ViewHeight => Math.Max(0.0, this._viewportHeight - ViewportInset) * this.Zoom;
        #endregion

        #region Netzwerkart und Einstellungen
        public AnimeNetworkMode Mode {
            get => this._mode;
            set {
                if (this.SetProperty(ref this._mode, value)) {
                    this._settingsService.Current.AnimeNetworkMode = value;
                    this._settingsService.Save();
                    this.OnPropertyChanged(nameof(this.IsRelationsMode));
                    this.OnPropertyChanged(nameof(this.IsSimilarityMode));
                    this.OnPropertyChanged(nameof(this.SimilarityPercent));
                    this.OnPropertyChanged(nameof(this.ModeDescription));
                    this.Rebuild(true);
                }
            }
        }

        public bool IsRelationsMode => this._mode == AnimeNetworkMode.Relations;

        /// <summary>Bei Tags und Bewertung gibt es Regler für Ähnlichkeit und Nachbarn.</summary>
        public bool IsSimilarityMode => this._mode != AnimeNetworkMode.Relations;

        public string ModeDescription => this._mode switch {
            AnimeNetworkMode.Relations => "Animes sind verbunden, wenn sie bei Vorgänger, Nachfolger oder Verwandt eingetragen sind. "
                + "Der Pfeil zeigt vom Vorgänger zum Nachfolger, gestrichelte Linien verbinden Verwandte.",
            AnimeNetworkMode.Tags => "Animes mit ähnlichen Tags rücken zusammen. Seltene gemeinsame Tags zählen mehr als häufige. "
                + "Animes ohne Tags erscheinen nicht.",
            _ => "Animes mit ähnlichen Teilnoten (Story, Animation, Effekte, Akustik) rücken zusammen. Unbewertete Animes erscheinen nicht."
        };

        /// <summary>Mindestähnlichkeit in Prozent. Für Tags und Bewertung wird je ein eigener Wert gespeichert.</summary>
        public int SimilarityPercent {
            get => this._mode == AnimeNetworkMode.Ratings
                ? this._settingsService.Current.AnimeNetworkRatingSimilarity
                : this._settingsService.Current.AnimeNetworkTagSimilarity;
            set {
                int clamped = Math.Clamp(value, 0, 100);
                if (clamped == this.SimilarityPercent) {
                    return;
                }
                if (this._mode == AnimeNetworkMode.Ratings) {
                    this._settingsService.Current.AnimeNetworkRatingSimilarity = clamped;
                }
                else {
                    this._settingsService.Current.AnimeNetworkTagSimilarity = clamped;
                }
                this._settingsService.Save();
                this.OnPropertyChanged();
                this.Rebuild(false);
            }
        }

        /// <summary>Wie viele ähnlichste Nachbarn jeder Anime höchstens behält</summary>
        public int NeighborCount {
            get => this._neighborCount;
            set {
                if (this.SetProperty(ref this._neighborCount, Math.Clamp(value, 1, MaxNeighborCount))) {
                    this._settingsService.Current.AnimeNetworkNeighborCount = this._neighborCount;
                    this._settingsService.Save();
                    this.Rebuild(false);
                }
            }
        }

        /// <summary>Animes, die in der gewählten Netzwerkart keine Verbindung haben, trotzdem anzeigen</summary>
        public bool ShowUnlinked {
            get => this._showUnlinked;
            set {
                if (this.SetProperty(ref this._showUnlinked, value)) {
                    this._settingsService.Current.AnimeNetworkShowUnlinked = value;
                    this._settingsService.Save();
                    this.Rebuild(false);
                }
            }
        }

        /// <summary>Nur Animes mit diesem Status nehmen am Netzwerk teil.</summary>
        public LibraryStatusFilter StatusFilter {
            get => this._statusFilter;
            set {
                if (this.SetProperty(ref this._statusFilter, value)) {
                    this.Rebuild(false);
                }
            }
        }

        /// <summary>Hebt Animes hervor, deren Name den Text enthält, alle anderen werden abgeblendet.</summary>
        public string SearchText {
            get => this._searchText;
            set {
                if (this.SetProperty(ref this._searchText, value)) {
                    this.RefreshHighlighting();
                }
            }
        }

        public string SearchResultText {
            get => this._searchResultText;
            private set => this.SetProperty(ref this._searchResultText, value);
        }
        #endregion

        #region Anzeige der Ergebnisse
        public string InfoText {
            get => this._infoText;
            private set => this.SetProperty(ref this._infoText, value);
        }

        /// <summary>Hinweise, was nicht dargestellt wird (ohne Tags, unbewertet, ohne Verbindung)</summary>
        public string HintText {
            get => this._hintText;
            private set {
                if (this.SetProperty(ref this._hintText, value)) {
                    this.OnPropertyChanged(nameof(this.HasHint));
                }
            }
        }

        public bool HasHint => this._hintText.Length > 0;

        public int VisibleCount {
            get => this._visibleCount;
            private set {
                if (this.SetProperty(ref this._visibleCount, value)) {
                    this.OnPropertyChanged(nameof(this.IsEmpty));
                }
            }
        }

        public bool IsEmpty => this._visibleCount == 0;

        public string EmptyText => this._mode == AnimeNetworkMode.Relations
            ? "Keine Verknüpfungen gefunden. Vorgänger, Nachfolger und Verwandte wählst du im Editor eines Animes aus."
            : "Mit diesen Einstellungen gibt es keine Verbindungen. Senke die Mindestähnlichkeit oder zeige Animes ohne Verbindung an.";
        #endregion

        #region Auswahl
        public bool HasSelection => this._selected is not null;

        /// <summary>Der gewählte Anime für die Detailansicht (Tier und Status wie in den Listen)</summary>
        public AnimeListItemViewModel? SelectedItem { get; private set; }

        public string SelectedTagsText { get; private set; } = string.Empty;

        public string SelectedRatingText { get; private set; } = string.Empty;

        public string SelectedClusterText { get; private set; } = string.Empty;

        public string NeighborsText => this.Neighbors.Count == 1 ? "1 Verbindung" : $"{this.Neighbors.Count} Verbindungen";
        #endregion

        public AnimeNetworkViewModel(IAnimeService animeService, ITagService tagService, AppSettingsService settingsService) {
            this._animeService = animeService;
            this._settingsService = settingsService;
            this._animes = animeService.SelectAnimeListEntries();
            this._relations = animeService.SelectRelationEntries();
            foreach (AnimeRatingEntry rating in animeService.SelectRatingEntries()) {
                this._ratings.Add(rating);
                this._ratingsById[rating.Id] = rating;
            }
            foreach (TagUsage usage in tagService.SelectTagUsage()) {
                this._tagNames[usage.Id] = usage.Designation;
            }

            this._mode = settingsService.Current.AnimeNetworkMode;
            this._neighborCount = Math.Clamp(settingsService.Current.AnimeNetworkNeighborCount, 1, MaxNeighborCount);
            this._showUnlinked = settingsService.Current.AnimeNetworkShowUnlinked;

            foreach (AnimeListEntry entry in this._animes) {
                AnimeNodeViewModel node = new AnimeNodeViewModel(entry);
                this.Nodes.Add(node);
                this._nodesById[node.Id] = node;
                this._entriesById[node.Id] = entry;
                this._simulation.Nodes.Add(node.Node);
            }

            this._timer = new DispatcherTimer(DispatcherPriority.Render) { Interval = TimeSpan.FromMilliseconds(16) };
            this._timer.Tick += this.OnTimerTick;

            this.Rebuild(false);
        }

        #region Darstellung und Simulation
        /// <summary>Teilt die Größe des sichtbaren Bereichs mit. Beim ersten Mal werden die Knoten verteilt.</summary>
        public void SetViewport(double width, double height) {
            if (width < 100 || height < 100) {
                return;
            }
            this._viewportWidth = width;
            this._viewportHeight = height;
            this.OnPropertyChanged(nameof(this.ViewWidth));
            this.OnPropertyChanged(nameof(this.ViewHeight));
            this.UpdateWorldSize();
            if (!this._hasLayout) {
                this._hasLayout = true;
                this.PlaceVisibleNodes();
                this._simulation.Restart();
                this.StartTimer();
            }
        }

        /// <summary>Verteilt alle sichtbaren Knoten neu und startet die Simulation von vorn.</summary>
        public void Relayout() {
            if (!this._hasLayout) {
                return;
            }
            this.PlaceVisibleNodes();
            this._simulation.Restart();
            this.StartTimer();
        }

        /// <summary>Ändert den Zoom um die angegebene Anzahl Stufen (positiv = größer).</summary>
        public void ZoomBy(int steps) {
            this.Zoom *= Math.Pow(ZoomFactor, steps);
        }

        /// <summary>Zeigt wieder das ganze Netzwerk im sichtbaren Bereich.</summary>
        public void ResetZoom() {
            this.Zoom = MinZoom;
        }

        /// <summary>Hält einen Knoten beim Ziehen an der Mausposition fest (in Koordinaten der Fläche).</summary>
        public void DragNode(AnimeNodeViewModel node, double x, double y) {
            node.Node.IsPinned = true;
            node.Node.X = Math.Min(Math.Max(x, node.Width / 2.0), this.WorldWidth - node.Width / 2.0);
            node.Node.Y = Math.Min(Math.Max(y, node.Height / 2.0), this.WorldHeight - node.Height / 2.0);
            node.NotifyPositionChanged();
            this._simulation.Reheat(0.3);
            this.StartTimer();
            this.Redraw?.Invoke(this, EventArgs.Empty);
        }

        public void EndDrag(AnimeNodeViewModel node) {
            node.Node.IsPinned = false;
        }

        public void Dispose() {
            this._timer.Stop();
            this._timer.Tick -= this.OnTimerTick;
        }

        private void OnTimerTick(object? sender, EventArgs e) {
            this._simulation.Step();
            foreach (AnimeNodeViewModel node in this.Nodes) {
                if (node.IsVisibleInGraph) {
                    node.NotifyPositionChanged();
                }
            }
            this.Redraw?.Invoke(this, EventArgs.Empty);
            if (this._simulation.IsSettled) {
                this._timer.Stop();
            }
        }

        private void StartTimer() {
            if (!this._timer.IsEnabled) {
                this._timer.Start();
            }
        }

        /// <summary>
        /// Anfangsverteilung: Jeder Cluster bekommt seinen Platz auf einer Spirale um die Mitte, seine Mitglieder liegen zunächst nah beieinander.
        /// Animes ohne Verbindung liegen außen herum.
        /// </summary>
        private void PlaceVisibleNodes() {
            int unlinkedIndex = 0;
            IEnumerable<AnimeNodeViewModel> ordered = this.Nodes
                .Where(x => x.IsVisibleInGraph)
                .OrderBy(x => x.ClusterId)
                .ThenBy(x => x.Name, StringComparer.CurrentCultureIgnoreCase);
            foreach (AnimeNodeViewModel node in ordered) {
                double centerX;
                double centerY;
                double spread;
                if (node.IsLinked) {
                    double angle = node.ClusterId * 2.399963;
                    double radius = 40 + 70 * Math.Sqrt(node.ClusterId);
                    centerX = this.WorldWidth / 2.0 + Math.Cos(angle) * radius;
                    centerY = this.WorldHeight / 2.0 + Math.Sin(angle) * radius;
                    spread = 70;
                }
                else {
                    double angle = unlinkedIndex * 2.399963;
                    double radius = 150 + 30 * Math.Sqrt(unlinkedIndex);
                    centerX = this.WorldWidth / 2.0 + Math.Cos(angle) * radius;
                    centerY = this.WorldHeight / 2.0 + Math.Sin(angle) * radius;
                    spread = 0;
                    unlinkedIndex += 1;
                }
                node.Node.X = centerX + (this._random.NextDouble() - 0.5) * spread;
                node.Node.Y = centerY + (this._random.NextDouble() - 0.5) * spread;
                node.Node.Vx = 0;
                node.Node.Vy = 0;
                node.NotifyPositionChanged();
            }
        }

        /// <summary>Ein neu sichtbarer Anime erscheint in der Nähe seiner bereits sichtbaren Nachbarn (sonst nahe der Mitte).</summary>
        private void PlaceNearNeighbors(AnimeNodeViewModel node, HashSet<int> previouslyVisible) {
            List<AnimeNodeViewModel> anchors = [];
            foreach (AnimeEdgeViewModel edge in this.GetEdges(node.Id)) {
                AnimeNodeViewModel other = edge.A == node ? edge.B : edge.A;
                if (other.IsVisibleInGraph && previouslyVisible.Contains(other.Id)) {
                    anchors.Add(other);
                }
            }
            double x = this.WorldWidth / 2.0;
            double y = this.WorldHeight / 2.0;
            double spread = 160;
            if (anchors.Count > 0) {
                x = anchors.Average(n => n.CenterX);
                y = anchors.Average(n => n.CenterY);
                spread = 70;
            }
            node.Node.X = x + (this._random.NextDouble() - 0.5) * spread;
            node.Node.Y = y + (this._random.NextDouble() - 0.5) * spread;
            node.Node.Vx = 0;
            node.Node.Vy = 0;
            node.NotifyPositionChanged();
        }

        /// <summary>
        /// Bei vielen sichtbaren Knoten wird die Fläche größer gewählt, damit die Knoten Platz haben, ohne sich zu überlappen.
        /// Der Zoom gleicht das aus: Bei 100 % passt alles in den sichtbaren Bereich, darüber wird gescrollt.
        /// </summary>
        private void UpdateWorldSize() {
            if (this._viewportWidth <= 0) {
                return;
            }
            double area = this.Nodes.Where(x => x.IsVisibleInGraph).Sum(x => x.Width * x.Height);
            double factor = Math.Sqrt(area / (TargetCoverage * this._viewportWidth * this._viewportHeight));
            factor = Math.Min(1.0 / MinDisplayScale, Math.Max(1.0, factor));
            double newWidth = this._viewportWidth * factor;
            double newHeight = this._viewportHeight * factor;
            if (this._hasLayout && this.WorldWidth > 0 && this.WorldHeight > 0
                && (Math.Abs(newWidth - this.WorldWidth) > 1 || Math.Abs(newHeight - this.WorldHeight) > 1)) {
                // Vorhandene Positionen mit der Fläche mitnehmen, damit das Layout erhalten bleibt
                double scaleX = newWidth / this.WorldWidth;
                double scaleY = newHeight / this.WorldHeight;
                foreach (AnimeNodeViewModel node in this.Nodes) {
                    node.Node.X *= scaleX;
                    node.Node.Y *= scaleY;
                    node.NotifyPositionChanged();
                }
            }
            this.WorldWidth = newWidth;
            this.WorldHeight = newHeight;
            this._simulation.Width = newWidth;
            this._simulation.Height = newHeight;
        }
        #endregion

        #region Netzwerk berechnen
        /// <summary>
        /// Berechnet Verbindungen und Cluster neu und übernimmt sie ins Netzwerk. Mit <paramref name="relayout"/> werden alle Knoten
        /// neu verteilt (bei Wechsel der Netzwerkart), sonst bleiben die Positionen erhalten und neu sichtbare Knoten erscheinen bei ihren Nachbarn.
        /// </summary>
        private void Rebuild(bool relayout) {
            List<AnimeListEntry> animes = this._animes.Where(x => this.MatchesStatus(x.State)).ToList();
            HashSet<int> animeIds = animes.Select(x => x.Id).ToHashSet();
            AnimeNetworkResult result = this.Calculate(animes, animeIds);
            HashSet<int> eligibleIds = result.EligibleIds.ToHashSet();
            HashSet<int> previouslyVisible = this.Nodes.Where(x => x.IsVisibleInGraph).Select(x => x.Id).ToHashSet();

            // Verbindungen und Federn der Simulation
            this._edgesByNode.Clear();
            this._simulation.Links.Clear();
            List<AnimeEdgeViewModel> edges = [];
            foreach (AnimeNetworkEdge edge in result.Edges) {
                if (!this._nodesById.TryGetValue(edge.IdA, out AnimeNodeViewModel? a)
                    || !this._nodesById.TryGetValue(edge.IdB, out AnimeNodeViewModel? b)) {
                    continue;
                }
                AnimeEdgeViewModel edgeViewModel = new AnimeEdgeViewModel { A = a, B = b, Weight = edge.Weight, Kind = edge.Kind };
                edges.Add(edgeViewModel);
                this.AddEdgeToNode(a.Id, edgeViewModel);
                this.AddEdgeToNode(b.Id, edgeViewModel);
                this._simulation.Links.Add(new ForceLink { A = a.Node, B = b.Node, Strength = this.GetLinkStrength(edge) });
            }

            // Sichtbarkeit: Daten vorhanden und verbunden (oder "ohne Verbindung" eingeschaltet)
            List<AnimeNodeViewModel> newlyVisible = [];
            foreach (AnimeNodeViewModel node in this.Nodes) {
                bool isLinked = result.Clusters.TryGetValue(node.Id, out int clusterId);
                node.IsLinked = isLinked;
                node.ClusterId = isLinked ? clusterId : -1;
                bool isVisible = eligibleIds.Contains(node.Id) && (isLinked || this.ShowUnlinked);
                if (isVisible && !previouslyVisible.Contains(node.Id)) {
                    newlyVisible.Add(node);
                }
                node.IsVisibleInGraph = isVisible;
                node.Node.IsActive = isVisible;
            }
            this.Edges = edges;
            this.Clusters = this.CreateClusters();
            this.VisibleCount = this.Nodes.Count(x => x.IsVisibleInGraph);
            this.UpdateTexts(animes.Count, eligibleIds.Count, result);
            this.OnPropertyChanged(nameof(this.EmptyText));

            this.UpdateWorldSize();
            if (this._hasLayout) {
                if (relayout) {
                    this.PlaceVisibleNodes();
                    this._simulation.Restart();
                }
                else {
                    foreach (AnimeNodeViewModel node in newlyVisible) {
                        this.PlaceNearNeighbors(node, previouslyVisible);
                    }
                    this._simulation.Reheat(0.6);
                }
                this.StartTimer();
            }

            if (this._selected is not null && !this._selected.IsVisibleInGraph) {
                this.SelectNode(null);
            }
            else {
                this.UpdateSelectedDetails();
                this.UpdateNeighbors();
                this.RefreshHighlighting();
            }
        }

        private AnimeNetworkResult Calculate(List<AnimeListEntry> animes, HashSet<int> animeIds) {
            double minSimilarity = this.SimilarityPercent / 100.0;
            return this._mode switch {
                AnimeNetworkMode.Tags => AnimeNetworkBuilder.BuildTags(animes, minSimilarity, this.NeighborCount),
                AnimeNetworkMode.Ratings => AnimeNetworkBuilder.BuildRatings(
                    this._ratings.Where(x => animeIds.Contains(x.Id)).ToList(),
                    minSimilarity,
                    this.NeighborCount
                ),
                _ => AnimeNetworkBuilder.BuildRelations(this._relations.Where(x => animeIds.Contains(x.Id)).ToList())
            };
        }

        /// <summary>Stärke der Feder in der Simulation. Ähnlichkeiten werden zwischen Schwellwert (locker) und 100 % (fest) gespreizt.</summary>
        private double GetLinkStrength(AnimeNetworkEdge edge) {
            if (edge.Kind != AnimeEdgeKind.Similarity) {
                return edge.Weight;
            }
            double minimum = this.SimilarityPercent / 100.0;
            double scaled = minimum >= 1.0 ? 1.0 : (edge.Weight - minimum) / (1.0 - minimum);
            return 0.2 + 0.8 * Math.Clamp(scaled, 0.0, 1.0);
        }

        private void AddEdgeToNode(int nodeId, AnimeEdgeViewModel edge) {
            if (!this._edgesByNode.TryGetValue(nodeId, out List<AnimeEdgeViewModel>? list)) {
                list = [];
                this._edgesByNode[nodeId] = list;
            }
            list.Add(edge);
        }

        private IReadOnlyList<AnimeEdgeViewModel> GetEdges(int nodeId) {
            if (this._edgesByNode.TryGetValue(nodeId, out List<AnimeEdgeViewModel>? list)) {
                return list;
            }
            return [];
        }

        private bool MatchesStatus(State state) {
            return this.StatusFilter switch {
                LibraryStatusFilter.Wishlist => state == State.Wishlist,
                LibraryStatusFilter.Started => state == State.Started,
                LibraryStatusFilter.Finished => state == State.Finished,
                LibraryStatusFilter.Interrupted => state == State.Interrupted,
                LibraryStatusFilter.Canceled => state == State.Canceled,
                _ => true
            };
        }

        private List<AnimeClusterViewModel> CreateClusters() {
            List<AnimeClusterViewModel> clusters = [];
            IEnumerable<IGrouping<int, AnimeNodeViewModel>> groups = this.Nodes
                .Where(x => x.IsVisibleInGraph && x.IsLinked)
                .GroupBy(x => x.ClusterId)
                .OrderBy(group => group.Key);
            foreach (IGrouping<int, AnimeNodeViewModel> group in groups) {
                List<AnimeNodeViewModel> members = group.ToList();
                clusters.Add(new AnimeClusterViewModel(group.Key, members, this.CreateCaption(members)));
            }
            return clusters;
        }

        /// <summary>Beschriftung eines Clusters: häufigste Tags, Durchschnittsnote oder die Anzahl der Animes.</summary>
        private string CreateCaption(IReadOnlyList<AnimeNodeViewModel> members) {
            if (members.Count < MinCaptionMembers) {
                return string.Empty;
            }
            string caption = string.Empty;
            switch (this._mode) {
                case AnimeNetworkMode.Tags:
                    List<IReadOnlyCollection<int>> tagIds = members
                        .Select(x => (IReadOnlyCollection<int>)this._entriesById[x.Id].TagIds)
                        .ToList();
                    caption = ClusterLabeler.TopTags(tagIds, this._tagNames, 2);
                    break;
                case AnimeNetworkMode.Ratings:
                    List<int> generals = members
                        .Where(x => this._ratingsById.ContainsKey(x.Id))
                        .Select(x => this._ratingsById[x.Id].General)
                        .ToList();
                    caption = ClusterLabeler.AverageRating(generals);
                    break;
            }
            return caption.Length > 0 ? caption : $"{members.Count} Animes";
        }

        private void UpdateTexts(int filteredCount, int eligibleCount, AnimeNetworkResult result) {
            int clusterCount = result.Clusters.Values.Distinct().Count();
            int unlinkedCount = eligibleCount - result.Clusters.Count;
            this.InfoText = this.VisibleCount == 0
                ? "Keine Animes im Netzwerk"
                : $"{this.VisibleCount} Animes · {result.Edges.Count} Verbindungen · {clusterCount} Cluster";

            List<string> hints = [];
            int withoutData = filteredCount - eligibleCount;
            if (this._mode == AnimeNetworkMode.Tags && withoutData > 0) {
                hints.Add($"{withoutData} Animes ohne Tags werden nicht dargestellt.");
            }
            if (this._mode == AnimeNetworkMode.Ratings && withoutData > 0) {
                hints.Add($"{withoutData} unbewertete Animes werden nicht dargestellt.");
            }
            if (!this.ShowUnlinked && unlinkedCount > 0) {
                hints.Add($"{unlinkedCount} Animes ohne Verbindung sind ausgeblendet.");
            }
            this.HintText = string.Join(Environment.NewLine, hints);
        }
        #endregion

        #region Auswahl, Hervorhebung und Aktionen
        /// <summary>Klick auf einen Knoten: wählt ihn aus, ein zweiter Klick hebt die Auswahl auf.</summary>
        public void ClickNode(AnimeNodeViewModel node) {
            this.SelectNode(this._selected == node ? null : node);
        }

        public void ClearSelection() {
            this.SelectNode(null);
        }

        /// <summary>Wählt einen Anime aus (<c>null</c> hebt die Auswahl auf).</summary>
        public void SelectNode(AnimeNodeViewModel? node) {
            if (this._selected is not null) {
                this._selected.IsSelected = false;
            }
            this._selected = node;
            if (node is not null) {
                node.IsSelected = true;
            }
            this.UpdateSelectedDetails();
            this.UpdateNeighbors();
            this.RefreshHighlighting();
        }

        /// <summary>Hebt den Knoten unter der Maus und seine Nachbarn hervor, <c>null</c> beendet das.</summary>
        public void SetHover(AnimeNodeViewModel? node) {
            if (this._hovered == node) {
                return;
            }
            this._hovered = node;
            this.RefreshHighlighting();
        }

        /// <summary>Öffnet den Anime im Editor.</summary>
        public void RequestEdit(AnimeNodeViewModel node) {
            this.EditRequested?.Invoke(this, node.Id);
        }

        public void OpenSelected() {
            if (this._selected is not null) {
                this.RequestEdit(this._selected);
            }
        }

        /// <summary>Zeigt den gewählten Anime in der Bibliothek (sucht nach seinem Namen).</summary>
        public void SearchSelectedInLibrary() {
            if (this._selected is not null) {
                this.SearchInLibraryRequested?.Invoke(this, this._selected.Name);
            }
        }

        private void UpdateSelectedDetails() {
            AnimeNodeViewModel? node = this._selected;
            if (node is null || !this._entriesById.TryGetValue(node.Id, out AnimeListEntry? entry)) {
                this.SelectedItem = null;
                this.SelectedTagsText = string.Empty;
                this.SelectedRatingText = string.Empty;
                this.SelectedClusterText = string.Empty;
            }
            else {
                this.SelectedItem = new AnimeListItemViewModel(entry, this._animeService);
                List<string> tagNames = entry.TagIds
                    .Where(id => this._tagNames.ContainsKey(id))
                    .Select(id => this._tagNames[id])
                    .OrderBy(name => name, StringComparer.CurrentCultureIgnoreCase)
                    .ToList();
                this.SelectedTagsText = tagNames.Count == 0 ? "Keine Tags" : string.Join(", ", tagNames);
                this.SelectedRatingText = this._ratingsById.TryGetValue(node.Id, out AnimeRatingEntry? rating)
                    ? $"Gesamtnote {rating.General} (Story {rating.Story}, Animation {rating.Animation}, "
                        + $"Effekte {rating.SpecialEffects}, Akustik {rating.Acoustic})"
                    : "Nicht bewertet";
                this.SelectedClusterText = this.DescribeCluster(node);
            }
            this.OnPropertyChanged(nameof(this.HasSelection));
            this.OnPropertyChanged(nameof(this.SelectedItem));
            this.OnPropertyChanged(nameof(this.SelectedTagsText));
            this.OnPropertyChanged(nameof(this.SelectedRatingText));
            this.OnPropertyChanged(nameof(this.SelectedClusterText));
        }

        private string DescribeCluster(AnimeNodeViewModel node) {
            if (!node.IsLinked) {
                return "Ohne Verbindung";
            }
            AnimeClusterViewModel? cluster = this.Clusters.FirstOrDefault(x => x.Id == node.ClusterId);
            if (cluster is null) {
                return string.Empty;
            }
            string text = $"Cluster {cluster.Id + 1} mit {cluster.Members.Count} Animes";
            return cluster.Caption.Length > 0 && !cluster.Caption.EndsWith(" Animes") ? $"{text} ({cluster.Caption})" : text;
        }

        private void UpdateNeighbors() {
            this.Neighbors.Clear();
            if (this._selected is not null) {
                List<AnimeNeighborViewModel> items = [];
                foreach (AnimeEdgeViewModel edge in this.GetEdges(this._selected.Id)) {
                    AnimeNodeViewModel other = edge.A == this._selected ? edge.B : edge.A;
                    items.Add(new AnimeNeighborViewModel(other, DescribeEdge(edge, this._selected), edge.Weight));
                }
                IEnumerable<AnimeNeighborViewModel> ordered = items
                    .OrderByDescending(x => x.Weight)
                    .ThenBy(x => x.Name, StringComparer.CurrentCultureIgnoreCase);
                foreach (AnimeNeighborViewModel item in ordered) {
                    this.Neighbors.Add(item);
                }
            }
            this.OnPropertyChanged(nameof(this.NeighborsText));
        }

        private static string DescribeEdge(AnimeEdgeViewModel edge, AnimeNodeViewModel from) {
            return edge.Kind switch {
                AnimeEdgeKind.Successor => edge.A == from ? "Nachfolger" : "Vorgänger",
                AnimeEdgeKind.Related => "Verwandt",
                _ => $"{Math.Round(edge.Weight * 100)} %"
            };
        }

        /// <summary>
        /// Blendet alles aus, was nicht zum Fokus gehört: beim Überfahren alle außer dem Knoten und seinen Nachbarn,
        /// sonst bei einer Auswahl alle außer dem gewählten Anime und seinen Nachbarn, sonst bei einer Suche alle ohne Treffer.
        /// </summary>
        private void RefreshHighlighting() {
            AnimeNodeViewModel? focus = this._hovered ?? this._selected;
            HashSet<int> focusIds = [];
            if (focus is not null) {
                focusIds.Add(focus.Id);
                foreach (AnimeEdgeViewModel edge in this.GetEdges(focus.Id)) {
                    focusIds.Add(edge.A.Id);
                    focusIds.Add(edge.B.Id);
                }
            }
            string search = this.SearchText.Trim();
            bool hasSearch = search.Length > 0;
            int matchCount = 0;

            foreach (AnimeNodeViewModel node in this.Nodes) {
                if (!node.IsVisibleInGraph) {
                    node.IsHighlighted = false;
                    node.IsDimmed = false;
                    continue;
                }
                bool isMatch = hasSearch && node.Name.Contains(search, StringComparison.CurrentCultureIgnoreCase);
                if (isMatch) {
                    matchCount += 1;
                }
                if (focus is not null) {
                    node.IsHighlighted = focusIds.Contains(node.Id);
                    node.IsDimmed = !node.IsHighlighted;
                }
                else {
                    node.IsHighlighted = isMatch;
                    node.IsDimmed = hasSearch && !isMatch;
                }
            }
            foreach (AnimeEdgeViewModel edge in this.Edges) {
                edge.IsHighlighted = focus is not null && (edge.A == focus || edge.B == focus);
            }
            this.SearchResultText = !hasSearch ? string.Empty : (matchCount == 1 ? "1 Treffer" : $"{matchCount} Treffer");
            this.Redraw?.Invoke(this, EventArgs.Empty);
        }
        #endregion
    }
}
