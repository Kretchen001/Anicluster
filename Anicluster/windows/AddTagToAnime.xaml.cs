using Modells.Anime;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Anicluster.windows {
    /// <summary>
    /// Interaktionslogik für AddTagToAnime.xaml
    /// </summary>
    public partial class AddTagToAnime : Window {

        public ObservableCollection<Tag> AllTags { get; set; } = new ObservableCollection<Tag>();
        public ObservableCollection<Tag> SelectedTags { get; set; } = [];

        public AddTagToAnime(List<Tag>? tags = null) {
            if (tags is not null) {
                foreach (Tag x in tags) {
                    SelectedTags.Add(x);
                }
            }
            InitializeComponent();
            DataContext = this;
        }

        public bool IsTagSelected(Tag tag) {
            return SelectedTags.Contains(tag);
        }

        public void ToggleTagSelection(Tag tag) {
            if (SelectedTags.Contains(tag)) {
                SelectedTags.Remove(tag);
            }
            else {
                SelectedTags.Add(tag);
            }
        }

        private void CheckBoxSelection_Click(object sender, RoutedEventArgs e) {
            CheckBox? checkBox = sender as CheckBox;
            Tag? tag = checkBox?.DataContext as Tag;
            if (tag is not null) {
                ToggleTagSelection(tag);
            }
        }
    }
}
