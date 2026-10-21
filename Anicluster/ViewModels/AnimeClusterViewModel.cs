namespace Anicluster.ViewModels {

    /// <summary>
    /// Ein Cluster im Anime-Netzwerk: seine Mitglieder und die Beschriftung. Die Zeichenfläche umgibt die Mitglieder mit einem
    /// weichen Hintergrund und schreibt die Beschriftung darüber.
    /// </summary>
    public class AnimeClusterViewModel {

        public int Id { get; }

        public IReadOnlyList<AnimeNodeViewModel> Members { get; }

        /// <summary>Kurze Beschriftung (häufigste Tags, Durchschnittsnote oder Anzahl), leer bei sehr kleinen Clustern</summary>
        public string Caption { get; }

        public AnimeClusterViewModel(int id, IReadOnlyList<AnimeNodeViewModel> members, string caption) {
            this.Id = id;
            this.Members = members;
            this.Caption = caption;
        }
    }
}
