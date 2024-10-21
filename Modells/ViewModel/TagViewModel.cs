using Modells.Anime;

namespace Modells.ViewModel {

    public class TagViewModel {

        public Tag Tag { get; set; }
        public bool IsSelected { get; set; } = false;

        public TagViewModel(Tag tag) {
            Tag = tag;
            IsSelected = false;
        }
    }
}
