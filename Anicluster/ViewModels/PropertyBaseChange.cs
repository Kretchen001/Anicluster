using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Anicluster.ViewModels {

    /// <summary>
    /// Basisklasse für alle ViewModels. Stellt <see cref="INotifyPropertyChanged"/> bereit,
    /// damit die Views auf Änderungen reagieren können.
    /// </summary>
    public abstract class PropertyBaseChange : INotifyPropertyChanged {

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Setzt das Feld und benachrichtigt die View, wenn sich der Wert geändert hat.
        /// </summary>
        /// <returns><c>true</c>, wenn sich der Wert geändert hat.</returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
            if (EqualityComparer<T>.Default.Equals(field, value)) {
                return false;
            }
            field = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
