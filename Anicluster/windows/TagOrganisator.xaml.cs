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
            this.InitializeComponent();
            foreach (Tag x in this._tagService.SelectAllTags()) {
                this.AllTags.Add(x);
            }
            this.DataContext = this;
            this.FilteredTags = CollectionViewSource.GetDefaultView(this.AllTags);
        }

        /// <summary></summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemAddTag_Click(object sender, RoutedEventArgs e) {
            this.PopupInputNewTag.IsOpen = true;
        }

        /// <summary></summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnPopUp_Click(object sender, RoutedEventArgs e) {
            if (!this.AllTags.Any(x => x.Designation.Contains(this.InTxtBoxNewTag.Text, StringComparison.OrdinalIgnoreCase))) {
                Tag temp = new Tag(this.InTxtBoxNewTag.Text);
                temp.Id = this._tagService.Insert(temp);
                this.AllTags.Add(temp);
                MessageBox.Show(owner: this, $"Tag '{temp.Designation}' erfolgreich gespeichert!");
                this.InTxtBoxNewTag.Text = "";
            }
            else {
                MessageBox.Show(owner: this, $"Tag '{this.InTxtBoxNewTag.Text}' bereits vorhanden!");
            }
        }

        /// <summary></summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InputTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            if (this.AllTags.Any(x => x.Designation.Equals(this.InTxtBoxNewTag.Text, StringComparison.OrdinalIgnoreCase))) {
                this.InTxtBoxNewTag.Background = new SolidColorBrush(Color.FromArgb(128, 255, 0, 0)); // A=128 (50% Transparenz), R=255 (Rot), G=0, B=0
            }
            else {
                this.InTxtBoxNewTag.Background = new SolidColorBrush(Colors.LightGreen);
            }
        }

        /// <summary></summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItemDeleteSelectedTag_Click(object sender, RoutedEventArgs e) {
            if (this.SelectedTag is not null) {
                if (this._tagService.DeleteTagById(this.SelectedTag)) {
                    MessageBox.Show(owner: this, $"Tag '{this.SelectedTag.Designation}' erfolgreich gelöscht.");
                    this.AllTags.Remove(this.SelectedTag);
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
            this.GrdSearch.Visibility = this.GrdSearch.IsVisible ? Visibility.Collapsed : Visibility.Visible;
            this.MenuItemTagSearch.Header = this.GrdSearch.IsVisible ? "Suche ausblenden" : "Suche einblenden";
        }

        private void TxtBxSearchPattern_TextChanged(object sender, TextChangedEventArgs e) {
            this.FilteredTags.Filter = obj => {
                if (obj is Tag tagViewModel) {
                    return (
                        string.IsNullOrEmpty(this.TxtBxSearchPattern.Text) 
                        || tagViewModel.Designation.Contains(this.TxtBxSearchPattern.Text, StringComparison.OrdinalIgnoreCase)
                    );
                }
                return false;
            };
            this.FilteredTags.Refresh();
        }

        private void BtnSearchPatternEmpty_Click(object sender, RoutedEventArgs e) {
            this.TxtBxSearchPattern.Text = "";
        }
    }
}
