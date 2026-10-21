using Modells.Anime;
using System.Collections.ObjectModel;

namespace Anicluster.ViewModels {

    /// <summary>
    /// Dialog zum Verwalten von OVAs, Filmen und anderen Video-Formaten eines Animes.
    /// Arbeitet auf Kopien, das Ergebnis steht nach dem Übernehmen in <see cref="Result"/>.
    /// </summary>
    public class VideoAnimationEditorViewModel : DialogViewModelBase {

        private VideoType _selectedType = VideoType.Episode;
        private string _comment = string.Empty;
        private VideoAnimation? _selectedVideo;

        public override string Title => "OVA / Movie Generator";

        public override bool FillsOwnerWindow => true;

        public ObservableCollection<VideoAnimation> Videos { get; } = [];

        /// <summary>Auswahlliste für den Typ</summary>
        public IEnumerable<VideoType> VideoTypes { get; } = Enum.GetValues<VideoType>();

        public VideoType SelectedType {
            get => this._selectedType;
            set => this.SetProperty(ref this._selectedType, value);
        }

        public string Comment {
            get => this._comment;
            set => this.SetProperty(ref this._comment, value);
        }

        public VideoAnimation? SelectedVideo {
            get => this._selectedVideo;
            set => this.SetProperty(ref this._selectedVideo, value);
        }

        /// <summary>Die übernommenen Videos, leer solange nicht übernommen wurde.</summary>
        public List<VideoAnimation> Result { get; private set; } = [];

        public VideoAnimationEditorViewModel(IEnumerable<VideoAnimation> videos) {
            foreach (VideoAnimation video in videos) {
                this.Videos.Add(new VideoAnimation(video.Id, video.VideoType, video.Comment));
            }
            this.UpdateIds();
        }

        public void AddVideo() {
            this.Videos.Add(new VideoAnimation(
                this.Videos.Count + 1,
                this.SelectedType,
                string.IsNullOrWhiteSpace(this.Comment) ? null : this.Comment
            ));
        }

        public void RemoveSelectedVideo() {
            if (this.SelectedVideo is not null) {
                this.Videos.Remove(this.SelectedVideo);
                this.UpdateIds();
            }
        }

        /// <summary>Verschiebt ein Video (Drag &amp; Drop) an die angegebene Position.</summary>
        public void MoveVideo(VideoAnimation video, int targetIndex) {
            this.Videos.Remove(video);
            this.Videos.Insert(Math.Clamp(targetIndex, 0, this.Videos.Count), video);
            this.UpdateIds();
        }

        public void Accept() {
            this.Result = new List<VideoAnimation>(this.Videos);
            this.RequestClose(true);
        }

        public void Cancel() {
            this.RequestClose(false);
        }

        /// <summary>
        /// Nummeriert die Videos entsprechend der Reihenfolge neu. <see cref="VideoAnimation"/> meldet Änderungen
        /// nicht selbst, deshalb wird die Liste neu aufgebaut, damit die View die neuen Nummern anzeigt.
        /// </summary>
        private void UpdateIds() {
            List<VideoAnimation> ordered = this.Videos.ToList();
            for (int i = 0; i < ordered.Count; i += 1) {
                ordered[i].Id = i + 1;
            }
            this.Videos.Clear();
            foreach (VideoAnimation video in ordered) {
                this.Videos.Add(video);
            }
        }
    }
}
