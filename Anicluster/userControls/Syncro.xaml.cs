using Anicluster.dialogs;
using Modells.Anime.SoundRating;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using AnimeSR = Modells.Anime.SoundRating;

namespace Anicluster.userControls {
    /// <summary>
    /// Interaktionslogik für Syncro.xaml
    /// </summary>
    public partial class Syncro : UserControl, INotifyPropertyChanged {

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

        private AnimeSR.Syncro _NewSyncro = new AnimeSR.Syncro();
        public AnimeSR.Syncro NewSyncro {
            get {
                return this._NewSyncro;
            }
            set {
                if (this._NewSyncro != value) {
                    this._NewSyncro = value;
                    this.OnPropertyChanged(nameof(this._NewSyncro));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public Syncro(AnimeSR.Syncro? syncro = null) {
            if (syncro is not null) {
                this.NewSyncro = syncro;
            }
            this.InitializeComponent();
            foreach (KeyValuePair<SyncroLanuage, string> x in TranslationOfLanguage) {
                this.Languages.Add(x);
            }
            this.NewSyncro.General = this.NewSyncro.General.Equals(-1) ? 0 : this.NewSyncro.General;
            this.DataContext = this;
            //this.CBLanguageSelection.SelectedIndex = this.NewSyncro.General;
        }

        private void ProgBarGeneral_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e) {
            if (e.Delta > 0) {
                this.NewSyncro.General = Math.Min(this.NewSyncro.General + 1, 100);
            }
            else {
                this.NewSyncro.General = Math.Max(this.NewSyncro.General - 1, 0);
            }
        }

        private void ProgBarGeneral_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            NumberInputDialog inputDialog = new NumberInputDialog(this.NewSyncro.General);
            if (inputDialog.ShowDialog() == true) {
                this.NewSyncro.General = inputDialog.Result;
            }
        }

        public event EventHandler RemoveThisSynco;
        private void MenuItem_Delete_Click(object sender, RoutedEventArgs e) {
            RemoveThisSynco.Invoke(this, e);
        }

        private void BtnRemoveEntry_Click(object sender, RoutedEventArgs e) {
            RemoveThisSynco(this, e);
        }

        protected void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
