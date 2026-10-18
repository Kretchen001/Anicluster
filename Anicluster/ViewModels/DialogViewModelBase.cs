namespace Anicluster.ViewModels {

    /// <summary>
    /// Basisklasse für ViewModels, die in einem modalen Dialog angezeigt werden.
    /// Der <see cref="Dialogs.IDialogService"/> schließt den Dialog, sobald <see cref="CloseRequested"/> ausgelöst wird.
    /// </summary>
    public abstract class DialogViewModelBase : PropertyBaseChange {

        /// <summary>Titel des Dialogfensters</summary>
        public abstract string Title { get; }

        /// <summary>Wird ausgelöst, wenn der Dialog geschlossen werden soll (Wert = Dialogergebnis).</summary>
        public event EventHandler<bool>? CloseRequested;

        /// <param name="dialogResult"><c>true</c> = übernommen, <c>false</c> = abgebrochen</param>
        protected void RequestClose(bool dialogResult) {
            this.CloseRequested?.Invoke(this, dialogResult);
        }
    }
}
