using Anicluster.AnimeNetwork;
using Anicluster.Database.Services;
using Anicluster.TagNetwork;
using Modells.Anime;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Threading;

namespace Anicluster.ViewModels {

    /// <summary>
    /// Das Tag-Netzwerk: Tags als Knoten (Schriftgröße = Häufigkeit), Tags, die oft gemeinsam vorkommen, rücken zusammen.
    /// Knoten lassen sich ein- und ausschalten und auswählen, die Auswahl wirkt als Filter auf die Animes.
    /// </summary>
    public class TagNetworkViewModel : PropertyBaseChange, IDisposable {

        private const double MinFontSize = 12;
        private const double MaxFontSize = 34;
        private const double TargetCoverage = 0.20;
        private const double MinDisplayScale = 0.55;
        private const double MinZoom = 1.0;
        private const double MaxZoom = 8.0;

        /// <summary>Faktor pro Mausrad-Stufe (multiplikativ, damit sich jede Stufe gleich anfühlt)</summary>
        private const double ZoomFactor = 1.25;
        private const double ViewportInset = 2.0;

        /// <summary>Für die Gruppen zählen je Tag nur seine stärksten Verbindungen, sonst verschmilzt bei vielen Tags alles zu einer Gruppe.</summary>
        private const int ClusterNeighborCount = 3;

        private readonly ForceSimulation _simulation = new ForceSimulation();
        private readonly DispatcherTimer _timer;
        private readonly IAnimeService _animeService;
        private readonly List<AnimeListEntry> _animes;
        private readonly Dictionary<int, TagNodeViewModel> _nodesById = [];
        private readonly Dictionary<int, List<TagNodeViewModel>> _neighbors = [];
        private readonly Random _random = new Random(7);
        private TagNodeViewModel? _hovered;
        private bool _suppressApply;
        private bool _hasLayout;
        private double _viewportWidth;
        private double _viewportHeight;
        private double _worldWidth = 900;
        private double _worldHeight = 600;
        private string _searchText = string.Empty;
        private int _minCount = 1;
        private TagMatchMode _matchMode = TagMatchMode.All;
        private double _zoom = MinZoom;
        private IReadOnlyList<TagClusterViewModel> _clusters = [];

        public ObservableCollection<TagNodeViewModel> Nodes { get; } = [];

        public IReadOnlyList<TagEdgeViewModel> Edges { get; }

        /// <summary>Alle Tags für die Liste neben dem Netzwerk (durchsuchbar, alphabetisch)</summary>
        public ICollectionView ListNodes { get; }

        public ObservableCollection<TagNodeViewModel> SelectedNodes { get; } = [];

        /// <summary>Die Animes, auf die der Filter der ausgewählten Tags zutrifft</summary>
        public ObservableCollection<AnimeListItemViewModel> Results { get; } = [];

        /// <summary>Wird bei jedem Schritt der Simulation und bei Änderungen der Hervorhebung ausgelöst, damit die Linien neu gezeichnet werden.</summary>
        public event EventHandler? Redraw;

        /// <summary>Ein Anime soll zum Bearbeiten geöffnet werden.</summary>
        public event EventHandler<AnimeListItemViewModel>? EditRequested;

        /// <summary>Die Auswahl soll als Tag-Filter in der Bibliothek angezeigt werden.</summary>
        public event EventHandler<ShowInLibraryEventArgs>? ShowInLibraryRequested;

        #region Fläche
        /// <summary>Größe der Fläche, auf der simuliert wird. Bei vielen Knoten ist sie größer als der sichtbare Bereich und wird verkleinert dargestellt.</summary>
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

        /// <summary>Gruppen eng zusammenhängender Tags, jede mit eigener Fläche und Farbe</summary>
        public IReadOnlyList<TagClusterViewModel> Clusters => this._clusters;
        #endregion

        #region Liste, Suche und Mindesthäufigkeit
        public string SearchText {
            get => this._searchText;
            set {
                if (this.SetProperty(ref this._searchText, value)) {
                    this.ListNodes.Refresh();
                }
            }
        }

        /// <summary>Größte Häufigkeit eines Tags (Obergrenze des Reglers)</summary>
        public int MaxCount { get; }

        /// <summary>Tags, die seltener vorkommen, werden im Netzwerk ausgeblendet.</summary>
        public int MinCount {
            get => this._minCount;
            set {
                if (this.SetProperty(ref this._minCount, Math.Clamp(value, 0, this.MaxCount))) {
                    this.ApplyVisibility();
                }
            }
        }

