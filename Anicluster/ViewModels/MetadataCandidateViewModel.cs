using Anicluster.Metadata;

namespace Anicluster.ViewModels {

    /// <summary>Ein Treffer in der Liste des Dialogs "Metadaten laden": Titel und eine Zeile mit den wichtigsten Angaben.</summary>
    public class MetadataCandidateViewModel {

        public MetadataCandidate Candidate { get; }

        public string Title => this.Candidate.Title;

        /// <summary>Zweiter Titel, Art, Jahr und Episodenzahl, getrennt durch " · "</summary>
        public string Subtitle { get; }

        public bool HasSubtitle => this.Subtitle.Length > 0;

        public MetadataCandidateViewModel(MetadataCandidate candidate) {
            this.Candidate = candidate;
            List<string> parts = [];
            if (candidate.AlternativeTitle is not null) {
                parts.Add(candidate.AlternativeTitle);
            }
            if (candidate.Format is not null) {
                parts.Add(candidate.Format);
            }
            if (candidate.Year is int year) {
                parts.Add(year.ToString("D4"));
            }
            if (candidate.EpisodeCount is int episodes) {
                parts.Add(episodes == 1 ? "1 Episode" : $"{episodes} Episoden");
            }
            this.Subtitle = string.Join(" · ", parts);
        }
    }
}
