using Anicluster.windows;
using Anicluster.dialogs;
using Modells.Anime.SoundRating;
using System.Collections.ObjectModel;
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
                NewSyncro = syncro;
            }
            InitializeComponent();
            DataContext = this;
            foreach (KeyValuePair<SyncroLanuage, string> x in TranslationOfLanguage) {
                Languages.Add(x);
            }
            ProgBarGeneral.Value = 0;
            CBLanguageSelection.SelectedIndex = 0;
            NewSyncro.General = 0;
        }

        private void ProgBarGeneral_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e) {
            if (e.Delta > 0) {
                NewSyncro.General = Math.Min(NewSyncro.General + 1, 100);
            }
            else {
                NewSyncro.General = Math.Max(NewSyncro.General - 1, 0);
            }
            ProgBarGeneral.Value = NewSyncro.General; // Update ProgressBar
        }

        private void ProgBarGeneral_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            NumberInputDialog inputDialog = new NumberInputDialog();
            if (inputDialog.ShowDialog() == true) {
                NewSyncro.General = inputDialog.Result;
                ProgBarGeneral.Value = inputDialog.Result;
            }
        }
    }
}
