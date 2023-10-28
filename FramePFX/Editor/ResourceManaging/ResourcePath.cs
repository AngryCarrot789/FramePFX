using System;
using System.Diagnostics;
using FramePFX.Editor.ResourceManaging.Events;
using FramePFX.Editor.Timelines.ResourceHelpers;
using FramePFX.Logger;
using FramePFX.RBC;

namespace FramePFX.Editor.ResourceManaging {
    /// <summary>
    /// A class which encapsulated a resource reference and handles the reference counter for the
    /// resource, and provides events for when the resource changes, is replaced, removed and/or added back.
    /// <para>
    /// This class makes managing a resource much simpler, though it still needs a bit of work done I think, as
    /// it's very easy to accidentally create a loading loop (and therefore a crash)
    /// </para>
    /// </summary>
    public sealed class ResourcePath : IDisposable {
        private readonly ResourceAndManagerEventHandler resourceAddedHandler;
        private readonly ResourceAndManagerEventHandler resourceRemovedHandler;
        private readonly ResourceAndManagerEventHandler onlineStateChangedHandler;

        private ResourceItem cached;        // our cached resource
        private bool isDisposing;           // are we currently disposing?
        private bool isDisposed;            // are we disposed and cannot be used again?
        private bool isManagerChanging;     // is the manager being changed? used for fail-fast exceptions
        private bool isReferencedCounted;   // is this instance counted as a reference to our resource?

        public bool IsDisposing => this.isDisposing;
        public bool IsDisposed => this.isDisposed;
        public bool CanDispose => !this.isDisposed && !this.isDisposing;

        /// <summary>
        /// This resource path's manager, which is used to query resources from and listen to events
        /// </summary>
        public ResourceManager Manager { get; private set; }

        /// <summary>
        /// Gets this resource path's ID. This will not be <see cref="ResourceManager.EmptyId"/>
        /// and will therefore always be a valid ID
        /// </summary>
        public ulong ResourceId { get; private set; }

        /// <summary>
        /// The online state of this path.
        /// <para>True means we have resolved a resource and it is valid (as per <see cref="IsItemApplicable"/>)</para>
        /// <para>False means the resource did not exist or was the wrong type (as per <see cref="IsItemApplicable"/>)</para>
        /// <para>Null means the resource hasn't been resolved yet or there is no manager associated with this path</para>
        /// </summary>
        public bool? IsValid { get; private set; }

        /// <summary>
        /// An event that gets fired when this path's resource changes. This can be due to the resource being deleted or
        /// added back (after deletion), <see cref="TryGetResource"/> being invoked and a resource being resolved, etc.
        /// </summary>
        public event ResourceChangedEventHandler ResourceChanged;

        /// <summary>
        /// An event fired when the online state of our resource changes (as in, when <see cref="ResourceItem.IsOnline"/> changes)
        /// </summary>
        public event ResourceAndManagerEventHandler OnlineStateChanged;

        /// <summary>
        /// Gets the resource path key that owns this resource path
        /// </summary>
        public IBaseResourcePathKey Owner { get; }

        public ResourcePath(IBaseResourcePathKey owner, ulong resourceId) : this(owner, null, resourceId) { }

        public ResourcePath(IBaseResourcePathKey owner, ResourceManager manager, ulong resourceId) {
            this.ResourceId = resourceId == ResourceManager.EmptyId ? throw new ArgumentException("Unique id cannot be 0 (null)") : resourceId;
            this.Owner = owner ?? throw new ArgumentNullException(nameof(owner));
            this.resourceAddedHandler = this.OnManagerResourceAdded;
            this.resourceRemovedHandler = this.OnManagerResourceRemoved;
            this.onlineStateChangedHandler = this.OnOnlineStateChanged;
            if (manager != null) {
                this.Manager = manager;
                this.AttachManager(manager);
            }
        }

        public bool IsItemApplicable(ResourceItem item) => this.Owner.IsItemTypeApplicable(item);

        public void SetManager(ResourceManager manager) {
            this.EnsureNotDisposed();
            this.EnsureNotDisposing();
            this.EnsureManagerNotChanging("Cannot set manager while it is already being set");
            ResourceManager oldManager = this.Manager;
            if (ReferenceEquals(oldManager, manager)) {
                if (manager != null) {
                    Debugger.Break();
                    AppLogger.WriteLine($"[{this.GetType().Name}] Attempted to set the same manager instance:\n{Environment.StackTrace}");
                }

                return;
            }

            this.isManagerChanging = true;

            if (oldManager != null)
                this.DetachManager(oldManager);

            this.ClearInternalResource();
            this.IsValid = null;

            this.Manager = manager;
            if (manager != null)
                this.AttachManager(manager);

            this.isManagerChanging = false;
        }

