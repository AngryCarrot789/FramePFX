using FFmpeg.Wrapper;

namespace FramePFX.ResourceManaging.Items {
    public class ResourceVideoMedia : ResourceItem {
        private string filePath;
        public string FilePath {
            get => this.filePath;
            set => this.RaisePropertyChanged(ref this.filePath, value);
        }

        // TODO: Somehow process video frames here, and provide frame caching maybe?
        public MediaDemuxer Demuxer { get; private set; }

        public override void DisposeResource() {
            this.Demuxer?.Dispose();
        }
    }
}