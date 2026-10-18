using Anicluster.Database.Services;
using Anicluster.Dialogs;
using Modells.Anime;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace Anicluster.ViewModels {

    /// <summary>
    /// Dialog zum Verwalten aller Tags (anlegen, löschen, suchen).
    /// </summary>
    public class TagManagerViewModel : DialogViewModelBase {

        private readonly ITagService _tagService;
        private readonly IDialogService _dialogService;
        private Tag? _selectedTag;
        private bool _isSearchVisible;
        private string _searchText = string.Empty;
        private bool _isAddTagPopupOpen;
        private string _newTagName = string.Empty;

        public override string Title => "TagOrganisator";

        public ObservableCollection<Tag> AllTags { get; } = [];

        /// <summary>Gefilterte Sicht auf <see cref="AllTags"/></summary>
        public ICollectionView FilteredTags { get; }

        public Tag? SelectedTag {
            get => this._selectedTag;
            set => this.SetProperty(ref this._selectedTag, value);
        }

        public bool IsSearchVisible {
            get => this._isSearchVisible;
            private set {
                if (this.SetProperty(ref this._isSearchVisible, value)) {
                    this.OnPropertyChanged(nameof(this.SearchToggleText));
                }
            }
        }

        public string SearchToggleText => this.IsSearchVisible ? "Suche ausblenden" : "Suche einblenden";

        public string SearchText {
            get => this._searchText;
            set {
                if (this.SetProperty(ref this._searchText, value)) {
                    this.FilteredTags.Refresh();
                }
            }
        }

        public bool IsAddTagPopupOpen {
            get => this._isAddTagPopupOpen;
            set => this.SetProperty(ref this._isAddTagPopupOpen, value);
        }

        public string NewTagName {
            get => this._newTagName;
            set {
                if (this.SetProperty(ref this._newTagName, value)) {
                    this.OnPropertyChanged(nameof(this.IsNewTagNameDuplicate));
                }
            }
        }

        /// <summary>Gibt es den eingegebenen Tag-Namen bereits? (dient zur Rotfärbung des Eingabefelds)</summary>
        public bool IsNewTagNameDuplicate => this.AllTags.Any(x => x.Designation.Equals(this.NewTagName, StringComparison.OrdinalIgnoreCase));

        public TagManagerViewModel(ITagService tagService, IDialogService dialogService) {
            this._tagService = tagService;
            this._dialogService = dialogService;
            foreach (Tag tag in this._tagService.SelectAllTags()) {
                this.AllTags.Add(tag);
            }
            this.FilteredTags = CollectionViewSource.GetDefaultView(this.AllTags);
            this.FilteredTags.Filter = this.MatchesSearch;
        }

        public void ToggleSearch() {
            this.IsSearchVisible = !this.IsSearchVisible;
        }

        public void ClearSearch() {
            this.SearchText = string.Empty;
        }

        public void OpenAddTagPopup() {
            this.IsAddTagPopupOpen = true;
        }

        /// <summary>Legt den Tag mit dem eingegebenen Namen an, sofern es keinen ähnlichen gibt.</summary>
        public void AddTag() {
            if (string.IsNullOrWhiteSpace(this.NewTagName)) {
                return;
            }
            if (this.AllTags.Any(x => x.Designation.Contains(this.NewTagName, StringComparison.OrdinalIgnoreCase))) {
                this._dialogService.ShowMessage($"Tag '{this.NewTagName}' bereits vorhanden!");
                return;
            }
            Tag tag = new Tag(this.NewTagName);
            tag.Id = this._tagService.Insert(tag);
            this.AllTags.Add(tag);
            this._dialogService.ShowMessage($"Tag '{tag.Designation}' erfolgreich gespeichert!");
            this.NewTagName = string.Empty;
        }

        public void DeleteSelectedTag() {
            if (this.SelectedTag is null) {
                this._dialogService.ShowMessage("Kein Tag ausgewählt zum Löschen.");
                return;
            }
            Tag tag = this.SelectedTag;
            if (this._tagService.DeleteTagById(tag)) {
                this._dialogService.ShowMessage($"Tag '{tag.Designation}' erfolgreich gelöscht.");
                this.AllTags.Remove(tag);
            }
            else {
                this._dialogService.ShowMessage("Fehler beim Löschen!\nBitte beim Log aufbewahren und Programmierer informieren.", string.Empty, MessageBoxImage.Error);
            }
        }


        private bool MatchesSearch(object item) {
            return item is Tag tag
                && (string.IsNullOrEmpty(this.SearchText)
                    || tag.Designation.Contains(this.SearchText, StringComparison.OrdinalIgnoreCase));
        }
    }
}
