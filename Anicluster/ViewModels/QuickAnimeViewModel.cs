using Anicluster.Database.Services;
using Anicluster.Dialogs;
using Modells.Anime;
using System.Windows;

namespace Anicluster.ViewModels {

    /// <summary>
    /// Dialog zum schnellen Anlegen eines Animes nur mit Name, Favorit und Tier.
    /// </summary>
    public class QuickAnimeViewModel : DialogViewModelBase {

        private readonly IAnimeService _animeService;
        private readonly IDialogService _dialogService;
        private string _name = string.Empty;
        private bool _isFavorite;
        private Tier _tier = Tier.NotDefined;

        public override string Title => "Neuer Anime (kurz)";

        public string Name {
            get => this._name;
            set => this.SetProperty(ref this._name, value);
        }

        public bool IsFavorite {
            get => this._isFavorite;
            set => this.SetProperty(ref this._isFavorite, value);
        }

        public Tier Tier {
            get => this._tier;
            set => this.SetProperty(ref this._tier, value);
        }

        public QuickAnimeViewModel(IAnimeService animeService, IDialogService dialogService) {
            this._animeService = animeService;
            this._dialogService = dialogService;
        }

        public void SetTier(Tier tier) {
            this.Tier = tier;
        }

        public void ToggleFavorite() {
            this.IsFavorite = !this.IsFavorite;
        }

        /// <summary>Speichert den Anime und schließt den Dialog.</summary>
        public void Save() {
            if (string.IsNullOrWhiteSpace(this.Name)) {
                this._dialogService.ShowMessage("Pflichtfelder sind leider nicht ausgefüllt!", string.Empty, MessageBoxImage.Warning);
                return;
            }
            if (this._animeService.InsertFast(this.Name, this.IsFavorite, this.Tier)) {
                this.RequestClose(true);
            }
            else {
                this._dialogService.ShowMessage("Fehler beim Speichern!\nBitte beim Log aufbewahren und Programmierer informieren.", string.Empty, MessageBoxImage.Error);
            }
        }
    }
}
