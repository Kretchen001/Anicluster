using Anicluster.Database;
using Anicluster.Database.Services;
using Modells.Anime;
using Modells.ViewModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Anicluster.windows {
    /// <summary>
    /// Interaktionslogik für TagOrganisator.xaml
    /// </summary>
    public partial class TagOrganisator : Window {

        public ObservableCollection<Tag> AllTags { get; set; } = new ObservableCollection<Tag>();
        public Tag? SelectedTag { get; set; }
        private readonly TagService _tagService = new TagService(new DatabaseManager($"Data Source=db/data.sqlite"));
        public ICollectionView FilteredTags { get; set; }

        /// <summary></summary>
        public TagOrganisator() {
            InitializeComponent();
            foreach (Tag x in _tagService.SelectAllTags()) {
                AllTags.Add(x);
            }
            DataContext = this;
            FilteredTags = CollectionViewSource.GetDefaultView(AllTags);
        }

        /// <summary></summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemAddTag_Click(object sender, RoutedEventArgs e) {
            PopupInputNewTag.IsOpen = true;
        }

        /// <summary></summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnPopUp_Click(object sender, RoutedEventArgs e) {
            if (!AllTags.Any(x => x.Designation.Contains(InTxtBoxNewTag.Text, StringComparison.OrdinalIgnoreCase))) {
                Tag temp = new Tag(InTxtBoxNewTag.Text);
                temp.Id = _tagService.Insert(temp);
                AllTags.Add(temp);
                MessageBox.Show(owner: this, $"Tag '{temp.Designation}' erfolgreich gespeichert!");
                InTxtBoxNewTag.Text = "";
            }
            else {
                MessageBox.Show(owner: this, $"Tag '{InTxtBoxNewTag.Text}' bereits vorhanden!");
            }
        }

        /// <summary></summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InputTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            if (AllTags.Any(x => x.Designation.Equals(InTxtBoxNewTag.Text, StringComparison.OrdinalIgnoreCase))) {
                InTxtBoxNewTag.Background = new SolidColorBrush(Color.FromArgb(128, 255, 0, 0)); // A=128 (50% Transparenz), R=255 (Rot), G=0, B=0
            }
            else {
                InTxtBoxNewTag.Background = new SolidColorBrush(Colors.LightGreen);
            }
        }

        /// <summary></summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemDeleteSelectedTag_Click(object sender, RoutedEventArgs e) {
            if (SelectedTag is not null) {
                if (_tagService.DeleteTagById(SelectedTag)) {
                    MessageBox.Show(owner: this, $"Tag '{SelectedTag.Designation}' erfolgreich gelöscht.");
                    AllTags.Remove(SelectedTag);
                }
                else {
                    MessageBox.Show(owner: this, "Fehler beim Löschen!\nBitte beim Log aufbewahren und Programmierer informieren.");
                }
            }
            else {
                MessageBox.Show(owner: this, "Kein Tag ausgewählt zum Löschen.");
            }
        }

        private void MenuItemTagSearch_Click(object sender, RoutedEventArgs e) {
            GrdSearch.Visibility = GrdSearch.IsVisible ? Visibility.Collapsed : Visibility.Visible;
            MenuItemTagSearch.Header = GrdSearch.IsVisible ? "Suche ausblenden" : "Suche einblenden";
        }

        private void TxtBxSearchPattern_TextChanged(object sender, TextChangedEventArgs e) {
            FilteredTags.Filter = obj => {
                if (obj is Tag tagViewModel) {
                    return (
                        string.IsNullOrEmpty(TxtBxSearchPattern.Text) 
                        || tagViewModel.Designation.Contains(TxtBxSearchPattern.Text, StringComparison.OrdinalIgnoreCase)
                    );
                }
                return false;
            };
            FilteredTags.Refresh();
        }

        private void BtnSearchPatternEmpty_Click(object sender, RoutedEventArgs e) {
            TxtBxSearchPattern.Text = "";
        }
    }
}
