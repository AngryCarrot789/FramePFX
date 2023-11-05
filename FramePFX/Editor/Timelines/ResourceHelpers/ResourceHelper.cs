using System;
using System.Collections.Generic;
using System.Reflection;
using FramePFX.Editor.ResourceManaging;
using FramePFX.Editor.ResourceManaging.Events;
using FramePFX.Editor.Timelines.VideoClips;
using FramePFX.RBC;
using FramePFX.Utils;

namespace FramePFX.Editor.Timelines.ResourceHelpers {
    /// <summary>
    /// A helper class that manages resource manager events automatically
    /// <para>
    /// This class only exists so that clips don't have to extend 'BaseResourceVideoClip', 'BaseResourceAudioClip',
    /// etc., and instead they can inherit from <see cref="IResourceHolder"/>. This allows the same functionality
    /// to be reused for video clips, audio clips, etc.
    /// </para>
    /// </summary>
    public class ResourceHelper {
        private readonly Dictionary<string, BaseResourcePathEntry> ResourceMap;

        /// <summary>
        /// An event fired when the underlying resource being used has changed
        /// </summary>
        public event EntryResourceChangedEventHandler ResourceChanged;

        /// <summary>
        /// An event fired when the underlying resource raises a <see cref="ResourceItem.DataModified"/> event
        /// </summary>
        public event EntryResourceModifiedEventHandler ResourceDataModified;

        /// <summary>
        /// An event fired when the online state of a resource changes (e.g. user set it to offline or online)
        /// </summary>
        public event EntryOnlineStateChangedEventHandler OnlineStateChanged;

        /// <summary>
        /// The resource holder object that owns this helper. This is typically an object that extends <see cref="Clip"/>
        /// </summary>
        public IResourceHolder Owner { get; }

        /// <summary>
        /// Returns an unordered enumerable of this resource helper's registered keys
        /// </summary>
        public IEnumerable<IBaseResourcePathKey> RegisteredKeys => this.ResourceMap.Values;

        public ResourceHelper(IResourceHolder clip) {
            this.Owner = clip ?? throw new ArgumentNullException(nameof(clip));
            this.ResourceMap = new Dictionary<string, BaseResourcePathEntry>();
        }

        private static string KeyForTypeName(Type type) => type.Name;

        /// <summary>
        /// Registers a key for usage by this <see cref="ResourceHelper"/>
        /// </summary>
        /// <param name="key">The key to use</param>
        /// <returns>An interface which wraps the internal key entry object</returns>
        /// <exception cref="InvalidOperationException">The key is already in use</exception>
        public IResourcePathKey<T> RegisterKey<T>(string key) where T : ResourceItem {
            if (this.ResourceMap.ContainsKey(key))
                throw new InvalidOperationException("Key already registered: " + key);
            ResourcePathEntry<T> entry = new ResourcePathEntry<T>(this, key);
            this.ResourceMap[key] = entry;
            return entry;
        }

        /// <summary>
        /// Registers the type name of the given type (passed to <see cref="RegisterKey"/>)
        /// </summary>
        /// <typeparam name="T">The type of object to register</typeparam>
        /// <returns>An interface which wraps the internal key entry object</returns>
        /// <exception cref="InvalidOperationException">The key is already in use</exception>
        public IResourcePathKey<T> RegisterKeyByTypeName<T>() where T : ResourceItem => this.RegisterKey<T>(KeyForTypeName(typeof(T)));

        /// <summary>
        /// Tries to get a resource, of the given type, by the given name. Returns true if an entry exists for the key
        /// and the resource path is valid and its ID maps to the correct type of resource. Otherwise, returns false
        /// </summary>
        /// <param name="key">The key for the entry</param>
        /// <param name="resource">The resource found</param>
        /// <typeparam name="T">The type of resource to try to get</typeparam>
        /// <returns>See summary</returns>
        public bool TryGetResource<T>(string key, out T resource) where T : ResourceItem {
            if (this.ResourceMap.TryGetValue(key, out BaseResourcePathEntry entry) && entry.TryGetResource(out resource))
                return true;
            resource = null;
            return false;
        }

        /// <summary>
        /// Tries to get a resource, using the <see cref="MemberInfo.Name"/> property as a key when calling <see cref="TryGetResource{T}"/>
        /// </summary>
        /// <param name="resource">The resource found</param>
        /// <typeparam name="T">The type of resource</typeparam>
        /// <returns></returns>
        public bool TryGetResourceByTypeName<T>(out T resource) where T : ResourceItem => this.TryGetResource(KeyForTypeName(typeof(T)), out resource);

