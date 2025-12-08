using Modells.Anime;

namespace Modells.ViewModel {

    public class TagViewModel : ValueComparableObject {

        public Tag Tag { get; set; }
        public bool IsSelected { get; set; } = false;

        public TagViewModel(Tag tag) {
            this.Tag = tag;
            this.IsSelected = false;
        }
    }
}
