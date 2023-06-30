using System;
using System.Collections.Generic;
using System.Diagnostics;
using FramePFX.Core.Editor.ResourceManaging;
using FramePFX.Core.Editor.ResourceManaging.Events;
using FramePFX.Core.Editor.Timelines.VideoClips;
using FramePFX.Core.RBC;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.Timelines {
    /// <summary>
    /// A base video clip that can reference
    /// </summary>
    public abstract class BaseMultiResourceClip : VideoClip {
        public delegate void ClipResourceModifiedEventHandler(string key, ResourceItem resource, string property);
        public delegate void ClipResourceChangedEventHandler(string key, ResourceItem oldItem, ResourceItem newItem);

        private readonly Dictionary<string, ResourcePathEntry> ResourceMap;

        public event ClipResourceChangedEventHandler ClipResourceChanged;
        public event ClipResourceModifiedEventHandler ClipResourceDataModified;
        public event ResourceItemEventHandler OnlineStateChanged;

        protected BaseMultiResourceClip() {
            this.ResourceMap = new Dictionary<string, ResourcePathEntry>();
        }

        protected void AddResourceKey(string key) {
            // throws for existing keys
            this.ResourceMap.Add(key, new ResourcePathEntry(this, key));
        }

        public void SetTargetResourceId(string key, string id) {
            this.ResourceMap[key].SetTargetResourceId(id, this.ResourceManager);
        }

        protected override void OnTrackChanged(Track oldTrack, Track newTrack) {
            base.OnTrackChanged(oldTrack, newTrack);
            ResourceManager manager = newTrack?.Timeline.Project.ResourceManager;
            using (ExceptionStack stack = new ExceptionStack()) {
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

        protected virtual void OnOnlineStateChanged(ResourceItem item) {
            this.InvalidateRender();
        }

        protected virtual void OnResourceChanged(string key, ResourceItem oldItem, ResourceItem newItem) {
            this.InvalidateRender();
        }

        protected virtual void OnResourceDataModified(string key, string property) {
            this.InvalidateRender();
        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            if (this.ResourceMap.Count > 0) {
                RBEDictionary resourceMapDictionary = data.CreateDictionary(nameof(this.ResourceMap));
                foreach (KeyValuePair<string, ResourcePathEntry> entry in this.ResourceMap) {
                    Debug.Assert(entry.Key == entry.Value.key, "Map pair key and entry key do not match");
                    ResourcePathEntry.WriteToRBE(entry.Value, resourceMapDictionary);
                }
            }
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            if (data.TryGetElement(nameof(this.ResourceMap), out RBEDictionary resourceMapDictionary)) {
                ResourceManager manager = this.ResourceManager;
                foreach (KeyValuePair<string, RBEBase> pair in resourceMapDictionary.Map) {
                    if (this.ResourceMap.TryGetValue(pair.Key, out ResourcePathEntry entry) && pair.Value is RBEDictionary dictionary) {
                        ResourcePathEntry.ReadFromRBE(entry, dictionary, manager);
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

        protected override void DisposeCore(ExceptionStack stack) {
            base.DisposeCore(stack);
            foreach (ResourcePathEntry entry in this.ResourceMap.Values) {
                entry.Dispose(stack);
            }
        }

        private class ResourcePathEntry {
            public readonly string key;
            private readonly BaseMultiResourceClip clip;
            private readonly ResourcePath.ResourceChangedEventHandler resourceChangedHandler;
            private readonly ResourceModifiedEventHandler dataModifiedHandler;
            private readonly ResourceItemEventHandler onlineStateChangedHandler;
            public ResourcePath path;

            public ResourcePathEntry(BaseMultiResourceClip clip, string key) {
                this.clip = clip ?? throw new ArgumentNullException();
                this.key = key ?? throw new ArgumentNullException();
                this.resourceChangedHandler = this.OnEntryResourceChangedInternal;
                this.dataModifiedHandler = this.OnEntryResourceDataModifiedInternal;
                this.onlineStateChangedHandler = this.OnEntryOnlineStateChangedInternal;
            }

            public void SetTargetResourceId(string id, ResourceManager manager) {
                if (this.path != null) {
                    this.path.ResourceChanged -= this.resourceChangedHandler;
                    this.path.Dispose();
                    this.path = null; // just in case the code below throws, don't reference a disposed instance
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

                this.clip.OnResourceChanged(this.key, oldItem, newItem);
                this.clip.ClipResourceChanged?.Invoke(this.key, oldItem, newItem);
            }

            private void OnEntryResourceDataModifiedInternal(ResourceItem sender, string property) {
                if (this.path == null)
                    throw new InvalidOperationException("Expected resource path to be non-null");
                if (!this.path.IsCachedItemEqualTo(sender))
                    throw new InvalidOperationException("Received data modified event for a resource that does not equal the resource path's item");
                this.clip.OnResourceDataModified(this.key, property);
                this.clip.ClipResourceDataModified?.Invoke(this.key, sender, property);
            }

            private void OnEntryOnlineStateChangedInternal(ResourceManager manager, ResourceItem item) {
                if (!this.path.IsCachedItemEqualTo(item))
                    throw new InvalidOperationException("Received data modified event for a resource that does not equal the resource path's item");
                this.clip.OnOnlineStateChanged(item);
                this.clip.OnlineStateChanged?.Invoke(manager, item);
            }

            public static void WriteToRBE(ResourcePathEntry entry, RBEDictionary resourceMapDictionary) {
                if (entry.path == null) {
                    return;
                }

                RBEDictionary dictionary = resourceMapDictionary.CreateDictionary(entry.key);
                ResourcePath.WriteToRBE(entry.path, dictionary);
            }

            public static void ReadFromRBE(ResourcePathEntry entry, RBEDictionary dictionary, ResourceManager manager) {
                if (entry.path != null && entry.path.CanDispose) { // handle this, just in case...
                    entry.path.Dispose();
                }

                entry.path = ResourcePath.ReadFromRBE(manager, dictionary);
            }

            public void Dispose(ExceptionStack stack) {
                if (this.path != null && this.path.CanDispose) {
                    // this shouldn't throw unless it was already disposed for some reason. Might as well handle that case
                    this.path?.Dispose();
                }
            }
        }
    }
}