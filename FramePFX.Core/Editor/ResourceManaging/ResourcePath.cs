using System;
using System.Diagnostics;
using FramePFX.Core.Editor.ResourceManaging.Events;
using FramePFX.Core.RBC;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.ResourceManaging {
    public abstract class ResourcePathBase : IDisposable {
        protected readonly ResourceItemEventHandler resourceAddedHandler;
        protected readonly ResourceItemEventHandler resourceRemovedHandler;
        protected readonly ResourceRenamedEventHandler resourceRenamedHandler;

        // volatile juuust in case...
        protected volatile bool isDisposing;
        protected volatile bool isDisposed;

        public bool IsDisposing => this.isDisposing;
        public bool IsDisposed => this.isDisposed;
        public bool CanDispose => !this.isDisposed && !this.isDisposing;

        public ResourceManager Manager { get; protected set; }

        public string ResourceId { get; protected set; }

        public bool? IsOnline { get; protected set; }

        /// <summary>
        /// An event called when this resource path is disposed. This is only
        /// called when explicitly disposed; finalizer does not call this event
        /// </summary>
        public event EventHandler Disposed;

        protected ResourcePathBase(ResourceManager manager, string resourceId) {
            this.ResourceId = string.IsNullOrWhiteSpace(resourceId) ? throw new ArgumentException("Unique id cannot be null, empty or whitespaces") : resourceId;
            this.resourceAddedHandler = this.OnManagerResourceAdded;
            this.resourceRemovedHandler = this.OnManagerResourceRemoved;
            this.resourceRenamedHandler = this.OnManagerResourceRenamed;
            if (manager != null) {
                this.SetManager(manager);
            }
        }

        public void SetManager(ResourceManager newManager, bool fireResourceChanged = true) {
            this.EnsureNotDispose();
            ResourceManager oldManager = this.Manager;
            if (ReferenceEquals(oldManager, newManager)) {
                if (newManager != null) {
                    Debug.WriteLine($"[{this.GetType().Name}] Attempted to set the same manager instance: {new Exception().GetToString()}");
                }

                return;
            }

            this.ClearInternalResource(fireResourceChanged);

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

        protected abstract void ClearInternalResource(bool fireResourceChanged);

        ~ResourcePathBase() {
            this.Dispose(false);
        }

        protected abstract void OnManagerResourceAdded(ResourceManager manager, ResourceItem item);

        protected abstract void OnManagerResourceRemoved(ResourceManager manager, ResourceItem item);

        protected abstract void OnManagerResourceRenamed(ResourceManager manager, ResourceItem item, string oldId, string newId);

        /// <summary>
        /// Disposes this resource path, removing all <see cref="ResourceManager"/> handlers that it has registered,
        /// then sets the manager to null which in tern sets the cached item to null (invoking the <see cref="ResourceChanged"/> event), and then
        /// finally invokes the <see cref="Disposed"/> event
        /// </summary>
        public void Dispose() {
            GC.SuppressFinalize(this);
            this.Dispose(true);
            this.Disposed?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void Dispose(bool disposing) {

        }

        protected void EnsureNotDispose(string message = null) {
            if (this.isDisposed) {
                throw new ObjectDisposedException(this.GetType().Name, message ?? "This resource path is disposed");
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

        public ResourcePath(ResourceManager manager, string resourceId) : base(manager, resourceId) {
        }

        private void SetInternalResource(ResourceItem item, bool fireEvent = true) {
            ResourceItem oldItem = this.cached;
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

        private ResourceItem GetInternalResource() {
            ResourceItem item = this.cached;
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

            if (this.ResourceId == uniqueId) {
                Debug.WriteLine($"[{this.GetType().Name}] Attempted to set the same resource ID");
                return;
            }

            this.ResourceId = uniqueId;
            if (this.IsOnline == true) {
                this.SetInternalResource(null, fireResourceChanged);
            }

            this.IsOnline = null;
        }

        public bool TryGetResource<T>(out T resource) where T : ResourceItem {
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
                    if (this.cached is T t) {
                        resource = t;
                        return true;
                    }

                    this.SetInternalResource(resource = null);
                    return false;
                default: {
                    ResourceManager manager = this.Manager;
                    if (manager != null && manager.TryGetResource(this.ResourceId, out ResourceItem res) && res is T value) {
                        this.SetInternalResource(resource = value);
                        return true;
                    }

                    this.IsOnline = false;
                    resource = null;
                    return false;
                }
            }
        }

        protected override void ClearInternalResource(bool fireResourceChanged) {
            if (this.GetInternalResource() != null) {  // lazy; let SetInternalResource throw exceptions
                this.SetInternalResource(null, fireResourceChanged);
            }
        }

        protected override void OnManagerResourceAdded(ResourceManager manager, ResourceItem item) {
            if (this.isDisposed) {
                Debug.WriteLine("RESOURCE IS DISPOSED BUT RECEIVED RESOURCE ADDED EVENT!!!!!!!!!!!!!!!!!!!!!!!");
                return;
            }

            if (item.UniqueId != this.ResourceId)
                return;
            if (this.IsOnline == true)
                throw new Exception("Expected the resource to be offline/unknown, not online");
            if (this.cached != null)
                throw new Exception("Expected the cached item to be null");
            this.SetInternalResource(item);
        }

        protected override void OnManagerResourceRemoved(ResourceManager manager, ResourceItem item) {
            if (this.isDisposed) {
                Debug.WriteLine("RESOURCE IS DISPOSED BUT RECEIVED RESOURCE REMOVED EVENT!!!!!!!!!!!!!!!!!!!!!!!");
                return;
            }

            if (item.UniqueId != this.ResourceId) {
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

        protected override void OnManagerResourceRenamed(ResourceManager manager, ResourceItem item, string oldId, string newId) {
            if (this.isDisposed) {
                Debug.WriteLine("RESOURCE IS DISPOSED BUT RECEIVED RESOURCE RENAME EVENT!!!!!!!!!!!!!!!!!!!!!!!");
                return;
            }

            if (oldId == this.ResourceId) {
                // our possibly active resource was renamed
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

                    if (item != null) {
                        this.SetInternalResource(item);
                    }
                    else if (this.cached != null) {
                        throw new Exception("Expected null cached item when resource online state is unknown");
                    }
                }

                this.ResourceId = newId;
            }
            else if (newId == this.ResourceId) {
                // a random resource was named our resource; try to use it
                if (this.IsOnline == true) {
                    throw new Exception("Expected online state to be offline or unknown, not online");
                }

                if (item != null) {
                    this.SetInternalResource(item);
                }
                else {
                    this.IsOnline = false;
                }
            }
        }

        protected override void Dispose(bool disposing) {
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
            this.isDisposing = true;
            try {
                this.SetManager(null);
            }
            finally {
                this.isDisposing = false;
                this.isDisposed = true;
            }
        }

        public bool IsCachedItemEqualTo(ResourceItem item) {
            return ReferenceEquals(this.cached, item);
        }

        public static void WriteToRBE(ResourcePath resource, RBEDictionary data) {
            data.SetString(nameof(resource.ResourceId), resource.ResourceId);
        }

        public static ResourcePath ReadFromRBE(ResourceManager manager, RBEDictionary data) {
            string id = data.GetString(nameof(ResourceId));
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Resource ID from the data was null, empty or whitespaces");
            return new ResourcePath(manager, id);
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

        public ResourcePath(ResourceManager manager, string resourceId) : base(manager, resourceId) {

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

            if (this.ResourceId == uniqueId) {
                Debug.WriteLine($"[{this.GetType().Name}] Attempted to set the same resource ID");
                return;
            }

            this.ResourceId = uniqueId;
            if (this.IsOnline == true) {
                this.SetInternalResource(null, fireResourceChanged);
            }

            this.IsOnline = null;
        }

        protected override void ClearInternalResource(bool fireResourceChanged) {
            if (this.GetInternalResource() != null) {
                // lazy; let SetInternalResource throw exceptions
                this.SetInternalResource(null, fireResourceChanged);
            }
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
                    if (manager != null && manager.TryGetResource(this.ResourceId, out ResourceItem res) && res is T value) {
                        this.SetInternalResource(resource = value);
                        return true;
                    }

                    this.IsOnline = false;
                    resource = null;
                    return false;
                }
            }
        }

        protected override void OnManagerResourceAdded(ResourceManager manager, ResourceItem item) {
            if (this.isDisposed) {
                Debug.WriteLine("RESOURCE IS DISPOSED BUT RECEIVED RESOURCE ADDED EVENT!!!!!!!!!!!!!!!!!!!!!!!");
                return;
            }

            if (item.UniqueId != this.ResourceId)
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

        protected override void OnManagerResourceRemoved(ResourceManager manager, ResourceItem item) {
            if (this.isDisposed) {
                Debug.WriteLine("RESOURCE IS DISPOSED BUT RECEIVED RESOURCE REMOVED EVENT!!!!!!!!!!!!!!!!!!!!!!!");
                return;
            }

            if (item.UniqueId != this.ResourceId) {
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

        protected override void OnManagerResourceRenamed(ResourceManager manager, ResourceItem item, string oldId, string newId) {
            if (this.isDisposed) {
                Debug.WriteLine("RESOURCE IS DISPOSED BUT RECEIVED RESOURCE RENAME EVENT!!!!!!!!!!!!!!!!!!!!!!!");
                return;
            }

            if (oldId == this.ResourceId) {
                // our possibly active resource was renamed
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

                this.ResourceId = newId;
            }
            else if (newId == this.ResourceId) {
                // a random resource was named our resource; try to use it
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

        protected override void Dispose(bool disposing) {
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
            this.isDisposing = true;
            try {
                this.SetManager(null);
            }
            finally {
                this.isDisposing = false;
                this.isDisposed = true;
            }
        }

        public bool IsCachedItemEqualTo(ResourceItem item) {
            return item is T && ReferenceEquals(this.cached, item);
        }

        public static void WriteToRBE(ResourcePath<T> resource, RBEDictionary data) {
            data.SetString(nameof(resource.ResourceId), resource.ResourceId);
        }

        public static ResourcePath<T> ReadFromRBE(ResourceManager manager, RBEDictionary data) {
            string id = data.GetString(nameof(ResourceId));
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Resource ID from the data was null, empty or whitespaces");
            return new ResourcePath<T>(manager, id);
        }
    }
}