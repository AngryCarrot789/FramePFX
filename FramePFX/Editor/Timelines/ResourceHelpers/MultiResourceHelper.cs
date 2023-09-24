using System;
using System.Collections.Generic;
using FramePFX.Editor.ResourceManaging;
using FramePFX.Editor.ResourceManaging.Events;
using FramePFX.Editor.Timelines.VideoClips;
using FramePFX.RBC;
using FramePFX.Utils;

namespace FramePFX.Editor.Timelines.ResourceHelpers {
    public class MultiResourceHelper : BaseResourceHelper {
        public delegate void ClipResourceModifiedEventHandler(string key, ResourceItem resource, string property);

        public delegate void ClipResourceChangedEventHandler(string key, ResourceItem oldItem, ResourceItem newItem);

        private readonly Dictionary<string, ResourcePathEntry> ResourceMap;

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
        public IMultiResourceClip Clip { get; }

        public MultiResourceHelper(IMultiResourceClip clip) {
            this.Clip = clip ?? throw new ArgumentNullException(nameof(clip));
            this.ResourceMap = new Dictionary<string, ResourcePathEntry>();
            clip.TrackChanged += this.OnTrackChanged;
            clip.TrackTimelineChanged += this.OnTrackTimelineChanged;
            clip.TrackTimelineProjectChanged += this.OnTrackTimelineProjectChanged;
            clip.SerialiseExtension += (c, data) => this.WriteToRBE(data);
            clip.DeserialiseExtension += (c, data) => this.ReadFromRBE(data);
        }

        protected void AddResourceKey(string key) {
            // throws for existing keys
            this.ResourceMap.Add(key, new ResourcePathEntry(this, key));
        }

        public void SetTargetResourceId(string key, ulong id) {
            this.ResourceMap[key].SetTargetResourceId(id, this.Clip.Project?.ResourceManager);
        }

        private void OnTrackChanged(Track oldTrack, Track newTrack) {
            this.SetManager(newTrack?.Timeline?.Project?.ResourceManager);
        }

        private void OnTrackTimelineChanged(Timeline oldTimeline, Timeline timeline) {
            this.SetManager(timeline?.Project?.ResourceManager);
        }

        private void OnTrackTimelineProjectChanged(Project oldproject, Project newproject) {
            this.SetManager(newproject?.ResourceManager);
        }

        private void SetManager(ResourceManager manager) {
            using (ErrorList stack = new ErrorList()) {
                foreach (ResourcePathEntry entry in this.ResourceMap.Values) {
                    ResourcePath path = entry.path;
                    if (path == null || ReferenceEquals(path.Manager, manager)) {
                        continue;
                    }

                    try {
                        path.SetManager(manager);
                    }
                    catch (Exception e) {
                        stack.Add(e);
                    }
                }
            }
        }

        private void OnResourceChanged(string key, ResourceItem oldItem, ResourceItem newItem) {
            this.TriggerClipRender();
        }

        private void OnResourceDataModified(string key, string property) {
            this.TriggerClipRender();
        }

        protected override void OnOnlineStateChanged(ResourceManager manager, ResourceItem item) {
            base.OnOnlineStateChanged(manager, item);
            this.TriggerClipRender();
        }

        private void TriggerClipRender() {
            if (this.Clip is VideoClip clip) {
                clip.InvalidateRender();
            }
        }

        public void WriteToRBE(RBEDictionary data) {
            if (this.ResourceMap.Count > 0) {
                RBEDictionary resourceMapDictionary = data.CreateDictionary(nameof(this.ResourceMap));
                foreach (KeyValuePair<string, ResourcePathEntry> entry in this.ResourceMap) {
                    ExceptionUtils.Assert(entry.Key == entry.Value.entryKey, "Map pair key and entry key do not match");
                    ResourcePathEntry.WriteToRBE(entry.Value, resourceMapDictionary);
                }
            }
        }

        public void ReadFromRBE(RBEDictionary data) {
            if (data.TryGetElement(nameof(this.ResourceMap), out RBEDictionary resourceMapDictionary)) {
                foreach (KeyValuePair<string, RBEBase> pair in resourceMapDictionary.Map) {
                    if (this.ResourceMap.TryGetValue(pair.Key, out ResourcePathEntry entry) && pair.Value is RBEDictionary dictionary) {
                        ResourcePathEntry.ReadFromRBE(entry, dictionary);
                    }
                }
            }
        }

