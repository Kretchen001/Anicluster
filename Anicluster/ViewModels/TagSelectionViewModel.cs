using Anicluster.Database.Services;
using Modells.Anime;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;

namespace Anicluster.ViewModels {

    /// <summary>
    /// Dialog zur Auswahl der Tags eines Animes (mit Filter).
    /// Das Ergebnis steht nach dem Übernehmen in <see cref="Result"/>.
    /// </summary>
    public class TagSelectionViewModel : DialogViewModelBase {

        private string _filterText = string.Empty;

        public override string Title => "Tag-Selektor";

        public override bool FillsOwnerWindow => true;

        public ObservableCollection<TagItemViewModel> AllTags { get; } = [];

        /// <summary>Gefilterte Sicht auf <see cref="AllTags"/></summary>
        public ICollectionView FilteredTags { get; }

        public string FilterText {
            get => this._filterText;
            set {
                if (this.SetProperty(ref this._filterText, value)) {
                    this.FilteredTags.Refresh();
                }
            }
        }

        /// <summary>Die ausgewählten Tags, leer solange nicht übernommen wurde.</summary>
        public List<Tag> Result { get; private set; } = [];

        public TagSelectionViewModel(ITagService tagService, IEnumerable<Tag> selectedTags) {
            HashSet<int> selectedIds = selectedTags.Select(x => x.Id).ToHashSet();
            foreach (Tag tag in tagService.SelectAllTags()) {
                this.AllTags.Add(new TagItemViewModel(tag, selectedIds.Contains(tag.Id)));
            }
            this.FilteredTags = CollectionViewSource.GetDefaultView(this.AllTags);
            this.FilteredTags.Filter = this.MatchesFilter;
        }

        public void ClearFilter() {
            this.FilterText = string.Empty;
        }

        public void Accept() {
            this.Result = this.AllTags.Where(x => x.IsSelected).Select(x => x.Tag).ToList();
            this.RequestClose(true);
        }

        public void Cancel() {
            this.RequestClose(false);
        }

        private bool MatchesFilter(object item) {
            return item is TagItemViewModel tagItem
                && (string.IsNullOrEmpty(this.FilterText)
                    || tagItem.Tag.Designation.Contains(this.FilterText, StringComparison.OrdinalIgnoreCase));
        }
    }
}
