using FramePFX.Core.Editor.Timeline.Clip;
using FramePFX.Core.RBC;
using FramePFX.Core.ResourceManaging;

namespace FramePFX.Core.Editor {
    /// <summary>
    /// Base video clip class that uses a single resources
    /// </summary>
    /// <typeparam name="T">Resource type</typeparam>
    public abstract class BaseResourceVideoClip<T> : VideoClipModel where T : ResourceItem {
        public delegate void ResourceStateChangedEventHandler(VideoClipModel sender);

        private string imageResourceId;
        public string ImageResourceId {
            get => this.imageResourceId;
            set {
                this.imageResourceId = value;
                this.IsResourceOffline = false;
            }
        }

        private bool isResourceOffline;
        public bool IsResourceOffline {
            get => this.isResourceOffline;
            set {
                this.isResourceOffline = value;
                this.OnResourceStateChanged();
            }
        }

        private T cachedItem;

        public event ResourceStateChangedEventHandler ResourceStateChanged;

        protected BaseResourceVideoClip() {

        }

        protected virtual void OnResourceStateChanged() {
            this.ResourceStateChanged?.Invoke(this);
        }

        public bool TryGetResource(out T resource) {
            if (this.IsResourceOffline) {
                resource = default;
                return false;
            }

            if (this.cachedItem != null) {
                resource = this.cachedItem;
                return true;
            }

            if (this.Layer == null || string.IsNullOrWhiteSpace(this.ImageResourceId)) {
                resource = null;
                return false;
            }

            // Realistically, this code shouldn't be run, because when the resource manager and clip are all loaded, there
            // should be a function that runs to detect missing resource ids, and offer to replace them or just offline the clip
            // And eventually a "removed" event should be created
            ResourceManager manager = this.Layer.Timeline.Project.ResourceManager;
            if (!manager.TryGetResource(this.ImageResourceId, out ResourceItem resItem) || !(resItem is T r)) {
                this.IsResourceOffline = true;
                resource = default;
                return false;
            }

            this.cachedItem = resource = r;
            return true;
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            if (data.TryGetString(nameof(this.ImageResourceId), out string id)) {
                this.ImageResourceId = id;
            }
        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            if (!string.IsNullOrEmpty(this.ImageResourceId))
                data.SetString(nameof(this.ImageResourceId), this.ImageResourceId);
        }
    }
}