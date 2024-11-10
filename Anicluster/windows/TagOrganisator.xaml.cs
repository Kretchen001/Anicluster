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
        private TagService _tagService = new TagService(new DatabaseManager($"Data Source=db/data.sqlite"));

        public TagOrganisator() {
            InitializeComponent();
            foreach (Tag x in _tagService.SelectAllTags()) {
                Tags.Add(x);
            }
            DataContext = this;
        }

        private void MenuItemAddTag_Click(object sender, RoutedEventArgs e) {
            PopupInputNewTag.IsOpen = true;
        }

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

        private void InputTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            if (Tags.Any(x => x.Designation.Contains(InTxtBoxNewTag.Text, StringComparison.OrdinalIgnoreCase))) {
                InTxtBoxNewTag.Background = new SolidColorBrush(Color.FromArgb(128, 255, 0, 0)); // A=128 (50% Transparenz), R=255 (Rot), G=0, B=0
            }
            else {
                InTxtBoxNewTag.Background = new SolidColorBrush(Colors.LightGreen);
            }
        }
    }
}
