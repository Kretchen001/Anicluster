using Anicluster.dialogs;
using Modells.Anime.SoundRating;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using AnimeSR = Modells.Anime.SoundRating;

namespace Anicluster.userControls {
    /// <summary>
    /// Interaktionslogik für Syncro.xaml
    /// </summary>
    public partial class Syncro : UserControl {

        private static Dictionary<SyncroLanuage, string> TranslationOfLanguage = new Dictionary<SyncroLanuage, string>() {
            { SyncroLanuage.en, "Englisch" },
            { SyncroLanuage.jp, "Japanisch" },
            { SyncroLanuage.de, "Deutsch" },
            { SyncroLanuage.es, "Spanisch" },
            { SyncroLanuage.ru, "Russisch" },
            { SyncroLanuage.ko, "Koreanisch" },
            { SyncroLanuage.ch, "Chinesisch" },
        };
        public ObservableCollection<KeyValuePair<SyncroLanuage, string>> Languages { get; set; } = [];

        public AnimeSR.Syncro NewSyncro { get; set; } = new AnimeSR.Syncro();

        public Syncro(AnimeSR.Syncro? syncro = null) {
            if (syncro is not null) {
                this.NewSyncro = syncro;
            }
            this.InitializeComponent();
            this.DataContext = this;
            foreach (KeyValuePair<SyncroLanuage, string> x in TranslationOfLanguage) {
                this.Languages.Add(x);
            }
            this.NewSyncro.General = this.NewSyncro.General.Equals(-1) ? 0 : this.NewSyncro.General;
            this.ProgBarGeneral.Value = this.NewSyncro.General;
            this.CBLanguageSelection.SelectedIndex = this.NewSyncro.General;
        }

        private void ProgBarGeneral_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e) {
            if (e.Delta > 0) {
                this.NewSyncro.General = Math.Min(this.NewSyncro.General + 1, 100);
            }
            else {
                this.NewSyncro.General = Math.Max(this.NewSyncro.General - 1, 0);
            }
            this.ProgBarGeneral.Value = this.NewSyncro.General; // Update ProgressBar
        }

        private void ProgBarGeneral_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            NumberInputDialog inputDialog = new NumberInputDialog();
            if (inputDialog.ShowDialog() == true) {
                this.NewSyncro.General = inputDialog.Result;
                this.ProgBarGeneral.Value = inputDialog.Result;
            }
        }

        public event EventHandler RemoveThisSynco;
        private void MenuItem_Delete_Click(object sender, RoutedEventArgs e) {
            RemoveThisSynco.Invoke(this, e);
        }

        private void BtnRemoveEntry_Click(object sender, RoutedEventArgs e) {
            RemoveThisSynco(this, e);
        }
    }
}