        public void SetTargetResourceId(string key, ulong id) {
            this.ResourceMap[key].SetTargetResourceId(id);
        }

        public void SetTargetResourceIdByTypeName<T>(ulong id) where T : ResourceItem => this.SetTargetResourceId(KeyForTypeName(typeof(T)), id);

        /// <summary>
        /// Sets the manager for all <see cref="IBaseResourcePathKey"/> entries in this helper
        /// </summary>
        /// <param name="manager"></param>
        public void SetManager(ResourceManager manager) {
            foreach (BaseResourcePathEntry entry in this.ResourceMap.Values) {
                ResourcePath path = entry.path;
                if (path == null || ReferenceEquals(path.Manager, manager)) {
                    continue;
                }

                path.SetManager(manager);
            }
        }

        private void OnResourceChanged(BaseResourcePathEntry key, ResourceItem oldItem, ResourceItem newItem) {
            this.ResourceChanged?.Invoke(this, new ResourceChangedEventArgs(key, oldItem, newItem));
            this.TriggerClipRender();
        }

        private void OnResourceDataModified(IBaseResourcePathKey key, ResourceItem sender, string property) {
            this.ResourceDataModified?.Invoke(this, new ResourceModifiedEventArgs(key, sender, property));
            this.TriggerClipRender();
        }

        private void OnOnlineStateChanged(IBaseResourcePathKey key) {
            this.OnlineStateChanged?.Invoke(this, key);
            this.TriggerClipRender();
        }

        private void TriggerClipRender() {
            if (this.Owner is VideoClip clip && clip.Project != null)
                clip.InvalidateRender();
        }

        public void WriteToRootRBE(RBEDictionary data) {
            if (this.ResourceMap.Count > 0) {
                RBEDictionary resourceMapDictionary = data.CreateDictionary(nameof(this.ResourceMap));
                foreach (KeyValuePair<string, BaseResourcePathEntry> entry in this.ResourceMap) {
                    ExceptionUtils.Assert(entry.Key == entry.Value.entryKey, "Map pair key and entry key do not match");
                    BaseResourcePathEntry.WriteToRBE(entry.Value, resourceMapDictionary);
                }
            }
        }

        public void ReadFromRootRBE(RBEDictionary data) {
            if (data.TryGetElement(nameof(this.ResourceMap), out RBEDictionary resourceMapDictionary)) {
                foreach (KeyValuePair<string, RBEBase> pair in resourceMapDictionary.Map) {
                    if (this.ResourceMap.TryGetValue(pair.Key, out BaseResourcePathEntry entry) && pair.Value is RBEDictionary dictionary) {
                        BaseResourcePathEntry.ReadFromRBE(entry, dictionary);
                    }
                }
            }
        }

        public void Dispose() {
            foreach (BaseResourcePathEntry entry in this.ResourceMap.Values) {
                entry.DisposePath();
            }
        }

        public void LoadDataIntoClone(ResourceHelper clone) {
            foreach (KeyValuePair<string, BaseResourcePathEntry> pair in this.ResourceMap) {
                ResourcePath path = pair.Value.path;
                if (path != null)
                    clone.ResourceMap[pair.Key].SetTargetResourceId(path.ResourceId);
            }
        }

        private abstract class BaseResourcePathEntry : IBaseResourcePathKey {
            private readonly ResourceHelper helper;
            private readonly ResourceChangedEventHandler resourceChangedHandler;
            private readonly ResourceModifiedEventHandler dataModifiedHandler;
            private readonly ResourceAndManagerEventHandler onlineStateChangedHandler;
            public readonly string entryKey;
            public ResourcePath path;

            ResourceHelper IResourceHolder.ResourceHelper => this.helper;
            ResourcePath IBaseResourcePathKey.Path => this.path;
            string IBaseResourcePathKey.Key => this.entryKey;

            public bool IsLinked {
                get {
                    bool? v = this.path?.IsLinked;
                    return v.HasValue && v.Value;
                }
            }

            public Project Project => this.helper.Owner.Project;

            protected BaseResourcePathEntry(ResourceHelper helper, string entryKey) {
                this.helper = helper ?? throw new ArgumentNullException(nameof(helper));
                this.entryKey = string.IsNullOrEmpty(entryKey) ? throw new ArgumentException("Entry id cannot be null or empty", nameof(entryKey)) : entryKey;
                this.resourceChangedHandler = this.OnEntryResourceChangedInternal;
                this.dataModifiedHandler = this.OnEntryResourceDataModifiedInternal;
                this.onlineStateChangedHandler = this.OnResourceOnlineStateChangedInternal;
            }

