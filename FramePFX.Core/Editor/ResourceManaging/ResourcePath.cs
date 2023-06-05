using System;
using System.Diagnostics;
using FramePFX.Core.Editor.ResourceManaging.Events;
using FramePFX.Core.RBC;

namespace FramePFX.Core.Editor.ResourceManaging {
    /// <summary>
    /// A helper class for managing a single resource
    /// </summary>
    public class ResourcePath<T> : IDisposable where T : ResourceItem {
        public delegate void ResourceChangedEventHandler(T oldItem, T newItem);

        private readonly ResourceItemEventHandler resourceAddedHandler;
        private readonly ResourceItemEventHandler resourceRemovedHandler;
        private readonly ResourceRenamedEventHandler resourceRenamedHandler;
        private volatile bool isDisposing; // volatile juuust in case...
        private volatile bool isDisposed;
        private T cached;

        public ResourceManager Manager { get; private set; }

        public string UniqueId { get; private set; }

        public bool? IsOnline { get; private set; }

        public bool IsDisposing => this.isDisposing;
        public bool IsDisposed => this.isDisposed;

        /// <summary>
        /// An event that gets fired when this path's internal cached resource changes, e.g. due to <see cref="TryGetResource"/> being
        /// invokes and a resource being resolved, the resource being added or removed from the manager, etc
        /// </summary>
        public ResourceChangedEventHandler ResourceChanged;

        /// <summary>
        /// An event called when this resource path is disposed. This is only
        /// called when explicitly disposed; finalizer does not call this event
        /// </summary>
        public EventHandler Disposed;

        public ResourcePath(ResourceManager manager, string uniqueId) {
            this.UniqueId = string.IsNullOrWhiteSpace(uniqueId) ? throw new ArgumentException("Unique id cannot be null, empty or whitespaces") : uniqueId;
            this.resourceAddedHandler = this.OnManagerResourceAdded;
            this.resourceRemovedHandler = this.OnManagerResourceRemoved;
            this.resourceRenamedHandler = this.OnManagerResourceRenamed;
            this.SetManager(manager);
        }

        private void SetInternalResource(T item, bool fireEvent = true) {
            T oldItem = this.cached;
            if (this.IsOnline == true) {
                if (oldItem == null)
                    throw new Exception("Expected non-null cached item when resource is online");
                this.IsOnline = null;
            }
            else if (oldItem != null) {
                throw new Exception("Expected null cached item when resource is offline or unknown");
            }

            if (ReferenceEquals(oldItem, item)) {
                Debug.WriteLine($"[{this.GetType().Name}] Attempted to set resource to same instance");
                return;
            }

            this.cached = item;
            this.IsOnline = item != null ? (bool?) true : null;
            if (fireEvent) {
                this.ResourceChanged?.Invoke(oldItem, item);
            }
        }

        private T GetInternalResource() {
            T item = this.cached;
            if (this.IsOnline == true) {
                if (item == null)
                    throw new Exception("Expected non-null cached item when resource is online");
                return item;
            }

            if (item != null)
                throw new Exception("Expected null cached item when resource is offline or unknown");
            return null;
        }

        public void SetResourceId(string uniqueId, bool fireResourceChanged = true) {
            this.EnsureNotDispose();
            if (string.IsNullOrWhiteSpace(uniqueId)) {
                throw new ArgumentException("Unique id cannot be null, empty or whitespaces");
            }

            if (this.UniqueId == uniqueId) {
                Debug.WriteLine($"[{this.GetType().Name}] Attempted to set the same resource ID");
                return;
            }

            this.UniqueId = uniqueId;
            if (this.IsOnline == true) {
                this.SetInternalResource(null, fireResourceChanged);
            }

            this.IsOnline = null;
        }

        public void SetManager(ResourceManager newManager, bool fireResourceChanged = true) {
            this.EnsureNotDispose();
            ResourceManager oldManager = this.Manager;
            if (ReferenceEquals(oldManager, newManager)) {
                Debug.WriteLine($"[{this.GetType().Name}] Attempted to set the same manager instance");
                return;
            }

            if (this.cached != null) { // lazy; let SetInternalResource throw exceptions
                this.SetInternalResource(null, fireResourceChanged);
            }

            this.Manager = newManager;

            if (oldManager != null) {
                oldManager.ResourceAdded -= this.resourceAddedHandler;
                oldManager.ResourceRemoved -= this.resourceRemovedHandler;
                oldManager.ResourceRenamed -= this.resourceRenamedHandler;
            }

            if (newManager != null) {
                newManager.ResourceAdded += this.resourceAddedHandler;
                newManager.ResourceRemoved += this.resourceRemovedHandler;
                newManager.ResourceRenamed += this.resourceRenamedHandler;
            }
        }

        ~ResourcePath() {
            this.Dispose(false);
        }