        private void SetInternalResource(ResourceItem item, bool fireEvent = true) {
            ResourceItem oldItem = this.cached;
            if (this.IsValid == true) {
                if (oldItem == null)
                    throw new Exception("Expected non-null cached item when state is valid");
                this.IsValid = null;
            }
            else if (oldItem != null) {
                throw new Exception("Expected null cached item when state is invalid or unknown");
            }

            if (ReferenceEquals(oldItem, item)) {
                AppLogger.WriteLine($"Attempted to set resource to the same instance: {oldItem} -> {item}");
                AppLogger.WriteLine(Environment.StackTrace);
                return;
            }

            this.SetInternalResourceUnsafe(oldItem, item, fireEvent);
        }

        private void SetInternalResourceUnsafe(ResourceItem oldItem, ResourceItem newItem, bool fireEvent = true) {
            this.cached = newItem;
            this.IsValid = newItem != null ? (bool?) true : null;
            this.OnResourceChanged(oldItem, newItem);
            if (fireEvent) {
                this.ResourceChanged?.Invoke(oldItem, newItem);
            }
        }

        private void OnResourceChanged(ResourceItem oldItem, ResourceItem newItem) {
            if (oldItem != null) {
                oldItem.OnlineStateChanged -= this.onlineStateChangedHandler;
                this.SetReferenceCount(oldItem, false);
            }

            if (newItem != null) {
                newItem.OnlineStateChanged += this.onlineStateChangedHandler;
                this.SetReferenceCount(newItem, true);
            }
        }

        public void SetReferenceCount(ResourceItem item, bool isReferenced) {
            if (isReferenced) {
                if (!this.isReferencedCounted) {
                    item.AddReference(this);
                    this.isReferencedCounted = true;
                }
            }
            else if (this.isReferencedCounted) {
                item.RemoveReference(this);
                this.isReferencedCounted = false;
            }
        }

        private ResourceItem GetInternalResource() {
            ResourceItem item = this.cached;
            if (this.IsValid == true) {
                if (item == null)
                    throw new Exception("Expected non-null cached item when state is valid");
                return item;
            }

            if (item != null)
                throw new Exception("Expected null cached item when state is invalid or unknown");
            return null;
        }

        public void SetResourceId(ulong uniqueId, bool fireResourceChanged = true) {
            this.EnsureNotDisposed();
            if (uniqueId == ResourceManager.EmptyId) {
                throw new ArgumentException("Unique id cannot be 0 (null)");
            }

            if (this.ResourceId == uniqueId) {
                AppLogger.WriteLine($"[{this.GetType().Name}] Attempted to set the same resource ID");
                return;
            }

            this.ResourceId = uniqueId;
            if (this.IsValid == true)
                this.SetInternalResource(null, fireResourceChanged);
            this.IsValid = null;
        }

        /// <summary>
        /// Tries to retrieve a cached resource item, otherwise the resource path is looked up in the owning <see cref="ResourceManager"/>.
        /// If a resource could not be found or the type does not match <see cref="T"/>, then this method returns false
        /// </summary>
        /// <param name="resource">The output resource that was found. May be non-null when this function returns false, indicating the resource is offline when <see cref="requireIsOnline"/> is false</param>
        /// <param name="requireIsOnline">Whether the resource is required to be online for this function to return true</param>
        /// <typeparam name="T">The type of resource that is required</typeparam>
        /// <returns>True if a valid resource was found and <see cref="requireIsOnline"/> matches its online state</returns>
        /// <exception cref="Exception">Internal errors that should not occur; cached item was wrong</exception>
        public bool TryGetResource<T>(out T resource, bool requireIsOnline = true) where T : ResourceItem {
            this.EnsureNotDisposed();
            this.EnsureManagerNotChanging("Cannot attempt to get resource while manager is being set");
            switch (this.IsValid) {
                case false: {
                    if (this.cached != null)
                        throw new Exception("Expected null cached item when state is invalid");
                    resource = null;
                    return false;
                }
                case true: {
                    if (this.cached == null)
                        throw new Exception("Expected non-null cached item when state is valid");
                    if (this.cached is T t)
                        return (resource = t).IsOnline || !requireIsOnline;
                    this.SetInternalResourceUnsafe(this.cached, resource = null);
                    return false;
                }
                default: {
                    ResourceManager manager = this.Manager;
                    if (manager != null && manager.TryGetEntryItem(this.ResourceId, out ResourceItem res) && res is T t) {
                        this.SetInternalResource(resource = t);
                        return t.IsOnline || !requireIsOnline;
                    }

                    this.IsValid = false;
                    resource = null;
                    return false;
                }
            }
        }