            private void SetResourcePath(ResourcePath newPath) {
                if (this.path != null) {
                    if (this.path.CanDispose) {
                        this.path.Dispose();
                    }

                    this.path.ResourceChanged -= this.resourceChangedHandler;
                    this.path = null;
                }

                this.path = newPath;
                if (newPath != null) {
                    newPath.ResourceChanged += this.resourceChangedHandler;
                }

                this.OnOnlineStateChanged();
                this.helper.OnOnlineStateChanged(this);
            }

            public virtual void SetTargetResourceId(ulong id) {
                if (id == ResourceManager.EmptyId)
                    throw new ArgumentException("ID must not be empty (0)");
                this.SetResourcePath(new ResourcePath(this, this.helper.Owner.Project?.ResourceManager, id));
            }

            public bool TryGetResource<T>(out T resource, bool requireIsOnline = true) where T : ResourceItem {
                if (this.path != null)
                    return this.path.TryGetResource(out resource, requireIsOnline);
                resource = null;
                return false;
            }

            public abstract bool IsItemTypeApplicable(ResourceItem item);

            protected abstract void OnEntryResourceChanged(ResourceItem oldItem, ResourceItem newItem);

            private void OnEntryResourceChangedInternal(ResourceItem oldItem, ResourceItem newItem) {
                if (oldItem != null) {
                    oldItem.OnlineStateChanged -= this.onlineStateChangedHandler;
                    oldItem.DataModified -= this.dataModifiedHandler;
                }

                if (newItem != null && this.IsItemTypeApplicable(newItem)) {
                    newItem.OnlineStateChanged += this.onlineStateChangedHandler;
                    newItem.DataModified += this.dataModifiedHandler;
                    this.OnEntryResourceChanged(oldItem, newItem);
                    this.helper.OnResourceChanged(this, oldItem, newItem);
                }
                else {
                    this.OnEntryResourceChanged(oldItem, null);
                    this.helper.OnResourceChanged(this, oldItem, null);
                }
            }

            protected abstract void OnEntryResourceDataModified(ResourceItem sender, string property);

            private void OnEntryResourceDataModifiedInternal(BaseResource sender, string property) {
                if (!(sender is ResourceItem item))
                    return;
                if (this.path == null)
                    throw new InvalidOperationException("Expected resource path to be non-null");
                if (!this.path.IsCachedItemEqualTo(item))
                    throw new InvalidOperationException("Received data modified event for a resource that does not equal the resource path's item");
                this.OnEntryResourceDataModified(item, property);
                this.helper.OnResourceDataModified(this, item, property);
            }

            protected abstract void OnOnlineStateChanged();

            private void OnResourceOnlineStateChangedInternal(ResourceManager manager, ResourceItem item) {
                if (!this.path.IsCachedItemEqualTo(item))
                    throw new InvalidOperationException("Received data modified event for a resource that does not equal the resource path's item");
                this.OnOnlineStateChanged();
                this.helper.OnOnlineStateChanged(this);
            }

            public static void WriteToRBE(BaseResourcePathEntry entry, RBEDictionary resourceMapDictionary) {
                if (entry.path == null)
                    return;
                ResourcePath.WriteToRBE(entry.path, resourceMapDictionary.CreateDictionary(entry.entryKey));
            }

            public static void ReadFromRBE(BaseResourcePathEntry entry, RBEDictionary dictionary) {
                entry.SetResourcePath(ResourcePath.ReadFromRBE(entry, dictionary));
            }

            public void DisposePath() => this.SetResourcePath(null);
        }

        private class ResourcePathEntry<T> : BaseResourcePathEntry, IResourcePathKey<T> where T : ResourceItem {
            public event EntryResourceChangedEventHandler<T> ResourceChanged;
            public event EntryResourceModifiedEventHandler<T> ResourceDataModified;
            public event EntryOnlineStateChangedEventHandler<T> OnlineStateChanged;

            public ResourcePathEntry(ResourceHelper helper, string entryKey) : base(helper, entryKey) {
            }

            public bool TryGetResource(out T resource, bool requireIsOnline = true) => base.TryGetResource(out resource, requireIsOnline);

            public override bool IsItemTypeApplicable(ResourceItem item) => item is T;

            protected override void OnEntryResourceChanged(ResourceItem oldItem, ResourceItem newItem) {
                this.ResourceChanged?.Invoke(this, (T) oldItem, (T) newItem);
            }

            protected override void OnEntryResourceDataModified(ResourceItem sender, string property) {
                this.ResourceDataModified?.Invoke(this, (T) sender, property);
            }

            protected override void OnOnlineStateChanged() {
                this.OnlineStateChanged?.Invoke(this);
            }
        }
    }
}