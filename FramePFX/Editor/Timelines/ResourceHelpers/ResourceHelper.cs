using System;
using System.Collections.Generic;
using System.Reflection;
using FramePFX.Editor.ResourceManaging;
using FramePFX.Editor.ResourceManaging.Events;
using FramePFX.Editor.Timelines.VideoClips;
using FramePFX.RBC;
using FramePFX.Utils;

namespace FramePFX.Editor.Timelines.ResourceHelpers
{
    /// <summary>
    /// A helper class that manages resource manager events automatically
    /// <para>
    /// This class only exists so that clips don't have to extend 'BaseResourceVideoClip', 'BaseResourceAudioClip',
    /// etc., and instead they can inherit from <see cref="IResourceClip"/>. This allows the same functionality
    /// to be reused for video clips, audio clips, etc.
    /// </para>
    /// </summary>
    public class ResourceHelper
    {
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
        public event EntryResourceOnlineStateChangedEventHandler OnlineStateChanged;

        /// <summary>
        /// The clip that owns this helper
        /// </summary>
        public IResourceClip Clip { get; }

        public ResourceHelper(IResourceClip clip)
        {
            this.Clip = clip ?? throw new ArgumentNullException(nameof(clip));
            this.ResourceMap = new Dictionary<string, BaseResourcePathEntry>();
            clip.TrackChanged += this.OnTrackChanged;
            clip.TrackTimelineChanged += this.OnTrackTimelineChanged;
            clip.TrackTimelineProjectChanged += this.OnTrackTimelineProjectChanged;
            clip.SerialiseExtension += (c, data) => this.WriteToRBE(data);
            clip.DeserialiseExtension += (c, data) => this.ReadFromRBE(data);
        }

        private static string KeyForTypeName(Type type) => type.Name;