        public bool TryGetResource(out T resource) {
            this.EnsureNotDispose();
            switch (this.IsOnline) {
                case false:
                    if (this.cached != null)
                        throw new Exception("Expected null cached item when resource is offline");
                    resource = null;
                    return false;
                case true:
                    if (this.cached == null)
                        throw new Exception("Expected non-null cached item when resource is online");
                    resource = this.cached;
                    return true;
                default: {
                    ResourceManager manager = this.Manager;
                    if (manager != null && manager.TryGetResource(this.UniqueId, out ResourceItem res) && res is T value) {
                        this.SetInternalResource(resource = value);
                        return true;
                    }

                    this.IsOnline = false;
                    resource = null;
                    return false;
                }
            }
        }

        private void OnManagerResourceAdded(ResourceManager manager, ResourceItem item) {
            if (this.isDisposed) {
                Debug.WriteLine("RESOURCE IS DISPOSED BUT RECEIVED RESOURCE ADDED EVENT!!!!!!!!!!!!!!!!!!!!!!!");
                return;
            }

            if (item.UniqueId != this.UniqueId)
                return;
            if (this.IsOnline == true)
                throw new Exception("Expected the resource to be offline/unknown, not online");
            if (this.cached != null)
                throw new Exception("Expected the cached item to be null");

            if (!(item is T value)) {
                this.IsOnline = false;
                return;
            }

            this.SetInternalResource(value);
            this.IsOnline = true;
        }

        private void OnManagerResourceRemoved(ResourceManager manager, ResourceItem item) {
            if (this.isDisposed) {
                Debug.WriteLine("RESOURCE IS DISPOSED BUT RECEIVED RESOURCE REMOVED EVENT!!!!!!!!!!!!!!!!!!!!!!!");
                return;
            }

            if (item.UniqueId != this.UniqueId) {
                return;
            }

            if (this.IsOnline == true) {
                if (this.cached == null) {
                    throw new Exception("Expected our cached item to not be null");
                }

                if (!ReferenceEquals(this.cached, item)) {
                    throw new Exception("Expected the cached item to equal the removed item");
                }
            }

            this.SetInternalResource(null);
        }

        private void OnManagerResourceRenamed(ResourceManager manager, ResourceItem item, string oldId, string newId) {
            if (this.isDisposed) {
                Debug.WriteLine("RESOURCE IS DISPOSED BUT RECEIVED RESOURCE RENAME EVENT!!!!!!!!!!!!!!!!!!!!!!!");
                return;
            }

            if (oldId == this.UniqueId) { // our possibly active resource was renamed
                if (this.IsOnline == true) {
                    if (this.cached == null)
                        throw new Exception("Expected our cached item to not be null");
                    if (!ReferenceEquals(this.cached, item))
                        throw new Exception("Expected the cached item to equal the removed item");
                }
                else {
                    if (this.IsOnline != null) {
                        throw new Exception("Expected online state to be unknown, not offline");
                    }

                    if (item is T value) {
                        this.SetInternalResource(value);
                    }
                    else if (this.cached != null) {
                        throw new Exception("Expected null cached item when resource online state is unknown");
                    }
                }

                this.UniqueId = newId;
            }
            else if (newId == this.UniqueId) { // a random resource was named our resource; try to use it
                if (this.IsOnline == true) {
                    throw new Exception("Expected online state to be offline or unknown, not online");
                }

                if (item is T value) {
                    this.SetInternalResource(value);
                }
                else {
                    this.IsOnline = false;
                }
            }
        }

        /// <summary>
        /// Disposes this resource path, removing all <see cref="ResourceManager"/> handlers that it has registered,
        /// then sets the manager to null which in tern sets the cached item to null (invoking the <see cref="ResourceChanged"/> event), and then
        /// finally invokes the <see cref="Disposed"/> event
        /// </summary>
        public void Dispose() {
            GC.SuppressFinalize(this);
            this.Dispose(true);
        }

        private void Dispose(bool disposing) {
            if (!disposing) {
                // finalizer call. Most likely means Manager is null, because otherwise how
                // could there be no references if the event handlers are still registered??
                ResourceManager manager = this.Manager;
                if (manager != null) {
                    manager.ResourceAdded -= this.resourceAddedHandler;
                    manager.ResourceRemoved -= this.resourceRemovedHandler;
                    manager.ResourceRenamed -= this.resourceRenamedHandler;
                }

                return;
            }

            this.EnsureNotDispose("This resource is already disposed");
            try {
                this.isDisposing = true;
                this.SetManager(null);
                this.Disposed?.Invoke(this, EventArgs.Empty);
            }
            finally {
                this.isDisposing = false;
                this.isDisposed = true;
            }
        }

        private void EnsureNotDispose(string message = null) {
            if (this.isDisposed) {
                throw new ObjectDisposedException(this.GetType().Name, message ?? "This resource path is disposed");
            }
        }

        public bool IsCachedItemEqualTo(ResourceItem item) {
            return item is T && ReferenceEquals(this.cached, item);
        }

        public static void WriteToRBE(ResourcePath<T> resource, RBEDictionary data) {
            data.SetString(nameof(resource.UniqueId), resource.UniqueId);
        }

        public static ResourcePath<T> ReadFromRBE(ResourceManager manager, RBEDictionary data) {
            string id = data.GetString(nameof(UniqueId));
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Resource ID from the data was null, empty or whitespaces");
            return new ResourcePath<T>(manager, id);
        }
    }
}