        private void ClearInternalResource(bool fireResourceChanged = true) {
            if (this.GetInternalResource() != null) {
                this.SetInternalResource(null, fireResourceChanged);
            }
        }

        private void OnManagerResourceAdded(ResourceManager manager, ResourceItem item) {
            if (this.isDisposed) {
                AppLogger.WriteLine("RESOURCE IS DISPOSED BUT RECEIVED RESOURCE ADDED EVENT!!!!!!!!!!!!!!!!!!!!!!!");
                Debugger.Break();
                return;
            }

            if (item.UniqueId != this.ResourceId)
                return;
            if (this.IsValid == true)
                throw new Exception("Expected the state to be invalid or unknown, not valid");
            if (this.cached != null)
                throw new Exception("Expected the cached item to be null");
            this.SetInternalResource(item);
        }

        private void OnManagerResourceRemoved(ResourceManager manager, ResourceItem item) {
            if (this.isDisposed) {
                AppLogger.WriteLine("RESOURCE IS DISPOSED BUT RECEIVED RESOURCE REMOVED EVENT!!!!!!!!!!!!!!!!!!!!!!!");
                Debugger.Break();
                return;
            }

            if (item.UniqueId != this.ResourceId) {
                return;
            }

            if (this.IsValid == true) {
                if (this.cached == null)
                    throw new Exception("Expected our cached item to not be null");
                if (!ReferenceEquals(this.cached, item))
                    throw new Exception("Expected the cached item to equal the removed item");
            }

            this.SetInternalResource(null);
        }

        private void OnOnlineStateChanged(ResourceManager manager, ResourceItem item) {
            this.OnlineStateChanged?.Invoke(manager, item);
        }

        public bool IsCachedItemEqualTo(ResourceItem item) {
            return ReferenceEquals(this.cached, item);
        }

        private void AttachManager(ResourceManager manager) {
            manager.ResourceAdded += this.resourceAddedHandler;
            manager.ResourceRemoved += this.resourceRemovedHandler;
        }

        private void DetachManager(ResourceManager manager) {
            manager.ResourceAdded -= this.resourceAddedHandler;
            manager.ResourceRemoved -= this.resourceRemovedHandler;
        }

        /// <summary>
        /// Disposes this resource path. This first clears the resource (causing the <see cref="ResourceChanged"/>
        /// event to be fired), removes the <see cref="ResourceManager"/> handlers and finally sets the manager
        /// to null
        /// </summary>
        public void Dispose() {
            this.isDisposing = true;
            this.ClearInternalResource();
            ResourceManager manager = this.Manager;
            if (manager != null) {
                this.Manager = null;
                this.DetachManager(manager);
            }

            this.isDisposed = true;
            this.isDisposing = false;
        }

        private void EnsureNotDisposed(string message = null) {
            if (this.isDisposed)
                throw new ObjectDisposedException(this.GetType().Name, message ?? "This resource path is disposed");
        }

        private void EnsureNotDisposing(string message = null) {
            if (this.isDisposing)
                throw new ObjectDisposedException(this.GetType().Name, message ?? "This resource path is being disposed");
        }

        private void EnsureManagerNotChanging(string message) {
            if (this.isManagerChanging)
                throw new InvalidOperationException(message);
        }

        #region Serialisation

        public static void WriteToRBE(ResourcePath resource, RBEDictionary data) {
            data.SetULong(nameof(resource.ResourceId), resource.ResourceId);
        }

        public static ResourcePath ReadFromRBE(IBaseResourcePathKey owner, RBEDictionary data) {
            ulong id = data.GetULong(nameof(ResourceId));
            if (id == ResourceManager.EmptyId)
                throw new ArgumentException("Resource ID from the data was 0 (null)");
            return new ResourcePath(owner, id);
        }

        #endregion
    }
}