        public int VisibleCount => this.Nodes.Count(x => x.IsVisibleInGraph);

        public string VisibleCountText => $"{this.VisibleCount} von {this.Nodes.Count} Tags sichtbar";

        public bool IsEmpty => this.Nodes.Count == 0;
        #endregion

        #region Auswahl und Treffer
        public TagMatchMode MatchMode {
            get => this._matchMode;
            set {
                if (this.SetProperty(ref this._matchMode, value)) {
                    this.OnSelectionChanged();
                }
            }
        }

        public bool HasSelection => this.SelectedNodes.Count > 0;

        public bool HasMultipleSelected => this.SelectedNodes.Count > 1;

        public string ResultsText => this.HasSelection
            ? (this.Results.Count == 1 ? "1 Treffer" : $"{this.Results.Count} Treffer")
            : "Wähle im Netzwerk einen oder mehrere Tags aus (Strg + Klick für mehrere).";
        #endregion

        public TagNetworkViewModel(IAnimeService animeService, ITagService tagService) {
            this._animeService = animeService;
            this._animes = animeService.SelectAnimeListEntries();
            List<TagUsage> usages = tagService.SelectTagUsage();
            List<TagLink> links = tagService.SelectTagLinks();

            this.MaxCount = Math.Max(1, usages.Count == 0 ? 1 : usages.Max(x => x.AnimeCount));
            int usedMin = usages.Where(x => x.AnimeCount > 0).Select(x => x.AnimeCount).DefaultIfEmpty(1).Min();
            int usedMax = usages.Where(x => x.AnimeCount > 0).Select(x => x.AnimeCount).DefaultIfEmpty(1).Max();

            foreach (TagUsage usage in usages) {
                // Schriftgröße nach Häufigkeit, die Wurzel verhindert, dass wenige sehr häufige Tags alles überdecken
                double share = usedMax == usedMin ? 0.4 : Math.Sqrt((double)(Math.Max(usage.AnimeCount, usedMin) - usedMin) / (usedMax - usedMin));
                TagNodeViewModel node = new TagNodeViewModel(usage.Id, usage.Designation, usage.AnimeCount, MinFontSize + (MaxFontSize - MinFontSize) * share);
                node.EnabledChanged += this.OnNodeEnabledChanged;
                this.Nodes.Add(node);
                this._nodesById[node.Id] = node;
                this._neighbors[node.Id] = [];
                this._simulation.Nodes.Add(node.Node);
            }

            int maxLinkCount = Math.Max(1, links.Count == 0 ? 1 : links.Max(x => x.AnimeCount));
            List<TagEdgeViewModel> edges = [];
            foreach (TagLink link in links) {
                if (!this._nodesById.TryGetValue(link.TagIdA, out TagNodeViewModel? a) || !this._nodesById.TryGetValue(link.TagIdB, out TagNodeViewModel? b)) {
                    continue;
                }
                double strength = Math.Min(1.0, (double)link.AnimeCount / Math.Max(1, Math.Min(a.Count, b.Count)));
                this._simulation.Links.Add(new ForceLink { A = a.Node, B = b.Node, Strength = strength });
                edges.Add(new TagEdgeViewModel { A = a, B = b, Count = link.AnimeCount, Weight = (double)link.AnimeCount / maxLinkCount });
                this._neighbors[a.Id].Add(b);
                this._neighbors[b.Id].Add(a);
            }
            this.Edges = edges;

            // Eigene Sicht für die Liste, damit die Suche das Netzwerk nicht beeinflusst
            this.ListNodes = new CollectionViewSource { Source = this.Nodes }.View;
            this.ListNodes.Filter = this.MatchesSearch;
            this.ListNodes.SortDescriptions.Add(new SortDescription(nameof(TagNodeViewModel.Designation), ListSortDirection.Ascending));

            this._timer = new DispatcherTimer(DispatcherPriority.Render) { Interval = TimeSpan.FromMilliseconds(16) };
            this._timer.Tick += this.OnTimerTick;

            // Anfangszustand: Tags ohne Anime (und alles unter der Mindesthäufigkeit) sind ausgeblendet
            this.UpdateVisibilityFlags();
            foreach (TagNodeViewModel node in this.Nodes) {
                node.IsVisibleInGraph = this.ShouldBeVisible(node);
                node.Node.IsActive = node.IsVisibleInGraph;
            }
            this.UpdateClusters();
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
                this.PlaceAllNodes();
                this._simulation.Restart();
                this.StartTimer();
            }
        }

