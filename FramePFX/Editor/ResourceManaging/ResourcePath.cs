using System;
using System.Diagnostics;
using FramePFX.Editor.ResourceManaging.Events;
using FramePFX.RBC;

namespace FramePFX.Editor.ResourceManaging {
    /// <summary>
    /// Base class the non-generic and generic resource path classes
    /// </summary>
    public abstract class ResourcePathBase : IDisposable {
        protected readonly ResourceItemEventHandler resourceAddedHandler;
        protected readonly ResourceItemEventHandler resourceRemovedHandler;
        protected readonly ResourceReplacedEventHandler resourceReplacedHandler;
        protected readonly ResourceItemEventHandler onlineStateChangedHandler;

        // volatile juuust in case...
        protected volatile bool isDisposing;
        protected volatile bool isDisposed;
        protected bool isManagerBeingReplaced;

        public bool IsDisposing => this.isDisposing;
        public bool IsDisposed => this.isDisposed;
        public bool CanDispose => !this.isDisposed && !this.isDisposing;

        public ResourceManager Manager { get; protected set; }

        public ulong ResourceId { get; protected set; }

        /// <summary>
        /// The online state of this resource. True means the state is valid and accessible. False
        /// means the state is invalid and cannot be access. Null means the resource hasn't been resolved yet or
        /// there is no manager associated with this instance
        /// </summary>
        public bool? IsValid { get; protected set; }

        /// <summary>
        /// An event called when this resource path is disposed. This is only
        /// called when explicitly disposed; finalizer does not call this event
        /// </summary>
        public event EventHandler Disposed;

        public event ResourceItemEventHandler OnlineStateChanged;

        protected ResourcePathBase(ResourceManager manager, ulong resourceId) {
            this.ResourceId = resourceId == ResourceManager.EmptyId ? throw new ArgumentException("Unique id cannot be 0 (null)") : resourceId;
            this.resourceAddedHandler = this.OnManagerResourceAdded;
            this.resourceRemovedHandler = this.OnManagerResourceRemoved;
            this.resourceReplacedHandler = this.OnManagerResourceReplaced;
            this.onlineStateChangedHandler = this.OnOnlineStateChanged;
            if (manager != null) {
                this.Manager = manager;
                this.AttachManager(manager);
            }
        }

        ~ResourcePathBase() {
            this.Dispose(false);
        }

        public void SetManager(ResourceManager manager) {
            this.EnsureNotDisposed();
            this.EnsureNotReplacingManager("Cannot set manager while it is already being set");
            ResourceManager oldManager = this.Manager;
            if (ReferenceEquals(oldManager, manager)) {
                if (manager != null) {
                    Debugger.Break();
                    Debug.WriteLine($"[{this.GetType().Name}] Attempted to set the same manager instance:\n{new StackTrace(true)}");
                }

                return;
            }

            this.isManagerBeingReplaced = true;
            this.ClearInternalResource(true);
            if (oldManager != null) {
                this.DetachManager(oldManager);
            }

            this.Manager = manager;
            if (manager != null) {
                this.AttachManager(manager);
            }

            this.isManagerBeingReplaced = false;
        }

        protected void AttachManager(ResourceManager manager) {
            manager.ResourceAdded += this.resourceAddedHandler;
            manager.ResourceRemoved += this.resourceRemovedHandler;
            manager.ResourceReplaced += this.resourceReplacedHandler;
        }

        protected void DetachManager(ResourceManager manager) {
            manager.ResourceAdded -= this.resourceAddedHandler;
            manager.ResourceRemoved -= this.resourceRemovedHandler;
            manager.ResourceReplaced -= this.resourceReplacedHandler;
        }

        protected abstract void OnManagerResourceAdded(ResourceManager manager, ResourceItem item);

        protected abstract void OnManagerResourceRemoved(ResourceManager manager, ResourceItem item);

        protected abstract void OnManagerResourceReplaced(ResourceManager manager, ulong id, ResourceItem oldItem, ResourceItem newItem);