        /// <summary>
        /// Registers a key for usage by this <see cref="ResourceHelper"/>
        /// </summary>
        /// <param name="key">The key to use</param>
        /// <returns>An interface which wraps the internal key entry object</returns>
        /// <exception cref="InvalidOperationException">The key is already in use</exception>
        public IResourcePathKey<T> RegisterKey<T>(string key) where T : ResourceItem
        {
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
        public bool TryGetResource<T>(string key, out T resource) where T : ResourceItem
        {
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

        public void SetTargetResourceId(string key, ulong id)
        {
            this.ResourceMap[key].SetTargetResourceId(id);
        }

        public void SetTargetResourceIdByTypeName<T>(ulong id) where T : ResourceItem => this.SetTargetResourceId(KeyForTypeName(typeof(T)), id);

        private void OnTrackChanged(Track oldTrack, Track newTrack)
        {
            this.SetManager(newTrack?.Timeline?.Project?.ResourceManager);
        }

        private void OnTrackTimelineChanged(Timeline oldTimeline, Timeline timeline)
        {
            this.SetManager(timeline?.Project?.ResourceManager);
        }

        private void OnTrackTimelineProjectChanged(Project oldproject, Project newproject)
        {
            this.SetManager(newproject?.ResourceManager);
        }

        private void SetManager(ResourceManager manager)
        {
            using (ErrorList stack = new ErrorList())
            {
                foreach (BaseResourcePathEntry entry in this.ResourceMap.Values)
                {
                    ResourcePath path = entry.path;
                    if (path == null || ReferenceEquals(path.Manager, manager))
                    {
                        continue;
                    }

                    try
                    {
                        path.SetManager(manager);
                    }
                    catch (Exception e)
                    {
                        stack.Add(e);
                    }
                }
            }
        }

        private void OnResourceChanged(BaseResourcePathEntry key, ResourceItem oldItem, ResourceItem newItem)
        {
            this.ResourceChanged?.Invoke(this, new ResourceChangedEventArgs(key, oldItem, newItem));
            this.TriggerClipRender();
        }

        private void OnResourceDataModified(IBaseResourcePathKey key, ResourceItem sender, string property)
        {
            this.ResourceDataModified?.Invoke(this, new ResourceModifiedEventArgs(key, sender, property));
            this.TriggerClipRender();
        }

        private void OnOnlineStateChanged(ResourceItem item)
        {
            this.OnlineStateChanged?.Invoke(this, item);
            this.TriggerClipRender();
        }

        private void TriggerClipRender()
        {
            if (this.Clip is VideoClip clip)
            {
                clip.InvalidateRender();
            }
        }

        public void WriteToRBE(RBEDictionary data)
        {
            if (this.ResourceMap.Count > 0)
            {
                RBEDictionary resourceMapDictionary = data.CreateDictionary(nameof(this.ResourceMap));
                foreach (KeyValuePair<string, BaseResourcePathEntry> entry in this.ResourceMap)
                {
                    ExceptionUtils.Assert(entry.Key == entry.Value.entryKey, "Map pair key and entry key do not match");
                    BaseResourcePathEntry.WriteToRBE(entry.Value, resourceMapDictionary);
                }
            }
        }

        public void ReadFromRBE(RBEDictionary data)
        {
            if (data.TryGetElement(nameof(this.ResourceMap), out RBEDictionary resourceMapDictionary))
            {
                foreach (KeyValuePair<string, RBEBase> pair in resourceMapDictionary.Map)
                {
                    if (this.ResourceMap.TryGetValue(pair.Key, out BaseResourcePathEntry entry) && pair.Value is RBEDictionary dictionary)
                    {
                        BaseResourcePathEntry.ReadFromRBE(entry, dictionary);
                    }
                }
            }
        }

        public void Dispose()
        {
            foreach (BaseResourcePathEntry entry in this.ResourceMap.Values)
                entry.DisposePath();
        }

        public void LoadDataIntoClone(ResourceHelper clone)
        {
            foreach (KeyValuePair<string, BaseResourcePathEntry> pair in this.ResourceMap)
            {
                ResourcePath path = pair.Value.path;
                if (path != null)
                    clone.ResourceMap[pair.Key].SetTargetResourceId(path.ResourceId);
            }
        }

        private abstract class BaseResourcePathEntry : IBaseResourcePathKey
        {
            private readonly ResourceHelper helper;
            private readonly ResourceChangedEventHandler resourceChangedHandler;
            private readonly ResourceModifiedEventHandler dataModifiedHandler;
            private readonly ResourceAndManagerEventHandler onlineStateChangedHandler;
            public readonly string entryKey;
            public ResourcePath path;

            ResourceHelper IBaseResourcePathKey.Helper => this.helper;
            ResourcePath IBaseResourcePathKey.Path => this.path;
            string IBaseResourcePathKey.Key => this.entryKey;

            protected BaseResourcePathEntry(ResourceHelper clip, string entryKey)
            {
                this.helper = clip ?? throw new ArgumentNullException(nameof(clip));
                this.entryKey = string.IsNullOrEmpty(entryKey) ? throw new ArgumentException("Entry id cannot be null or empty", nameof(entryKey)) : entryKey;
                this.resourceChangedHandler = this.OnEntryResourceChangedInternal;
                this.dataModifiedHandler = this.OnEntryResourceDataModifiedInternal;
                this.onlineStateChangedHandler = this.OnEntryOnlineStateChangedInternal;
            }

            public virtual void SetTargetResourceId(ulong id)
            {
                if (id == ResourceManager.EmptyId)
                    throw new ArgumentException("ID must not be empty (0)");

                ResourcePath oldPath = this.path;
                if (oldPath != null)
                {
                    this.path = null; // just in case the code below throws, don't reference a disposed instance
                    oldPath.Dispose();
                    oldPath.ResourceChanged -= this.resourceChangedHandler;
                }

                this.path = new ResourcePath(this, this.helper.Clip.Project?.ResourceManager, id);
                this.path.ResourceChanged += this.resourceChangedHandler;
            }

            public bool TryGetResource<T>(out T resource, bool requireIsOnline = true) where T : ResourceItem
            {
                if (this.path != null)
                    return this.path.TryGetResource(out resource, requireIsOnline);
                resource = null;
                return false;
            }

            public abstract bool IsItemTypeApplicable(ResourceItem item);

            protected abstract void OnEntryResourceChanged(ResourceItem oldItem, ResourceItem newItem);

            private void OnEntryResourceChangedInternal(ResourceItem oldItem, ResourceItem newItem)
            {
                if (oldItem != null)
                {
                    oldItem.OnlineStateChanged -= this.onlineStateChangedHandler;
                    oldItem.DataModified -= this.dataModifiedHandler;
                }

                if (newItem != null && this.IsItemTypeApplicable(newItem))
                {
                    newItem.OnlineStateChanged += this.onlineStateChangedHandler;
                    newItem.DataModified += this.dataModifiedHandler;
                    this.OnEntryResourceChanged(oldItem, newItem);
                    this.helper.OnResourceChanged(this, oldItem, newItem);
                }
                else
                {
                    this.OnEntryResourceChanged(oldItem, null);
                    this.helper.OnResourceChanged(this, oldItem, null);
                }
            }

            protected abstract void OnEntryResourceDataModified(ResourceItem sender, string property);

            private void OnEntryResourceDataModifiedInternal(ResourceItem sender, string property)
            {
                if (this.path == null)
                    throw new InvalidOperationException("Expected resource path to be non-null");
                if (!this.path.IsCachedItemEqualTo(sender))
                    throw new InvalidOperationException("Received data modified event for a resource that does not equal the resource path's item");
                this.OnEntryResourceDataModified(sender, property);
                this.helper.OnResourceDataModified(this, sender, property);
            }

            protected abstract void OnEntryOnlineStateChanged(ResourceItem item);

            private void OnEntryOnlineStateChangedInternal(ResourceManager manager, ResourceItem item)
            {
                if (!this.path.IsCachedItemEqualTo(item))
                    throw new InvalidOperationException("Received data modified event for a resource that does not equal the resource path's item");
                this.OnEntryOnlineStateChanged(item);
                this.helper.OnOnlineStateChanged(item);
            }

            public static void WriteToRBE(BaseResourcePathEntry entry, RBEDictionary resourceMapDictionary)
            {
                if (entry.path == null)
                    return;
                ResourcePath.WriteToRBE(entry.path, resourceMapDictionary.CreateDictionary(entry.entryKey));
            }

            public static void ReadFromRBE(BaseResourcePathEntry entry, RBEDictionary dictionary)
            {
                // handle this, just in case...
                if (entry.path != null && entry.path.CanDispose)
                {
                    entry.DisposePath();
                }

                entry.path = ResourcePath.ReadFromRBE(entry, dictionary);
            }

            public void DisposePath()
            {
                if (Helper.Exchange(ref this.path, null, out ResourcePath oldPath) && oldPath.CanDispose)
                {
                    oldPath.Dispose();
                    oldPath.ResourceChanged -= this.resourceChangedHandler;
                }
            }
        }

        private class ResourcePathEntry<T> : BaseResourcePathEntry, IResourcePathKey<T> where T : ResourceItem
        {
            public event EntryResourceChangedEventHandler<T> ResourceChanged;
            public event EntryResourceModifiedEventHandler<T> ResourceDataModified;
            public event EntryResourceOnlineStateChangedEventHandler<T> OnlineStateChanged;

            public ResourcePathEntry(ResourceHelper clip, string entryKey) : base(clip, entryKey)
            {
            }

            public bool TryGetResource(out T resource, bool requireIsOnline = true) => base.TryGetResource(out resource, requireIsOnline);

            public override void SetTargetResourceId(ulong id)
            {
                base.SetTargetResourceId(id);
                // base.TryGetResource(out T _, false);
            }

            public override bool IsItemTypeApplicable(ResourceItem item) => item is T;

            protected override void OnEntryResourceChanged(ResourceItem oldItem, ResourceItem newItem)
            {
                this.ResourceChanged?.Invoke((T) oldItem, (T) newItem);
            }

            protected override void OnEntryResourceDataModified(ResourceItem sender, string property)
            {
                this.ResourceDataModified?.Invoke((T) sender, property);
            }

            protected override void OnEntryOnlineStateChanged(ResourceItem item)
            {
                this.OnlineStateChanged?.Invoke((T) item);
            }
        }
    }
}