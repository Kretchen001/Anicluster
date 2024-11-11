using Anicluster.Database;
using Anicluster.Database.Services;
using Modells.Anime;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Anicluster.windows {
    /// <summary>
    /// Interaktionslogik für TagOrganisator.xaml
    /// </summary>
    public partial class TagOrganisator : Window {

        public ObservableCollection<Tag> Tags { get; set; } = new ObservableCollection<Tag>();
        public Tag? SelectedTag { get; set; }
        private readonly TagService _tagService = new TagService(new DatabaseManager($"Data Source=db/data.sqlite"));

        /// <summary></summary>
        public TagOrganisator() {
            InitializeComponent();
            foreach (Tag x in _tagService.SelectAllTags()) {
                Tags.Add(x);
            }
            DataContext = this;
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
            if (!Tags.Any(x => x.Designation.Contains(InTxtBoxNewTag.Text, StringComparison.OrdinalIgnoreCase))) {
                Tag temp = new Tag(InTxtBoxNewTag.Text);
                temp.Id = _tagService.Insert(temp);
                Tags.Add(temp);
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
            if (Tags.Any(x => x.Designation.Equals(InTxtBoxNewTag.Text, StringComparison.OrdinalIgnoreCase))) {
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
                    Tags.Remove(SelectedTag);
                }
                else {
                    MessageBox.Show(owner: this, "Fehler beim Löschen!\nBitte beim Log aufbewahren und Programmierer informieren.");
                }
            }
            else {
                MessageBox.Show(owner: this, "Kein Tag ausgewählt zum Löschen.");
            }
        }
    }
}