        /// <summary>Verteilt alle Knoten neu und startet die Simulation von vorn.</summary>
        public void Relayout() {
            if (!this._hasLayout) {
                return;
            }
            this.PlaceAllNodes();
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
        public void DragNode(TagNodeViewModel node, double x, double y) {
            node.Node.IsPinned = true;
            node.Node.X = Math.Min(Math.Max(x, node.Width / 2.0), this.WorldWidth - node.Width / 2.0);
            node.Node.Y = Math.Min(Math.Max(y, node.Height / 2.0), this.WorldHeight - node.Height / 2.0);
            node.NotifyPositionChanged();
            this._simulation.Reheat(0.3);
            this.StartTimer();
            this.Redraw?.Invoke(this, EventArgs.Empty);
        }

        public void EndDrag(TagNodeViewModel node) {
            node.Node.IsPinned = false;
        }

        public void Dispose() {
            this._timer.Stop();
            this._timer.Tick -= this.OnTimerTick;
        }

        private void OnTimerTick(object? sender, EventArgs e) {
            this._simulation.Step();
            foreach (TagNodeViewModel node in this.Nodes) {
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

        /// <summary>Anfangsverteilung auf einer Spirale um die Mitte.</summary>
        private void PlaceAllNodes() {
            int index = 0;
            foreach (TagNodeViewModel node in this.Nodes) {
                double angle = index * 2.399963;
                double radius = 30 + 36 * Math.Sqrt(index);
                node.Node.X = this.WorldWidth / 2.0 + Math.Cos(angle) * radius;
                node.Node.Y = this.WorldHeight / 2.0 + Math.Sin(angle) * radius;
                node.Node.Vx = 0;
                node.Node.Vy = 0;
                node.NotifyPositionChanged();
                index += 1;
            }
        }

        /// <summary>
        /// Bei vielen sichtbaren Knoten wird die Fläche größer gewählt (und verkleinert dargestellt),
        /// damit die Textfelder Platz haben, ohne sich zu überlappen.
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
            if (this._hasLayout && this.WorldWidth > 0 && this.WorldHeight > 0 && (Math.Abs(newWidth - this.WorldWidth) > 1 || Math.Abs(newHeight - this.WorldHeight) > 1)) {
                // Vorhandene Positionen mit der Fläche mitnehmen, damit das Layout erhalten bleibt
                double scaleX = newWidth / this.WorldWidth;
                double scaleY = newHeight / this.WorldHeight;
                foreach (TagNodeViewModel node in this.Nodes) {
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

        #region Tags ein- und ausschalten
        public void DisableNode(TagNodeViewModel node) {
            node.IsEnabled = false;
        }

        public void EnableAll() {
            this.SetAllEnabled(true);
        }

        public void DisableAll() {
            this.SetAllEnabled(false);
        }

        private void SetAllEnabled(bool isEnabled) {
            this._suppressApply = true;
            foreach (TagNodeViewModel node in this.Nodes) {
                node.IsEnabled = isEnabled;
            }
            this._suppressApply = false;
            this.ApplyVisibility();
        }

        private void OnNodeEnabledChanged(object? sender, EventArgs e) {
            if (!this._suppressApply) {
                this.ApplyVisibility();
            }
        }

        private bool ShouldBeVisible(TagNodeViewModel node) {
            return node.IsEnabled && node.Count >= this.MinCount;
        }

        private void UpdateVisibilityFlags() {
            foreach (TagNodeViewModel node in this.Nodes) {
                node.IsBelowMinimum = node.Count < this.MinCount;
            }
        }

        /// <summary>Übernimmt Ein/Aus und Mindesthäufigkeit ins Netzwerk, ausgeblendete Knoten fallen aus der Simulation.</summary>
        private void ApplyVisibility() {
            this.UpdateVisibilityFlags();
            List<TagNodeViewModel> shown = [];
            foreach (TagNodeViewModel node in this.Nodes) {
                bool visible = this.ShouldBeVisible(node);
                if (visible && !node.IsVisibleInGraph) {
                    shown.Add(node);
                }
                node.IsVisibleInGraph = visible;
                node.Node.IsActive = visible;
                if (!visible && node.IsSelected) {
                    this.SetSelected(node, false);
                }
            }
            this.UpdateWorldSize();
            this.UpdateClusters();
            foreach (TagNodeViewModel node in shown) {
                this.PlaceNearNeighbors(node);
            }
            this.OnPropertyChanged(nameof(this.VisibleCount));
            this.OnPropertyChanged(nameof(this.VisibleCountText));
            this.OnSelectionChanged();
            this._simulation.Reheat(0.6);
            this.StartTimer();
        }

        /// <summary>Ein wieder eingeschalteter Knoten erscheint in der Nähe seiner sichtbaren Nachbarn (sonst in der Mitte).</summary>
        private void PlaceNearNeighbors(TagNodeViewModel node) {
            List<TagNodeViewModel> visibleNeighbors = this._neighbors[node.Id].Where(x => x.IsVisibleInGraph).ToList();
            double x = this.WorldWidth / 2.0;
            double y = this.WorldHeight / 2.0;
            if (visibleNeighbors.Count > 0) {
                x = visibleNeighbors.Average(n => n.CenterX);
                y = visibleNeighbors.Average(n => n.CenterY);
            }
            node.Node.X = x + (this._random.NextDouble() - 0.5) * 80;
            node.Node.Y = y + (this._random.NextDouble() - 0.5) * 80;
            node.Node.Vx = 0;
            node.Node.Vy = 0;
            node.NotifyPositionChanged();
        }

        /// <summary>
        /// Gruppiert die sichtbaren Tags nach ihren Verbindungen (Label Propagation, wie im Anime-Netzwerk). Je Tag zählen nur seine
        /// <see cref="ClusterNeighborCount"/> stärksten Verbindungen. Die Stärke ist der Anteil gemeinsamer Animes am selteneren Tag.
        /// </summary>
        private void UpdateClusters() {
            Dictionary<TagEdgeViewModel, double> strengths = [];
            Dictionary<int, List<TagEdgeViewModel>> byNode = [];
            foreach (TagEdgeViewModel edge in this.Edges) {
                if (!edge.IsVisible) {
                    continue;
                }
                strengths[edge] = Math.Min(1.0, (double)edge.Count / Math.Max(1, Math.Min(edge.A.Count, edge.B.Count)));
                foreach (int id in new[] { edge.A.Id, edge.B.Id }) {
                    if (!byNode.TryGetValue(id, out List<TagEdgeViewModel>? list)) {
                        list = [];
                        byNode[id] = list;
                    }
                    list.Add(edge);
                }
            }
            HashSet<TagEdgeViewModel> kept = [];
            foreach (List<TagEdgeViewModel> list in byNode.Values) {
                foreach (TagEdgeViewModel edge in list.OrderByDescending(x => strengths[x]).Take(ClusterNeighborCount)) {
                    kept.Add(edge);
                }
            }
            List<AnimeNetworkEdge> clusterEdges = kept
                .OrderBy(x => x.A.Id)
                .ThenBy(x => x.B.Id)
                .Select(x => new AnimeNetworkEdge(x.A.Id, x.B.Id, strengths[x], AnimeEdgeKind.Similarity))
                .ToList();

            Dictionary<int, int> labels = ClusterDetector.LabelPropagation(clusterEdges);
            IReadOnlyList<TagClusterViewModel> clusters = labels
                .GroupBy(x => x.Value)
                .OrderBy(x => x.Key)
                .Select(group => new TagClusterViewModel(group.Key, group.Select(x => this._nodesById[x.Key]).ToList()))
                .ToList();
            this.SetProperty(ref this._clusters, clusters, nameof(this.Clusters));
        }

        private bool MatchesSearch(object item) {
            return item is TagNodeViewModel node
                && (string.IsNullOrWhiteSpace(this.SearchText)
                    || node.Designation.Contains(this.SearchText.Trim(), StringComparison.CurrentCultureIgnoreCase));
        }
        #endregion

        #region Auswahl, Hervorhebung und Treffer
        /// <summary>Klick auf einen Knoten: wählt ihn allein aus (bzw. hebt die Auswahl auf), mit Strg wird er der Auswahl hinzugefügt oder daraus entfernt.</summary>
        public void ClickNode(TagNodeViewModel node, bool additive) {
            if (additive) {
                this.SetSelected(node, !node.IsSelected);
            }
            else if (node.IsSelected && this.SelectedNodes.Count == 1) {
                this.SetSelected(node, false);
            }
            else {
                foreach (TagNodeViewModel selected in this.SelectedNodes.ToList()) {
                    this.SetSelected(selected, false);
                }
                this.SetSelected(node, true);
            }
            this.OnSelectionChanged();
        }

        public void RemoveFromSelection(TagNodeViewModel node) {
            this.SetSelected(node, false);
            this.OnSelectionChanged();
        }

        public void ClearSelection() {
            foreach (TagNodeViewModel selected in this.SelectedNodes.ToList()) {
                this.SetSelected(selected, false);
            }
            this.OnSelectionChanged();
        }

        /// <summary>Hebt den Knoten unter der Maus und seine Nachbarn hervor, <c>null</c> beendet das.</summary>
        public void SetHover(TagNodeViewModel? node) {
            if (this._hovered == node) {
                return;
            }
            this._hovered = node;
            this.RefreshHighlighting();
        }

        /// <summary>Zeigt die ausgewählten Tags als Filter in der Bibliothek an.</summary>
        public void ShowInLibrary() {
            if (!this.HasSelection) {
                return;
            }
            this.ShowInLibraryRequested?.Invoke(this, new ShowInLibraryEventArgs {
                Tags = this.SelectedNodes.Select(x => new TagFilterItem(x.Id, x.Designation)).ToList(),
                Mode = this.MatchMode
            });
        }

        private void SetSelected(TagNodeViewModel node, bool isSelected) {
            node.IsSelected = isSelected;
            if (isSelected && !this.SelectedNodes.Contains(node)) {
                this.SelectedNodes.Add(node);
            }
            else if (!isSelected) {
                this.SelectedNodes.Remove(node);
            }
        }

        private void OnSelectionChanged() {
            this.UpdateResults();
            this.OnPropertyChanged(nameof(this.HasSelection));
            this.OnPropertyChanged(nameof(this.HasMultipleSelected));
            this.OnPropertyChanged(nameof(this.ResultsText));
            this.RefreshHighlighting();
        }

        /// <summary>Ermittelt die Animes, auf die der Filter der ausgewählten Tags zutrifft.</summary>
        private void UpdateResults() {
            this.Results.Clear();
            if (!this.HasSelection) {
                return;
            }
            HashSet<int> selectedIds = this.SelectedNodes.Select(x => x.Id).ToHashSet();
            IEnumerable<AnimeListEntry> matches = this._animes.Where(anime => this.MatchMode == TagMatchMode.All
                ? selectedIds.All(id => anime.TagIds.Contains(id))
                : selectedIds.Any(id => anime.TagIds.Contains(id)));
            foreach (AnimeListEntry entry in matches.OrderBy(x => x.Name, StringComparer.CurrentCultureIgnoreCase)) {
                AnimeListItemViewModel item = new AnimeListItemViewModel(entry, this._animeService);
                item.EditRequested += (sender, e) => this.EditRequested?.Invoke(this, item);
                this.Results.Add(item);
            }
        }

        /// <summary>
        /// Blendet alles aus, was nicht zum Fokus gehört: beim Überfahren alle außer dem Knoten und seinen Nachbarn,
        /// sonst bei einer Auswahl alle Tags, die bei keinem Treffer vorkommen.
        /// </summary>
        private void RefreshHighlighting() {
            HashSet<int>? relatedTagIds = null;
            if (this.HasSelection) {
                HashSet<int> resultIds = this.Results.Select(x => x.Id).ToHashSet();
                relatedTagIds = this._animes
                    .Where(anime => resultIds.Contains(anime.Id))
                    .SelectMany(anime => anime.TagIds)
                    .ToHashSet();
            }
            HashSet<TagNodeViewModel> hoveredNeighbors = new HashSet<TagNodeViewModel>();
            if (this._hovered is not null) {
                hoveredNeighbors.UnionWith(this._neighbors[this._hovered.Id]);
            }

            foreach (TagNodeViewModel node in this.Nodes) {
                bool isHovered = node == this._hovered;
                bool isNeighbor = hoveredNeighbors.Contains(node);
                node.IsHighlighted = isHovered || isNeighbor;
                if (this._hovered is not null) {
                    node.IsDimmed = !(isHovered || isNeighbor);
                }
                else if (relatedTagIds is not null) {
                    node.IsDimmed = !node.IsSelected && !relatedTagIds.Contains(node.Id);
                }
                else {
                    node.IsDimmed = false;
                }
            }
            foreach (TagEdgeViewModel edge in this.Edges) {
                edge.IsHighlighted = this._hovered is not null
                    ? edge.A == this._hovered || edge.B == this._hovered
                    : edge.A.IsSelected || edge.B.IsSelected;
            }
            this.Redraw?.Invoke(this, EventArgs.Empty);
        }
        #endregion
    }
}