        protected virtual void OnOnlineStateChanged(ResourceManager manager, ResourceItem item) {
            this.OnlineStateChanged?.Invoke(manager, item);
        }

        /// <summary>
        /// Clears this path's internal resource
        /// </summary>
        /// <param name="fireResourceChanged">Whether to fire the resource changed event. There is practically no reason to not fire the event, so this should always be true</param>
        protected abstract void ClearInternalResource(bool fireResourceChanged = true);

        protected virtual void OnResourceChanged(ResourceItem oldItem, ResourceItem newItem) {
            if (oldItem != null)
                oldItem.OnlineStateChanged -= this.onlineStateChangedHandler;
            if (newItem != null)
                newItem.OnlineStateChanged += this.onlineStateChangedHandler;
        }

        /// <summary>
        /// Disposes this resource path, removing all <see cref="ResourceManager"/> handlers that it has registered,
        /// then sets the manager to null which in tern sets the cached item to null (invoking the <see cref="ResourceChanged"/> event), and then
        /// finally invokes the <see cref="Disposed"/> event
        /// </summary>
        public void Dispose() {
            if (this.isDisposed) {
                return;
            }

            this.Dispose(true);
            this.Disposed?.Invoke(this, EventArgs.Empty);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                this.ClearInternalResource();
            }

            // finalizer call would most likely mean Manager is null, because otherwise how
            // could there be no references if the event handlers are still registered??
            ResourceManager manager = this.Manager;
            if (manager != null) {
                this.Manager = null;
                this.DetachManager(manager);
            }
        }

        protected void EnsureNotDisposed(string message = null) {
            if (this.isDisposed) {
                throw new ObjectDisposedException(this.GetType().Name, message ?? "This resource path is disposed");
            }
        }

