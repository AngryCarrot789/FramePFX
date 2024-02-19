//
// Copyright (c) 2023-2024 REghZy
//
// This file is part of FramePFX.
//
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
//
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using System;
using System.Collections.Generic;
using System.Reflection;
using FramePFX.Editors.ResourceManaging.Events;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.RBC;
using FramePFX.Utils;

namespace FramePFX.Editors.ResourceManaging.ResourceHelpers {
    /// <summary>
    /// A helper class that manages resource manager events automatically and stores a collection of resource keys
    /// <para>
    /// This class only exists so that clips can have simple resource usages with hot-swappable resource usages.
    /// </para>
    /// </summary>
    public class ResourceHelper {
        // TODO: create frugal collection, as most clips only ever have 0 or 1 resources
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
        /// The object that created this <see cref="ResourceHelper"/> instance
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
        public IResourcePathKey<T> RegisterKey<T>(string key, ResourcePathFlags flags = ResourcePathFlags.None) where T : ResourceItem {
            if (this.ResourceMap.ContainsKey(key))
                throw new InvalidOperationException("Key already registered: " + key);
            ResourcePathEntry<T> entry = new ResourcePathEntry<T>(this, key, flags);
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
                ResourceLink link = entry.link;
                if (link == null || ReferenceEquals(link.Manager, manager)) {
                    continue;
                }

                link.SetManager(manager);
            }
        }

        private void OnResourceChanged(BaseResourcePathEntry key, ResourceItem oldItem, ResourceItem newItem) {
            this.ResourceChanged?.Invoke(this, new ResourceChangedEventArgs(key, oldItem, newItem));
            this.TryInvalidateVisual();
        }

        private void OnResourceDataModified(IBaseResourcePathKey key, ResourceItem sender, string property) {
            this.ResourceDataModified?.Invoke(this, new ResourceModifiedEventArgs(key, sender, property));
            this.TryInvalidateVisual();
        }

        private void OnOnlineStateChanged(IBaseResourcePathKey key) {
            this.OnlineStateChanged?.Invoke(this, key);
            // if ((key.Flags & ResourcePathFlags.AffectRender) != 0) {
            //     this.TryInvalidateVisual();
            // }

            this.TryInvalidateVisual();
        }

        private void TryInvalidateVisual() {
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
                ResourceLink link = pair.Value.link;
                if (link != null)
                    clone.ResourceMap[pair.Key].SetTargetResourceId(link.ResourceId);
            }
        }

        private abstract class BaseResourcePathEntry : IBaseResourcePathKey {
            private readonly ResourceHelper helper;
            private readonly ResourceChangedEventHandler resourceChangedHandler;
            private readonly ResourceItemEventHandler onlineStateChangedHandler;
            public readonly string entryKey;
            public ResourceLink link;
            private readonly ResourcePathFlags flags;

            public ResourcePathFlags Flags => this.flags;

            ResourceHelper IResourceHolder.ResourceHelper => this.helper;
            ResourceLink IBaseResourcePathKey.ActiveLink => this.link;
            string IBaseResourcePathKey.Key => this.entryKey;

            public Project Project => this.helper.Owner.Project;

            protected BaseResourcePathEntry(ResourceHelper helper, string entryKey, ResourcePathFlags flags) {
                this.helper = helper ?? throw new ArgumentNullException(nameof(helper));
                this.entryKey = string.IsNullOrEmpty(entryKey) ? throw new ArgumentException("Entry id cannot be null or empty", nameof(entryKey)) : entryKey;
                this.flags = flags;
                this.resourceChangedHandler = this.OnEntryResourceChangedInternal;
                this.onlineStateChangedHandler = this.OnResourceOnlineStateChangedInternal;
            }

            public bool HasFlag(ResourcePathFlags flag) {
                return (this.flags | flag) != 0;
            }

            private void SetResourcePath(ResourceLink newLink) {
                if (this.link != null) {
                    if (this.link.CanDispose) {
                        this.link.Dispose();
                    }

                    this.link.ResourceChanged -= this.resourceChangedHandler;
                    this.link = null;
                }

                this.link = newLink;
                if (newLink != null) {
                    newLink.SetManager(this.Project?.ResourceManager);
                    newLink.ResourceChanged += this.resourceChangedHandler;
                }

                this.OnOnlineStateChanged();
                this.helper.OnOnlineStateChanged(this);
            }

            public virtual void SetTargetResourceId(ulong id) {
                if (id == ResourceManager.EmptyId)
                    throw new ArgumentException("ID must not be empty (0)");
                this.SetResourcePath(new ResourceLink(this, id));
            }

            public void ClearResourceLink() {
                if (this.link != null)
                    this.SetResourcePath(null);
            }

            public bool TryGetResource<T>(out T resource, bool requireIsOnline = true) where T : ResourceItem {
                if (this.link != null)
                    return this.link.TryGetResource(out resource, requireIsOnline);
                resource = null;
                return false;
            }

            public abstract bool IsItemTypeApplicable(ResourceItem item);

            protected abstract void OnEntryResourceChanged(ResourceItem oldItem, ResourceItem newItem);

            private void OnEntryResourceChangedInternal(ResourceItem oldItem, ResourceItem newItem) {
                if (oldItem != null) {
                    oldItem.OnlineStateChanged -= this.onlineStateChangedHandler;
                }

                if (newItem != null && this.IsItemTypeApplicable(newItem)) {
                    newItem.OnlineStateChanged += this.onlineStateChangedHandler;
                    this.OnEntryResourceChanged(oldItem, newItem);
                    this.helper.OnResourceChanged(this, oldItem, newItem);
                }
                else {
                    this.OnEntryResourceChanged(oldItem, null);
                    this.helper.OnResourceChanged(this, oldItem, null);
                }
            }

            protected abstract void OnOnlineStateChanged();

            private void OnResourceOnlineStateChangedInternal(ResourceItem item) {
                this.OnOnlineStateChanged();
                this.helper.OnOnlineStateChanged(this);
            }

            public static void WriteToRBE(BaseResourcePathEntry entry, RBEDictionary resourceMapDictionary) {
                if (entry.link == null)
                    return;
                ResourceLink.WriteToRBE(entry.link, resourceMapDictionary.CreateDictionary(entry.entryKey));
            }

            public static void ReadFromRBE(BaseResourcePathEntry entry, RBEDictionary dictionary) {
                entry.SetResourcePath(ResourceLink.ReadFromRBE(entry, dictionary));
            }

            public void DisposePath() => this.SetResourcePath(null);
        }

        private class ResourcePathEntry<T> : BaseResourcePathEntry, IResourcePathKey<T> where T : ResourceItem {
            public event EntryResourceChangedEventHandler<T> ResourceChanged;
            public event EntryOnlineStateChangedEventHandler<T> OnlineStateChanged;

            public ResourcePathEntry(ResourceHelper helper, string entryKey, ResourcePathFlags flags) : base(helper, entryKey, flags) {
            }

            public bool TryGetResource(out T resource, bool requireIsOnline = true) => base.TryGetResource(out resource, requireIsOnline);

            public override bool IsItemTypeApplicable(ResourceItem item) => item is T;

            protected override void OnEntryResourceChanged(ResourceItem oldItem, ResourceItem newItem) {
                this.ResourceChanged?.Invoke(this, (T) oldItem, (T) newItem);
            }

            protected override void OnOnlineStateChanged() {
                this.OnlineStateChanged?.Invoke(this);
            }
        }
    }
}