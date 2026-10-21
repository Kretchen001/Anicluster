namespace Anicluster.ViewModels {

    /// <summary>
    /// Basisklasse für ViewModels, die in einem modalen Dialog angezeigt werden.
    /// Der <see cref="Dialogs.IDialogService"/> schließt den Dialog, sobald <see cref="CloseRequested"/> ausgelöst wird.
    /// </summary>
    public abstract class DialogViewModelBase : PropertyBaseChange {

        /// <summary>Titel des Dialogfensters</summary>
        public abstract string Title { get; }

        /// <summary>
        /// Soll das Dialogfenster mit der Größe des übergeordneten Fensters wachsen (mindestens so groß wie sein Inhalt)?
        /// Kleine Eingaben und Rückfragen lassen es bei <c>false</c> und behalten die Größe ihres Inhalts.
        /// </summary>
        public virtual bool FillsOwnerWindow => false;

        /// <summary>Wird ausgelöst, wenn der Dialog geschlossen werden soll (Wert = Dialogergebnis).</summary>
        public event EventHandler<bool>? CloseRequested;

        /// <param name="dialogResult"><c>true</c> = übernommen, <c>false</c> = abgebrochen</param>
        protected void RequestClose(bool dialogResult) {
            this.CloseRequested?.Invoke(this, dialogResult);
        }
    }
}
