using Anicluster.ViewModels;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Anicluster.Views {

    /// <summary>
    /// Gemeinsame View zum Anlegen und Bearbeiten eines Animes.
    /// Die Klick-Handler leiten nur an das <see cref="AnimeEditorViewModel"/> weiter,
    /// im Code-Behind liegen nur das Umschalten des Namensfelds und die Übergabe von Bildern aus Zwischenablage und Drag and Drop.
    /// </summary>
    public partial class AnimeEditorView : UserControl {

        private AnimeEditorViewModel? ViewModel => this.DataContext as AnimeEditorViewModel;

        public AnimeEditorView() {
            this.InitializeComponent();
        }

        /// <summary>Bei einem neuen Anime ohne Namen startet das Namensfeld direkt im Eingabemodus.</summary>
        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            if (string.IsNullOrEmpty(this.ViewModel?.Name)) {
                this.ShowNameEditor();
            }
        }

        #region Name (Anzeige <-> Eingabe)
        private void ShowNameEditor() {
            this.TBName.Visibility = Visibility.Visible;
            this.LbName.Visibility = Visibility.Collapsed;
            this.TBName.Focus();
        }

        private void ShowNameLabel() {
            this.TBName.Visibility = Visibility.Collapsed;
            this.LbName.Visibility = Visibility.Visible;
        }

        private void LbName_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            this.ShowNameEditor();
        }

        private void TBName_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                this.ShowNameLabel();
            }
        }

        private void TBName_LostFocus(object sender, RoutedEventArgs e) {
            this.ShowNameLabel();
        }
        #endregion

        private void BtnAutoLinkGeneration_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.GenerateUrl();
        }

        private void BtnRatingGenerator_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.EditRating();
        }

        private void BtnSeasonGenerator_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.EditSeasons();
        }

        private void BtnOvaGenerator_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.EditOvas();
        }

        private void BtnTagsSelector_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.EditTags();
        }

        private void BtnPublishingTime_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.EditPublishingTime();
        }

        #region Bild
        private void BtnSelectImage_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.SelectImage();
        }

        private void BtnRemoveImage_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.RemoveImage();
        }

        /// <summary>Übernimmt ein kopiertes Bild oder eine kopierte Bilddatei aus der Zwischenablage.</summary>
        private void BtnPasteImage_Click(object sender, RoutedEventArgs e) {
            if (this.ViewModel is null) {
                return;
            }
            try {
                if (Clipboard.ContainsFileDropList() && Clipboard.GetFileDropList() is { Count: > 0 } files && files[0] is string path) {
                    this.ViewModel.SetImageFromFile(path);
                    return;
                }
                if (Clipboard.ContainsImage() && Clipboard.GetImage() is BitmapSource bitmap) {
                    this.ViewModel.SetImage(EncodeAsPng(bitmap));
                    return;
                }
            }
            catch (COMException) {
                // Die Zwischenablage ist gerade von einem anderen Programm gesperrt, der Benutzer kann es erneut versuchen
            }
            this.ViewModel.ReportNoImageInClipboard();
        }

        private void ImageCard_DragOver(object sender, DragEventArgs e) {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }

        private void ImageCard_Drop(object sender, DragEventArgs e) {
            if (e.Data.GetData(DataFormats.FileDrop) is string[] { Length: > 0 } files) {
                this.ViewModel?.SetImageFromFile(files[0]);
            }
            e.Handled = true;
        }

        /// <summary>Kodiert ein Bild aus der Zwischenablage verlustfrei, das ViewModel behandelt es dann wie eine Bilddatei.</summary>
        private static byte[] EncodeAsPng(BitmapSource bitmap) {
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using (MemoryStream stream = new MemoryStream()) {
                encoder.Save(stream);
                return stream.ToArray();
            }
        }
        #endregion

        private void BtnSave_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.Save();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) {
            this.ViewModel?.Cancel();
        }
    }
}
