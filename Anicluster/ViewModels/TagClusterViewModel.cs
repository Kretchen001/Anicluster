namespace Anicluster.ViewModels {

    /// <summary>Eine Gruppe von Tags, die im Tag-Netzwerk eng zusammenhängen (eigene Fläche und Farbe). Nummer 0 ist die größte Gruppe.</summary>
    public class TagClusterViewModel {

        public int Id { get; }

        public IReadOnlyList<TagNodeViewModel> Members { get; }

        public TagClusterViewModel(int id, IReadOnlyList<TagNodeViewModel> members) {
            this.Id = id;
            this.Members = members;
        }
    }
}
