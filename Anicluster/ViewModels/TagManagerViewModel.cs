using Anicluster.Database.Services;
using Anicluster.Dialogs;
using Modells.Anime;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;

namespace Anicluster.ViewModels {

    /// <summary>
    /// Seite zum Verwalten aller Tags: suchen, neue anlegen und löschen.
    /// </summary>
    public class TagManagerViewModel : PropertyBaseChange {

        private readonly ITagService _tagService;
        private readonly IDialogService _dialogService;
        private string _searchText = string.Empty;
        private string _newTagName = string.Empty;

        public ObservableCollection<Tag> AllTags { get; } = [];

        /// <summary>Gefilterte und alphabetisch sortierte Sicht auf <see cref="AllTags"/></summary>
        public ICollectionView FilteredTags { get; }

        public string SearchText {
            get => this._searchText;
            set {
                if (this.SetProperty(ref this._searchText, value)) {
                    this.FilteredTags.Refresh();
                }
            }
        }

        public string NewTagName {
            get => this._newTagName;
            set {
                if (this.SetProperty(ref this._newTagName, value)) {
                    this.OnPropertyChanged(nameof(this.IsNewTagNameDuplicate));
                    this.OnPropertyChanged(nameof(this.CanAddTag));
                }
            }
        }

        /// <summary>Gibt es den eingegebenen Namen bereits?</summary>
        public bool IsNewTagNameDuplicate => this.AllTags.Any(x => x.Designation.Equals(this.NewTagName.Trim(), StringComparison.CurrentCultureIgnoreCase));

        public bool CanAddTag => !string.IsNullOrWhiteSpace(this.NewTagName) && !this.IsNewTagNameDuplicate;

        public string CountText => this.AllTags.Count == 1 ? "1 Tag" : $"{this.AllTags.Count} Tags";

        public TagManagerViewModel(ITagService tagService, IDialogService dialogService) {
            this._tagService = tagService;
            this._dialogService = dialogService;
            foreach (Tag tag in this._tagService.SelectAllTags()) {
                this.AllTags.Add(tag);
            }
            this.FilteredTags = CollectionViewSource.GetDefaultView(this.AllTags);
            this.FilteredTags.Filter = this.MatchesSearch;
            this.FilteredTags.SortDescriptions.Add(new SortDescription(nameof(Tag.Designation), ListSortDirection.Ascending));
        }

        public void ClearSearch() {
            this.SearchText = string.Empty;
        }

        /// <summary>Legt den Tag mit dem eingegebenen Namen an.</summary>
        public void AddTag() {
            if (!this.CanAddTag) {
                return;
            }
            Tag tag = new Tag(this.NewTagName.Trim());
            if (this._tagService.Insert(tag) < 0) {
                this._dialogService.ShowMessage("Fehler beim Speichern!\nBitte beim Log aufbewahren und Programmierer informieren.", string.Empty, System.Windows.MessageBoxImage.Error);
                return;
            }
            this.AllTags.Add(tag);
            this.NewTagName = string.Empty;
            this.OnPropertyChanged(nameof(this.CountText));
        }

        /// <summary>Löscht den Tag nach Rückfrage, er verschwindet damit auch bei allen Animes.</summary>
        public void DeleteTag(Tag tag) {
            if (!this._dialogService.Confirm($"Soll der Tag '{tag.Designation}' wirklich gelöscht werden?\nEr wird auch bei allen Animes entfernt.", "Tag löschen")) {
                return;
            }
            if (this._tagService.DeleteTagById(tag)) {
                this.AllTags.Remove(tag);
                this.OnPropertyChanged(nameof(this.CountText));
            }
            else {
                this._dialogService.ShowMessage("Fehler beim Löschen!\nBitte beim Log aufbewahren und Programmierer informieren.", string.Empty, System.Windows.MessageBoxImage.Error);
            }
        }

        private bool MatchesSearch(object item) {
            return item is Tag tag
                && (string.IsNullOrEmpty(this.SearchText)
                    || tag.Designation.Contains(this.SearchText, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}
