using System;
using FramePFX.Editor.ResourceManaging;
using FramePFX.Editor.ResourceManaging.Events;
using FramePFX.Editor.Timelines.VideoClips;
using FramePFX.RBC;

namespace FramePFX.Editor.Timelines.ResourceHelpers {
    /// <summary>
    /// A class that helps with managing a single resource object for use by a clip
    /// </summary>
    public class ResourceHelper<T> : BaseResourceHelper where T : ResourceItem {
        public delegate void ClipResourceModifiedEventHandler(T resource, string property);
        public delegate void ClipResourceChangedEventHandler(T oldItem, T newItem);

        private readonly ResourcePath<T>.ResourceChangedEventHandler resourceChangedHandler;
        private readonly ResourceModifiedEventHandler dataModifiedHandler;
        private readonly ResourceItemEventHandler onlineStateChangedHandler;

        public ResourcePath<T> ResourcePath { get; private set; }

        /// <summary>
        /// An event fired when the underlying resource being used has changed
        /// </summary>
        public event ClipResourceChangedEventHandler ResourceChanged;

        /// <summary>
        /// An event fired when the underlying resource raises a <see cref="ResourceItem.DataModified"/> event
        /// </summary>
        public event ClipResourceModifiedEventHandler ResourceDataModified;

        /// <summary>
        /// The clip that owns this helper
        /// </summary>
        public IResourceClip<T> Clip { get; }

        /// <summary>
        /// Whether or not this helper has a valid path
        /// </summary>
        public bool HasPath => this.ResourcePath != null;

        // this class is automatically disposed by the base clip class which uses
        // the dirty OOP trick of 'if (this is BaseResourceHelper)' to dispose it
        // the only thing that must be manually handled is LoadDataIntoClone

        public ResourceHelper(IResourceClip<T> clip) {
            this.Clip = clip ?? throw new ArgumentNullException(nameof(clip));
            this.resourceChangedHandler = this.OnResourceChangedInternal;
            this.dataModifiedHandler = this.OnResourceDataModifiedInternal;
            this.onlineStateChangedHandler = this.OnOnlineStateChangedInternal;
            clip.TrackChanged += this.OnTrackChanged;
            clip.TrackTimelineChanged += this.OnTrackTimelineChanged;
            clip.SerialiseExtension += (c, data) => this.WriteToRBE(data);
            clip.DeserialiseExtension += (c, data) => this.ReadFromRBE(data);
        }

        private void DisposePath() {
            ResourcePath<T> path = this.ResourcePath;
            this.ResourcePath = null; // just in case the code below throws, don't reference a disposed instance
            if (path != null) {
                try {
                    path.Dispose();
                }
                finally {
                    path.ResourceChanged -= this.resourceChangedHandler;
                }
            }
        }

        private void OnTrackChanged(Track oldTrack, Track newTrack) {
            if (this.ResourcePath == null)
                return;
            this.UpdateManager(newTrack?.Timeline?.Project?.ResourceManager);
        }

        private void OnTrackTimelineChanged(Timeline oldTimeline, Timeline timeline) {
            this.UpdateManager(timeline?.Project?.ResourceManager);
        }

        private void UpdateManager(ResourceManager manager) {
            if (this.ResourcePath == null)
                return;
            if (manager != this.ResourcePath.Manager) {
                this.ResourcePath.SetManager(manager);
            }
        }

        /// <summary>
        /// Sets this <see cref="ResourceHelper{T}"/>'s target resource ID. The previous <see cref="ResourcePath{T}"/> is
        /// disposed and replace with a new instance using the same <see cref="ResourceManager"/>
        /// </summary>
        /// <param name="id">The target resource ID</param>
        public void SetTargetResourceId(ulong id) {
            if (id == 0) {
                throw new ArgumentOutOfRangeException(nameof(id), "ID must not be the null value (0)");
            }

            if (this.ResourcePath != null && this.ResourcePath.ResourceId == id) {
                return;
            }

            this.DisposePath();
            this.ResourcePath = new ResourcePath<T>(this.Clip.Project?.ResourceManager, id);
            this.ResourcePath.ResourceChanged += this.resourceChangedHandler;
        }

        private void OnResourceChangedInternal(T oldItem, T newItem) {
            if (oldItem != null) {
                oldItem.OnlineStateChanged -= this.onlineStateChangedHandler;
                oldItem.DataModified -= this.dataModifiedHandler;
            }

            if (newItem != null) {
                newItem.OnlineStateChanged += this.onlineStateChangedHandler;
                newItem.DataModified += this.dataModifiedHandler;
            }

            this.ResourceChanged?.Invoke(oldItem, newItem);
            this.TriggerClipRender();
        }

        private void OnResourceDataModifiedInternal(ResourceItem sender, string property) {
            if (this.ResourcePath == null)
                throw new InvalidOperationException("Expected resource path to be non-null");
            if (!this.ResourcePath.IsCachedItemEqualTo(sender))
                throw new InvalidOperationException("Received data modified event for a resource that does not equal the resource path's item");
            this.ResourceDataModified?.Invoke((T) sender, property);
            this.TriggerClipRender();
        }

        private void OnOnlineStateChangedInternal(ResourceManager manager, ResourceItem item) {
            if (!this.ResourcePath.IsCachedItemEqualTo(item))
                throw new InvalidOperationException("Received data modified event for a resource that does not equal the resource path's item");
            this.OnOnlineStateChanged(manager, item);
            this.TriggerClipRender();
        }

        public void TriggerClipRender() {
            if (this.Clip is VideoClip clip) {
                clip.InvalidateRender();
            }
        }

        public void WriteToRBE(RBEDictionary data) {
            if (this.ResourcePath != null) {
                ResourcePath<T>.WriteToRBE(this.ResourcePath, data.CreateDictionary(nameof(this.ResourcePath)));
            }
        }

        public void ReadFromRBE(RBEDictionary data) {
            if (data.TryGetElement(nameof(this.ResourcePath), out RBEDictionary resource)) {
                this.ResourcePath = ResourcePath<T>.ReadFromRBE(this.Clip.Project?.ResourceManager, resource);
            }
        }

        public bool TryGetResource(out T resource) {
            if (this.ResourcePath != null) {
                return this.ResourcePath.TryGetResource(out resource);
            }

            resource = null;
            return false;
        }

        public override void Dispose() {
            if (this.ResourcePath != null && !this.ResourcePath.IsDisposed) {
                this.DisposePath();
            }
        }

        public void LoadDataIntoClone(ResourceHelper<T> helper) {
            if (this.ResourcePath != null) {
                helper.SetTargetResourceId(this.ResourcePath.ResourceId);
            }
        }
    }
}