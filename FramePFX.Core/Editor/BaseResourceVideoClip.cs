using System.Diagnostics;
using FramePFX.Core.Editor.ResourceManaging;
using FramePFX.Core.Editor.Timeline.Clip;
using FramePFX.Core.RBC;

namespace FramePFX.Core.Editor {
    // I don't use this class, but i'm keeping it anyway because why not
    public abstract class BaseResourceVideoClip<T> : VideoClipModel where T : ResourceItem {
        public delegate void ResourceStateChangedEventHandler(VideoClipModel sender);
        public delegate void ResourceIdChangedEventHandler(VideoClipModel clip, string oldId, string newId);
        public delegate void ResourceModifiedEventHandler(VideoClipModel clip, T resource, string property);
        public delegate void ResourceEventHandler(VideoClipModel clip, T resource);

        private string resourceId;
        private bool? isResourceOnline;

        public string ResourceId {
            get => this.resourceId;
            set {
                this.resourceId = value;
                this.IsResourceOnline = null;
            }
        }

        /// <summary>
        /// Indicates if this clip's resource is available or not. Null means "don't know", true
        /// means it is was online the last time it was checked, and false means offline
        /// </summary>
        public bool? IsResourceOnline {
            get => this.isResourceOnline;
            set {
                this.isResourceOnline = value;
                this.OnResourceOnlineChanged();
            }
        }

        private T cachedItem;

        public event ResourceStateChangedEventHandler ResourceOnlineChanged;
        public event ResourceIdChangedEventHandler ResourceRenamed;
        public event ResourceModifiedEventHandler DataModified;
        public event ResourceEventHandler ResourceRemoved;

        protected BaseResourceVideoClip() {

        }

        protected virtual void OnResourceOnlineChanged() {
            this.ResourceOnlineChanged?.Invoke(this);
        }

        public bool TryGetResource(out T resource) {
            if (this.IsResourceOnline == false) {
                resource = default;
                return false;
            }

            if (this.cachedItem != null) {
                resource = this.cachedItem;
                return true;
            }

            if (this.Layer == null || string.IsNullOrWhiteSpace(this.ResourceId)) {
                resource = null;
                return false;
            }

            // Realistically, this code shouldn't be run, because when the resource manager and clip are all loaded, there
            // should be a function that runs to detect missing resource ids, and offer to replace them or just offline the clip
            // And eventually a "removed" event should be created
            ResourceManager manager = this.Layer.Timeline.Project.ResourceManager;
            if (!manager.TryGetResource(this.ResourceId, out ResourceItem resItem) || !(resItem is T item)) {
                this.IsResourceOnline = false;
                resource = default;
                return false;
            }

            this.cachedItem = resource = item;
            item.DataModified += this.OnResourceModifiedInternal;
            manager.ResourceRemoved += this.OnResourceRemovedInternal;
            manager.ResourceRenamed += this.OnResourceRenamedInternal;
            this.IsResourceOnline = true;
            return true;
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            if (data.TryGetString(nameof(this.ResourceId), out string id)) {
                this.ResourceId = id;
            }
        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            if (!string.IsNullOrEmpty(this.ResourceId))
                data.SetString(nameof(this.ResourceId), this.ResourceId);
        }

        private void OnResourceRemovedInternal(ResourceManager man, ResourceItem res) {
            if (this.cachedItem != null && this.cachedItem == res) {
                try {
                    this.OnResourceRemoved(this.cachedItem);
                }
                finally {
                    this.cachedItem = null;
                    res.DataModified -= this.OnResourceModifiedInternal;
                    man.ResourceRemoved -= this.OnResourceRemovedInternal;
                }

                this.IsResourceOnline = null;
            }
        }

        private void OnResourceRenamedInternal(ResourceManager man, ResourceItem res, string a, string b) {
            if (this.cachedItem != null && this.cachedItem == res) {
                this.OnResourceRenamed(a, b);
            }
        }

        private void OnResourceModifiedInternal(ResourceItem sender, string property) {
            if (this.cachedItem == null) {
                sender.DataModified -= this.OnResourceModifiedInternal;
                Debug.WriteLine($"Warning! Item DataModified event was not removed: {sender.UniqueId}");
            }
            else if (sender != this.cachedItem) {
                Debug.WriteLine($"Warning! Cached item and sender do not match: {this.cachedItem} != {sender} ({this.ResourceId} != {sender.UniqueId})");
            }
            else {
                this.OnResourceModified(this.cachedItem, property);
            }
        }

        protected virtual void OnResourceModified(T resource, string property) {
            this.DataModified?.Invoke(this, resource, property);
        }

        protected virtual void OnResourceRemoved(T resource) {
            Debug.WriteLine($"Resource removed: {resource.UniqueId}");
            this.ResourceRemoved?.Invoke(this, resource);
        }

        protected virtual void OnResourceRenamed(string oldId, string newId) {
            Debug.WriteLine($"Resource renamed: {oldId} -> {newId}");
            this.ResourceId = newId;
            this.ResourceRenamed?.Invoke(this, oldId, newId);
        }
    }
}