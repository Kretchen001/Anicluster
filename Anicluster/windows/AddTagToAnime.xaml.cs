using Anicluster.Database;
using Anicluster.Database.Services;
using Modells.Anime;
using Modells.ViewModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Anicluster.windows {

    /// <summary>
    /// Interaktionslogik für AddTagToAnime.xaml
    /// </summary>
    public partial class AddTagToAnime : Window {

        public ObservableCollection<TagViewModel> AllTags { get; set; } = new ObservableCollection<TagViewModel>();
        public ObservableCollection<Tag> SelectedTags { get; set; } = [];
        public ICollectionView FilteredTags { get; set; }

        public AddTagToAnime(List<Tag>? selectedTags = null) {
            TagService tagService = new TagService(new DatabaseManager($"Data Source=db/data.sqlite"));
            if (AllTags.Count.Equals(0)) {
                List<Tag> tags = tagService.SelectAllTags();
                foreach (Tag x in tags) {
                    AllTags.Add(new TagViewModel(x));
                }
            }
            if (selectedTags is not null) {
                foreach (Tag x in selectedTags) {
                    SelectedTags.Add(x);
                    AllTags[AllTags.ToList().FindIndex(y => y.Tag.Id.Equals(x.Id))].IsSelected = true;
                }
            }
            InitializeComponent();
            DataContext = this;
            FilteredTags = CollectionViewSource.GetDefaultView(AllTags);
        }

        public bool IsTagSelected(Tag tag) {
            return SelectedTags.Contains(tag);
        }

        public void ToggleTagSelection(TagViewModel tagViewModel) {
            if (tagViewModel.IsSelected) {
                tagViewModel.IsSelected = false;
                SelectedTags.Remove(tagViewModel.Tag);
            }
            else {
                tagViewModel.IsSelected = true;
                SelectedTags.Add(tagViewModel.Tag);
            }
        }

        private void CheckBoxSelection_Click(object sender, RoutedEventArgs e) {
            CheckBox? checkBox = sender as CheckBox;
            TagViewModel? tagViewModel = checkBox?.DataContext as TagViewModel;
            if (tagViewModel is not null) {
                ToggleTagSelection(tagViewModel);
            }
        }

        public void FilterTags(string filter) {
            FilteredTags.Filter = obj => {
                if (obj is TagViewModel tagViewModel) {
                    return string.IsNullOrEmpty(filter) || tagViewModel.Tag.Designation.Contains(filter, StringComparison.OrdinalIgnoreCase);
                }
                return false;
            };
            FilteredTags.Refresh();
        }

        private void TBFilter_TextChanged(object sender, TextChangedEventArgs e) {
            TextBox? textBox = sender as TextBox;
            string filter = textBox!.Text;
            FilterTags(filter);
        }

        private void BtnFilterLeeren_Click(object sender, RoutedEventArgs e) {
            TBFilter.Text = "";
        }

        /// <summary></summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnFinish_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}
