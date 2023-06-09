using System;
using System.Diagnostics;
using FramePFX.Core.Editor.ResourceManaging.Events;
using FramePFX.Core.RBC;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.ResourceManaging {
    /// <summary>
    /// Base class the non-generic and generic resource path classes
    /// </summary>
    public abstract class ResourcePathBase : IDisposable {
        protected readonly ResourceItemEventHandler resourceAddedHandler;
        protected readonly ResourceItemEventHandler resourceRemovedHandler;
        protected readonly ResourceRenamedEventHandler resourceRenamedHandler;
        protected readonly ResourceReplacedEventHandler resourceReplacedHandler;
        protected readonly ResourceItemEventHandler onlineStateChangedHandler;

        // volatile juuust in case...
        protected volatile bool isDisposing;
        protected volatile bool isDisposed;

        public bool IsDisposing => this.isDisposing;
        public bool IsDisposed => this.isDisposed;
        public bool CanDispose => !this.isDisposed && !this.isDisposing;

        public ResourceManager Manager { get; protected set; }

        public string ResourceId { get; protected set; }

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

        protected ResourcePathBase(ResourceManager manager, string resourceId) {
            this.ResourceId = string.IsNullOrWhiteSpace(resourceId) ? throw new ArgumentException("Unique id cannot be null, empty or whitespaces") : resourceId;
            this.resourceAddedHandler = this.OnManagerResourceAdded;
            this.resourceRemovedHandler = this.OnManagerResourceRemoved;
            this.resourceRenamedHandler = this.OnManagerResourceRenamed;
            this.resourceReplacedHandler = this.OnManagerResourceReplaced;
            this.onlineStateChangedHandler = this.OnOnlineStateChanged;
            if (manager != null) {
                this.SetManager(manager);
            }
        }

        ~ResourcePathBase() {
            this.Dispose(false);
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
                oldManager.ResourceDeleted -= this.resourceRemovedHandler;
                oldManager.ResourceRenamed -= this.resourceRenamedHandler;
                oldManager.ResourceReplaced -= this.resourceReplacedHandler;
            }

            if (newManager != null) {
                newManager.ResourceAdded += this.resourceAddedHandler;
                newManager.ResourceDeleted += this.resourceRemovedHandler;
                newManager.ResourceRenamed += this.resourceRenamedHandler;
                newManager.ResourceReplaced += this.resourceReplacedHandler;
            }
        }

        protected abstract void OnManagerResourceAdded(ResourceManager manager, ResourceItem item);

        protected abstract void OnManagerResourceRemoved(ResourceManager manager, ResourceItem item);

        protected abstract void OnManagerResourceRenamed(ResourceManager manager, ResourceItem item, string oldId, string newId);

        protected abstract void OnManagerResourceReplaced(ResourceManager manager, string id, ResourceItem oldItem, ResourceItem newItem);

        protected virtual void OnOnlineStateChanged(ResourceManager manager, ResourceItem item) {
            this.OnlineStateChanged?.Invoke(manager, item);
        }

        protected abstract void ClearInternalResource(bool fireResourceChanged);

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
            GC.SuppressFinalize(this);
            this.Dispose(true);
            this.Disposed?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void Dispose(bool disposing) {
            // finalizer call. Most likely means Manager is null, because otherwise how
            // could there be no references if the event handlers are still registered??
            ResourceManager manager = this.Manager;
            if (manager != null) {
                manager.ResourceAdded -= this.resourceAddedHandler;
                manager.ResourceDeleted -= this.resourceRemovedHandler;
                manager.ResourceRenamed -= this.resourceRenamedHandler;
                manager.ResourceReplaced -= this.resourceReplacedHandler;
            }
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

            this.cached = item;
            this.IsValid = item != null ? (bool?) true : null;
            this.OnResourceChanged(oldItem, item);
            if (fireEvent) {
                this.ResourceChanged?.Invoke(oldItem, item);
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
            if (this.IsValid == true) {
                this.SetInternalResource(null, fireResourceChanged);
            }

            this.IsValid = null;
        }

        public bool TryGetResource<T>(out T resource, bool requireIsOnline = true) where T : ResourceItem {
            this.EnsureNotDispose();
            switch (this.IsValid) {
                case false:
                    if (this.cached != null)
                        throw new Exception("Expected null cached item when state is invalid");
                    resource = null;
                    return false;
                case true:
                    if (this.cached == null)
                        throw new Exception("Expected non-null cached item when state is valid");
                    if (this.cached is T t) {
                        resource = t;
                        return !requireIsOnline || t.IsOnline;
                    }

                    this.SetInternalResource(resource = null);
                    return false;
                default: {
                    ResourceManager manager = this.Manager;
                    if (manager != null && manager.TryGetResource(this.ResourceId, out ResourceItem res) && res is T value) {
                        this.SetInternalResource(resource = value);
                        return !requireIsOnline || value.IsOnline;
                    }

                    this.IsValid = false;
                    resource = null;
                    return false;
                }
            }
        }

        protected override void ClearInternalResource(bool fireResourceChanged) {
            if (this.GetInternalResource() != null) {
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
            if (this.IsValid == true)
                throw new Exception("Expected the state to be invalid or unknown, not valid");
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

            if (this.IsValid == true) {
                if (this.cached == null)
                    throw new Exception("Expected our cached item to not be null");
                if (!ReferenceEquals(this.cached, item))
                    throw new Exception("Expected the cached item to equal the removed item");
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
                if (this.IsValid == true) {
                    if (this.cached == null)
                        throw new Exception("Expected our cached item to not be null");
                    if (!ReferenceEquals(this.cached, item))
                        throw new Exception("Expected the cached item to equal the removed item");
                }
                else {
                    if (this.IsValid != null) {
                        throw new Exception("Expected state to be unknown, not invalid");
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
                if (this.IsValid == true) {
                    throw new Exception("Expected state to be invalid or unknown, not online");
                }

                if (item != null) {
                    this.SetInternalResource(item);
                }
                else {
                    this.IsValid = false;
                }
            }
        }

        protected override void OnManagerResourceReplaced(ResourceManager manager, string id, ResourceItem oldItem, ResourceItem newItem) {
            if (this.isDisposed) {
                Debug.WriteLine("RESOURCE IS DISPOSED BUT RECEIVED RESOURCE REPLACED EVENT!!!!!!!!!!!!!!!!!!!!!!!");
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

        protected override void Dispose(bool disposing) {
            if (!disposing) {
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

            this.cached = item;
            this.IsValid = item != null ? (bool?) true : null;
            this.OnResourceChanged(oldItem, item);
            if (fireEvent) {
                this.ResourceChanged?.Invoke(oldItem, item);
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
            if (this.IsValid == true) {
                this.SetInternalResource(null, fireResourceChanged);
            }

            this.IsValid = null;
        }

        protected override void ClearInternalResource(bool fireResourceChanged) {
            if (this.GetInternalResource() != null) {
                // lazy; let SetInternalResource throw exceptions
                this.SetInternalResource(null, fireResourceChanged);
            }
        }

        public bool TryGetResource(out T resource, bool requireIsOnline = true) {
            this.EnsureNotDispose();
            switch (this.IsValid) {
                case false:
                    if (this.cached != null)
                        throw new Exception("Expected null cached item when state is invalid");
                    resource = null;
                    return false;
                case true:
                    if (this.cached == null)
                        throw new Exception("Expected non-null cached item when state is valid");
                    resource = this.cached;
                    return !requireIsOnline || resource.IsOnline;
                default: {
                    ResourceManager manager = this.Manager;
                    if (manager != null && manager.TryGetResource(this.ResourceId, out ResourceItem res) && res is T value) {
                        this.SetInternalResource(resource = value);
                        return !requireIsOnline || resource.IsOnline;
                    }

                    this.IsValid = false;
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
                Debug.WriteLine("RESOURCE IS DISPOSED BUT RECEIVED RESOURCE REMOVED EVENT!!!!!!!!!!!!!!!!!!!!!!!");
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

        protected override void OnManagerResourceRenamed(ResourceManager manager, ResourceItem item, string oldId, string newId) {
            if (this.isDisposed) {
                Debug.WriteLine("RESOURCE IS DISPOSED BUT RECEIVED RESOURCE RENAME EVENT!!!!!!!!!!!!!!!!!!!!!!!");
                return;
            }

            if (oldId == this.ResourceId) {
                // our possibly active resource was renamed
                if (this.IsValid == true) {
                    if (this.cached == null)
                        throw new Exception("Expected our cached item to not be null");
                    if (!ReferenceEquals(this.cached, item))
                        throw new Exception("Expected the cached item to equal the removed item");
                }
                else {
                    if (this.IsValid != null) {
                        throw new Exception("Expected state to be unknown, not invalid");
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
                if (this.IsValid == true) {
                    throw new Exception("Expected state to be invalid or unknown, not online");
                }

                if (item is T value) {
                    this.SetInternalResource(value);
                }
                else {
                    this.IsValid = false;
                }
            }
        }

        protected override void OnManagerResourceReplaced(ResourceManager manager, string id, ResourceItem oldItem, ResourceItem newItem) {
            if (this.isDisposed) {
                Debug.WriteLine("RESOURCE IS DISPOSED BUT RECEIVED RESOURCE REPLACED EVENT!!!!!!!!!!!!!!!!!!!!!!!");
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

        protected override void Dispose(bool disposing) {
            if (!disposing) {
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