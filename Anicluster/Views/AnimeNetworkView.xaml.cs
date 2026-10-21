using Anicluster.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Anicluster.Views {

    /// <summary>
    /// Anime-Netzwerk. Der Code-Behind übersetzt nur Maus-Gesten auf den Knoten (Klick, Doppelklick, Ziehen, Überfahren) und das
    /// Strg + Mausrad der Fläche in Aufrufe am <see cref="AnimeNetworkViewModel"/> und stößt das Neuzeichnen der Linien an.
    /// </summary>
    public partial class AnimeNetworkView : UserControl {

        /// <summary>Bewegung in Pixeln, ab der ein Klick als Ziehen gilt.</summary>
        private const double DragThreshold = 4.0;

        private AnimeNetworkViewModel? _subscribedViewModel;
        private AnimeNodeViewModel? _dragNode;
        private Point _dragOffset;
        private Point _downPosition;
        private bool _moved;

        private AnimeNetworkViewModel? ViewModel => this.DataContext as AnimeNetworkViewModel;

        public AnimeNetworkView() {
            this.InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            this._subscribedViewModel = this.ViewModel;
            if (this._subscribedViewModel is not null) {
                this._subscribedViewModel.Redraw += this.OnRedraw;
            }
            this.UpdateViewport();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e) {
            if (this._subscribedViewModel is not null) {
                this._subscribedViewModel.Redraw -= this.OnRedraw;
                this._subscribedViewModel = null;
            }
        }

        private void OnRedraw(object? sender, EventArgs e) {
            this.EdgeLayer.InvalidateVisual();
        }

        private void ViewportHost_SizeChanged(object sender, SizeChangedEventArgs e) {
            this.UpdateViewport();
        }

        private void UpdateViewport() {
            this.ViewModel?.SetViewport(this.ViewportHost.ActualWidth, this.ViewportHost.ActualHeight);
        }

        /// <summary>Strg + Mausrad zoomt, ohne Strg scrollt die Fläche wie gewohnt.</summary>
        private void ViewportHost_PreviewMouseWheel(object sender, MouseWheelEventArgs e) {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) {
                this.ViewModel?.ZoomBy(e.Delta > 0 ? 1 : -1);
                e.Handled = true;
            }
        }

        /// <summary>Ein Klick ins Leere hebt die Auswahl auf (Klicks auf Knoten werden dort behandelt und nicht weitergereicht).</summary>
        private void World_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            this.ViewModel?.ClearSelection();
        }

        #region Knoten: Klick, Doppelklick, Ziehen, Überfahren
        private void Node_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (sender is FrameworkElement { DataContext: AnimeNodeViewModel node } element) {
                if (e.ClickCount == 2) {
                    this.ViewModel?.RequestEdit(node);
                    e.Handled = true;
                    return;
                }
                Point position = e.GetPosition(this.WorldHost);
                this._dragNode = node;
                this._dragOffset = new Point(node.CenterX - position.X, node.CenterY - position.Y);
                this._downPosition = position;
                this._moved = false;
                element.CaptureMouse();
                e.Handled = true;
            }
        }

        private void Node_MouseMove(object sender, MouseEventArgs e) {
            if (this._dragNode is null || e.LeftButton != MouseButtonState.Pressed) {
                return;
            }
            Point position = e.GetPosition(this.WorldHost);
            if (!this._moved
                && Math.Abs(position.X - this._downPosition.X) < DragThreshold
                && Math.Abs(position.Y - this._downPosition.Y) < DragThreshold) {
                return;
            }
            this._moved = true;
            this.ViewModel?.DragNode(this._dragNode, position.X + this._dragOffset.X, position.Y + this._dragOffset.Y);
        }

        private void Node_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            if (sender is FrameworkElement element) {
                element.ReleaseMouseCapture();
            }
            if (this._dragNode is not null) {
                if (this._moved) {
                    this.ViewModel?.EndDrag(this._dragNode);
                }
                else {
                    this.ViewModel?.ClickNode(this._dragNode);
                }
                this._dragNode = null;
            }
            e.Handled = true;
        }

        private void Node_MouseEnter(object sender, MouseEventArgs e) {
            if (sender is FrameworkElement { DataContext: AnimeNodeViewModel node }) {
                this.ViewModel?.SetHover(node);
            }
        }

        private void Node_MouseLeave(object sender, MouseEventArgs e) {
            this.ViewModel?.SetHover(null);
        }
        #endregion

        private void Neighbor_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (sender is FrameworkElement { DataContext: AnimeNeighborViewModel neighbor }) {
                this.ViewModel?.SelectNode(neighbor.Node);
            }
        }

        private void BtnOpenSelected_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.OpenSelected();
        }

        private void BtnSearchInLibrary_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.SearchSelectedInLibrary();
        }

        private void BtnClearSelection_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.ClearSelection();
        }

        private void BtnRelayout_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.Relayout();
        }
    }
}
