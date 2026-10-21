using Anicluster.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Anicluster.Views {

    /// <summary>
    /// Tag-Netzwerk. Der Code-Behind übersetzt nur Maus-Gesten (Klick, Ziehen, Überfahren, Mausrad-Zoom, Verschieben des Ausschnitts)
    /// in Aufrufe am <see cref="TagNetworkViewModel"/> und stößt das Neuzeichnen der Linien an.
    /// </summary>
    public partial class TagNetworkView : UserControl {

        /// <summary>Bewegung in Pixeln, ab der ein Klick als Ziehen gilt.</summary>
        private const double DragThreshold = 4.0;

        private TagNetworkViewModel? _subscribedViewModel;
        private TagNodeViewModel? _dragNode;
        private Point _dragOffset;
        private Point _downPosition;
        private bool _moved;
        private bool _panning;
        private bool _panMoved;
        private Point _panStart;
        private double _panOffsetX;
        private double _panOffsetY;

        private TagNetworkViewModel? ViewModel => this.DataContext as TagNetworkViewModel;

        public TagNetworkView() {
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

        /// <summary>Das Mausrad zoomt. Die Stelle unter dem Mauszeiger bleibt dabei an ihrem Platz.</summary>
        private void ViewportHost_PreviewMouseWheel(object sender, MouseWheelEventArgs e) {
            TagNetworkViewModel? viewModel = this.ViewModel;
            if (viewModel is null) {
                return;
            }
            Point anchor = e.GetPosition(this.ViewportHost);
            double contentX = this.ViewportHost.HorizontalOffset + anchor.X;
            double contentY = this.ViewportHost.VerticalOffset + anchor.Y;
            double before = viewModel.Zoom;
            viewModel.ZoomBy(e.Delta > 0 ? 1 : -1);
            double ratio = viewModel.Zoom / before;
            if (Math.Abs(ratio - 1.0) > 1e-9) {
                this.ViewportHost.UpdateLayout();
                this.ViewportHost.ScrollToHorizontalOffset(contentX * ratio - anchor.X);
                this.ViewportHost.ScrollToVerticalOffset(contentY * ratio - anchor.Y);
            }
            e.Handled = true;
        }

        #region Freie Fläche: Klick hebt die Auswahl auf, Ziehen verschiebt den Ausschnitt
        /// <summary>Klicks auf Knoten werden dort behandelt und kommen hier nicht an.</summary>
        private void World_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            this._panning = true;
            this._panMoved = false;
            this._panStart = e.GetPosition(this.ViewportHost);
            this._panOffsetX = this.ViewportHost.HorizontalOffset;
            this._panOffsetY = this.ViewportHost.VerticalOffset;
            this.WorldHost.CaptureMouse();
        }

        private void World_MouseMove(object sender, MouseEventArgs e) {
            if (!this._panning || e.LeftButton != MouseButtonState.Pressed) {
                return;
            }
            // Gemessen im sichtbaren Bereich, der beim Verschieben selbst stillsteht
            Point position = e.GetPosition(this.ViewportHost);
            double deltaX = position.X - this._panStart.X;
            double deltaY = position.Y - this._panStart.Y;
            if (!this._panMoved && Math.Abs(deltaX) < DragThreshold && Math.Abs(deltaY) < DragThreshold) {
                return;
            }
            this._panMoved = true;
            this.WorldHost.Cursor = Cursors.SizeAll;
            this.ViewportHost.ScrollToHorizontalOffset(this._panOffsetX - deltaX);
            this.ViewportHost.ScrollToVerticalOffset(this._panOffsetY - deltaY);
        }

        private void World_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            if (!this._panning) {
                return;
            }
            this._panning = false;
            this.WorldHost.ReleaseMouseCapture();
            this.WorldHost.ClearValue(FrameworkElement.CursorProperty);
            if (!this._panMoved) {
                this.ViewModel?.ClearSelection();
            }
        }
        #endregion

        #region Knoten: Klick, Ziehen, Überfahren
        private void Node_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (sender is FrameworkElement { DataContext: TagNodeViewModel node } element) {
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
            if (!this._moved && Math.Abs(position.X - this._downPosition.X) < DragThreshold && Math.Abs(position.Y - this._downPosition.Y) < DragThreshold) {
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
                    this.ViewModel?.ClickNode(this._dragNode, Keyboard.Modifiers.HasFlag(ModifierKeys.Control));
                }
                this._dragNode = null;
            }
            e.Handled = true;
        }

        private void Node_MouseEnter(object sender, MouseEventArgs e) {
            if (sender is FrameworkElement { DataContext: TagNodeViewModel node }) {
                this.ViewModel?.SetHover(node);
            }
        }

        private void Node_MouseLeave(object sender, MouseEventArgs e) {
            this.ViewModel?.SetHover(null);
        }

        private void NodeMenuHide_Click(object sender, RoutedEventArgs e) {
            if (sender is FrameworkElement { DataContext: TagNodeViewModel node }) {
                this.ViewModel?.DisableNode(node);
            }
        }
        #endregion

        private void Result_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (e.ClickCount == 2 && sender is FrameworkElement { DataContext: AnimeListItemViewModel item }) {
                item.RequestEdit();
            }
        }

        private void BtnRemoveSelected_Click(object sender, RoutedEventArgs e) {
            if (sender is FrameworkElement { DataContext: TagNodeViewModel node }) {
                this.ViewModel?.RemoveFromSelection(node);
            }
        }

        private void BtnClearSelection_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.ClearSelection();
        }

        private void BtnShowInLibrary_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.ShowInLibrary();
        }

        private void BtnEnableAll_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.EnableAll();
        }

        private void BtnDisableAll_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.DisableAll();
        }

        private void BtnZoomReset_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.ResetZoom();
            this.ViewportHost.ScrollToHome();
        }

        private void BtnRelayout_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.Relayout();
        }
    }
}