        public bool TryGetResource<T>(string key, out T resource) where T : ResourceItem {
            if (this.ResourceMap.TryGetValue(key, out ResourcePathEntry entry) && entry.path != null && entry.path.TryGetResource(out resource)) {
                return true;
            }

            resource = null;
            return false;
        }

        public override void Dispose() {
            base.Dispose();
            foreach (ResourcePathEntry entry in this.ResourceMap.Values) {
                entry.DisposePath();
            }
        }

        private class ResourcePathEntry {
            // unique keys that inheritors of MultiResourceHelper specify to identify a resource
            public readonly string entryKey;
            private readonly MultiResourceHelper helper;
            private readonly ResourcePath.ResourceChangedEventHandler resourceChangedHandler;
            private readonly ResourceModifiedEventHandler dataModifiedHandler;
            private readonly ResourceItemEventHandler onlineStateChangedHandler;
            public ResourcePath path;

            public ResourcePathEntry(MultiResourceHelper clip, string entryKey) {
                this.helper = clip ?? throw new ArgumentNullException(nameof(clip));
                this.entryKey = string.IsNullOrEmpty(entryKey) ? throw new ArgumentException("Entry id cannot be null or empty", nameof(entryKey)) : entryKey;
                this.resourceChangedHandler = this.OnEntryResourceChangedInternal;
                this.dataModifiedHandler = this.OnEntryResourceDataModifiedInternal;
                this.onlineStateChangedHandler = this.OnEntryOnlineStateChangedInternal;
            }

            public void SetTargetResourceId(ulong id, ResourceManager manager) {
                ResourcePath oldPath = this.path;
                if (oldPath != null) {
                    this.path = null; // just in case the code below throws, don't reference a disposed instance
                    oldPath.Dispose();
                    oldPath.ResourceChanged -= this.resourceChangedHandler;
                }

                this.path = new ResourcePath(manager, id);
                this.path.ResourceChanged += this.resourceChangedHandler;
            }

            private void OnEntryResourceChangedInternal(ResourceItem oldItem, ResourceItem newItem) {
                if (oldItem != null) {
                    oldItem.OnlineStateChanged -= this.onlineStateChangedHandler;
                    oldItem.DataModified -= this.dataModifiedHandler;
                }

                if (newItem != null) {
                    newItem.OnlineStateChanged += this.onlineStateChangedHandler;
                    newItem.DataModified += this.dataModifiedHandler;
                }

                this.helper.OnResourceChanged(this.entryKey, oldItem, newItem);
                this.helper.ResourceChanged?.Invoke(this.entryKey, oldItem, newItem);
            }

            private void OnEntryResourceDataModifiedInternal(ResourceItem sender, string property) {
                if (this.path == null)
                    throw new InvalidOperationException("Expected resource path to be non-null");
                if (!this.path.IsCachedItemEqualTo(sender))
                    throw new InvalidOperationException("Received data modified event for a resource that does not equal the resource path's item");
                this.helper.OnResourceDataModified(this.entryKey, property);
                this.helper.ResourceDataModified?.Invoke(this.entryKey, sender, property);
            }

            private void OnEntryOnlineStateChangedInternal(ResourceManager manager, ResourceItem item) {
                if (!this.path.IsCachedItemEqualTo(item))
                    throw new InvalidOperationException("Received data modified event for a resource that does not equal the resource path's item");
                this.helper.OnOnlineStateChanged(manager, item);
            }

            public static void WriteToRBE(ResourcePathEntry entry, RBEDictionary resourceMapDictionary) {
                if (entry.path == null)
                    return;
                ResourcePath.WriteToRBE(entry.path, resourceMapDictionary.CreateDictionary(entry.entryKey));
            }

            public static void ReadFromRBE(ResourcePathEntry entry, RBEDictionary dictionary) {
                if (entry.path != null && entry.path.CanDispose) {
                    // handle this, just in case...
                    entry.DisposePath();
                }

                entry.path = ResourcePath.ReadFromRBE(dictionary);
            }

            public void DisposePath() {
                if (Helper.Exchange(ref this.path, null, out ResourcePath myPath) && !myPath.IsDisposed) {
                    myPath.Dispose();
                    myPath.ResourceChanged -= this.resourceChangedHandler;
                }
            }
        }
    }
}