using Modells.Anime;

namespace Anicluster.ViewModels {

    /// <summary>
    /// Ein Tag mit Auswahlzustand für die Tag-Auswahl.
    /// </summary>
    public class TagItemViewModel : PropertyBaseChange {

        private bool _isSelected;

        public Tag Tag { get; }

        public bool IsSelected {
            get => this._isSelected;
            set => this.SetProperty(ref this._isSelected, value);
        }

        public TagItemViewModel(Tag tag, bool isSelected = false) {
            this.Tag = tag;
            this._isSelected = isSelected;
        }
    }
}