        protected void EnsureNotReplacingManager(string message) {
            if (this.isManagerBeingReplaced) {
                throw new InvalidOperationException(message);
            }
        }
    }

    public sealed class ResourcePath : ResourcePathBase {
        public delegate void ResourceChangedEventHandler(ResourceItem oldItem, ResourceItem newItem);

        private ResourceItem cached;

        /// <summary>
        /// An event that gets fired when this path's internal cached resource changes, e.g. due to <see cref="TryGetResource"/> being
        /// invokes and a resource being resolved, the resource being added or removed from the manager, etc
        /// </summary>
        public event ResourceChangedEventHandler ResourceChanged;

        public ResourcePath(ResourceManager manager, ulong resourceId) : base(manager, resourceId) {
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
                Debug.WriteLine($"[{this.GetType().Name}] Attempted to set resource to same instance");
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
                Debug.WriteLine($"[{this.GetType().Name}] Attempted to set the same resource ID");
                return;
            }

            this.ResourceId = uniqueId;
            if (this.IsValid == true) {
                this.SetInternalResource(null, fireResourceChanged);
            }

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
            this.EnsureNotReplacingManager("Cannot attempt to get resource while manager is being set");
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

        protected override void ClearInternalResource(bool fireResourceChanged = true) {
            if (this.GetInternalResource() != null) {
                this.SetInternalResource(null, fireResourceChanged);
            }
        }

        protected override void OnManagerResourceAdded(ResourceManager manager, ResourceItem item) {
            if (this.isDisposed) {
#if DEBUG
                Debugger.Break();
                Debug.WriteLine("RESOURCE IS DISPOSED BUT RECEIVED RESOURCE ADDED EVENT!!!!!!!!!!!!!!!!!!!!!!!");
#endif
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

        protected override void OnManagerResourceRemoved(ResourceManager manager, ResourceItem item) {
            if (this.isDisposed) {
#if DEBUG
                Debugger.Break();
                Debug.WriteLine("RESOURCE IS DISPOSED BUT RECEIVED RESOURCE REMOVED EVENT!!!!!!!!!!!!!!!!!!!!!!!");
#endif
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

        protected override void OnManagerResourceReplaced(ResourceManager manager, ulong id, ResourceItem oldItem, ResourceItem newItem) {
            if (this.isDisposed) {
#if DEBUG
                Debugger.Break();
                Debug.WriteLine("RESOURCE IS DISPOSED BUT RECEIVED RESOURCE REPLACED EVENT!!!!!!!!!!!!!!!!!!!!!!!");
#endif
                return;
            }

            if (id != this.ResourceId) {
                return;
            }

            if (this.IsValid == true) {
                if (this.cached == null)
                    throw new Exception("Expected our cached item to not be null");
                if (!ReferenceEquals(this.cached, oldItem))
                    throw new Exception("Expected the cached item to equal the new item");
            }

            this.SetInternalResource(newItem);
        }

        public bool IsCachedItemEqualTo(ResourceItem item) {
            return ReferenceEquals(this.cached, item);
        }

        public static void WriteToRBE(ResourcePath resource, RBEDictionary data) {
            data.SetULong(nameof(resource.ResourceId), resource.ResourceId);
        }

        public static ResourcePath ReadFromRBE(RBEDictionary data) {
            ulong id = data.GetULong(nameof(ResourceId));
            if (id == ResourceManager.EmptyId)
                throw new ArgumentException("Resource ID from the data was 0 (null)");
            return new ResourcePath(null, id);
        }
    }

    /// <summary>
    /// A helper class for managing a single resource
    /// </summary>
    public sealed class ResourcePath<T> : ResourcePathBase where T : ResourceItem {
        public delegate void ResourceChangedEventHandler(T oldItem, T newItem);

        private T cached;

        /// <summary>
        /// An event that gets fired when this path's internal cached resource changes, e.g. due to <see cref="TryGetResource"/> being
        /// invokes and a resource being resolved, the resource being added or removed from the manager, etc
        /// </summary>
        public event ResourceChangedEventHandler ResourceChanged;

        public ResourcePath(ResourceManager manager, ulong resourceId) : base(manager, resourceId) {
        }

        private void SetInternalResource(T item, bool fireEvent = true) {
            T oldItem = this.cached;
            if (this.IsValid == true) {
                if (oldItem == null)
                    throw new Exception("Expected non-null cached item when state is valid");
                this.IsValid = null;
            }
            else if (oldItem != null) {
                throw new Exception("Expected null cached item when state is invalid or unknown");
            }

            if (ReferenceEquals(oldItem, item)) {
                Debug.WriteLine($"[{this.GetType().Name}] Attempted to set resource to same instance");
                return;
            }

            this.SetInternalResourceUnsafe(oldItem, item, fireEvent);
        }

        private void SetInternalResourceUnsafe(T oldItem, T newItem, bool fireEvent = true) {
            this.cached = newItem;
            this.IsValid = newItem != null ? (bool?) true : null;
            this.OnResourceChanged(oldItem, newItem);
            if (fireEvent) {
                this.ResourceChanged?.Invoke(oldItem, newItem);
            }
        }

        private T GetInternalResource() {
            T item = this.cached;
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
                Debug.WriteLine($"[{this.GetType().Name}] Attempted to set the same resource ID");
                return;
            }

            this.ResourceId = uniqueId;
            if (this.IsValid == true) {
                this.SetInternalResource(null, fireResourceChanged);
            }

            this.IsValid = null;
        }

        protected override void ClearInternalResource(bool fireResourceChanged = true) {
            if (this.GetInternalResource() != null) {
                // lazy; let SetInternalResource throw exceptions
                this.SetInternalResource(null, fireResourceChanged);
            }
        }

        /// <summary>
        /// Tries to retrieve a cached resource item, otherwise the resource path is looked up in the owning <see cref="ResourceManager"/>.
        /// If a resource could not be found or the type does not match <see cref="T"/>, then this method returns false
        /// </summary>
        /// <param name="resource">The output resource that was found. May be non-null when this function returns false, indicating the resource is offline when <see cref="requireIsOnline"/> is false</param>
        /// <param name="requireIsOnline">Whether the resource is required to be online for this function to return true</param>
        /// <returns>True if a valid resource was found and <see cref="requireIsOnline"/> matches its online state</returns>
        /// <exception cref="Exception">Internal errors that should not occur; cached item was wrong</exception>
        public bool TryGetResource(out T resource, bool requireIsOnline = true) {
            this.EnsureNotDisposed();
            this.EnsureNotReplacingManager("Cannot attempt to get resource while manager is being set");
            switch (this.IsValid) {
                case false:
                    if (this.cached != null)
                        throw new Exception("Expected null cached item when state is invalid");
                    resource = null;
                    return false;
                case true:
                    if (this.cached == null)
                        throw new Exception("Expected non-null cached item when state is valid");
                    return (resource = this.cached).IsOnline || !requireIsOnline;
                default: {
                    ResourceManager manager = this.Manager;
                    if (manager != null && manager.TryGetEntryItem(this.ResourceId, out ResourceItem res) && res is T value) {
                        this.SetInternalResource(resource = value);
                        return resource.IsOnline || !requireIsOnline;
                    }

                    this.IsValid = false;
                    resource = null;
                    return false;
                }
            }
        }

        protected override void OnManagerResourceAdded(ResourceManager manager, ResourceItem item) {
            if (this.isDisposed) {
#if DEBUG
                Debugger.Break();
                Debug.WriteLine("RESOURCE IS DISPOSED BUT RECEIVED RESOURCE ADDED EVENT!!!!!!!!!!!!!!!!!!!!!!!");
#endif
                return;
            }

            if (item.UniqueId != this.ResourceId)
                return;
            if (this.IsValid == true)
                throw new Exception("Expected the resource to be invalid/unknown, not online");
            if (this.cached != null)
                throw new Exception("Expected the cached item to be null");

            if (!(item is T value)) {
                this.IsValid = false;
                return;
            }

            this.SetInternalResource(value);
        }

        protected override void OnManagerResourceRemoved(ResourceManager manager, ResourceItem item) {
            if (this.isDisposed) {
#if DEBUG
                Debugger.Break();
                Debug.WriteLine("RESOURCE IS DISPOSED BUT RECEIVED RESOURCE REMOVED EVENT!!!!!!!!!!!!!!!!!!!!!!!");
#endif
                return;
            }

            if (item.UniqueId != this.ResourceId) {
                return;
            }

            if (this.IsValid == true) {
                if (this.cached == null) {
                    throw new Exception("Expected our cached item to not be null");
                }

                if (!ReferenceEquals(this.cached, item)) {
                    throw new Exception("Expected the cached item to equal the removed item");
                }
            }

            this.SetInternalResource(null);
        }

        protected override void OnManagerResourceReplaced(ResourceManager manager, ulong id, ResourceItem oldItem, ResourceItem newItem) {
            if (this.isDisposed) {
#if DEBUG
                Debugger.Break();
                Debug.WriteLine("RESOURCE IS DISPOSED BUT RECEIVED RESOURCE REPLACED EVENT!!!!!!!!!!!!!!!!!!!!!!!");
#endif
                return;
            }

            if (id != this.ResourceId) {
                return;
            }

            if (this.IsValid == true) {
                if (this.cached == null)
                    throw new Exception("Expected our cached item to not be null");
                if (!ReferenceEquals(this.cached, oldItem))
                    throw new Exception("Expected the cached item to equal the new item");
            }

            if (!(newItem is T value)) {
                this.IsValid = false;
                return;
            }

            this.SetInternalResource(value);
        }

        public bool IsCachedItemEqualTo(ResourceItem item) {
            return item is T && ReferenceEquals(this.cached, item);
        }

        public static void WriteToRBE(ResourcePath<T> resource, RBEDictionary data) {
            data.SetULong(nameof(resource.ResourceId), resource.ResourceId);
        }

        public static ResourcePath<T> ReadFromRBE(ResourceManager manager, RBEDictionary data) {
            ulong id = data.GetULong(nameof(ResourceId));
            if (id == ResourceManager.EmptyId)
                throw new ArgumentException("Resource ID from the data was 0 (null)");
            return new ResourcePath<T>(manager, id);
        }
    }
}