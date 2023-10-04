using System;
using System.Diagnostics;
using FramePFX.Editor.ResourceManaging.Events;
using FramePFX.Logger;
using FramePFX.RBC;

namespace FramePFX.Editor.ResourceManaging
{
    public sealed class ResourcePath : IDisposable
    {
        private readonly ResourceItemEventHandler resourceAddedHandler;
        private readonly ResourceItemEventHandler resourceRemovedHandler;
        private readonly ResourceReplacedEventHandler resourceReplacedHandler;
        private readonly ResourceItemEventHandler onlineStateChangedHandler;

        private ResourceItem cached;
        private bool isDisposing;
        private bool isDisposed;
        private bool isManagerChanging;

        public bool IsDisposing => this.isDisposing;
        public bool IsDisposed => this.isDisposed;
        public bool CanDispose => !this.isDisposed && !this.isDisposing;

        public ResourceManager Manager { get; private set; }

        /// <summary>
        /// Gets this resource path's ID. This will not be <see cref="ResourceManager.EmptyId"/> and will therefore always be valid
        /// </summary>
        public ulong ResourceId { get; private set; }

        /// <summary>
        /// The online state of this resource. True means the state is valid and accessible. False
        /// means the state is invalid and cannot be access. Null means the resource hasn't been resolved yet or
        /// there is no manager associated with this instance
        /// </summary>
        public bool? IsValid { get; private set; }

        /// <summary>
        /// An event that gets fired when this path's internal cached resource changes, e.g. due to <see cref="TryGetResource"/> being
        /// invokes and a resource being resolved, the resource being added or removed from the manager, etc
        /// </summary>
        public event ResourceChangedEventHandler ResourceChanged;

        /// <summary>
        /// An event fired when the online state of our cached resource changes
        /// </summary>
        public event ResourceItemEventHandler OnlineStateChanged;

        public ResourcePath(ulong resourceId) : this(null, resourceId) { }

        public ResourcePath(ResourceManager manager, ulong resourceId)
        {
            this.ResourceId = resourceId == ResourceManager.EmptyId ? throw new ArgumentException("Unique id cannot be 0 (null)") : resourceId;
            this.resourceAddedHandler = this.OnManagerResourceAdded;
            this.resourceRemovedHandler = this.OnManagerResourceRemoved;
            this.resourceReplacedHandler = this.OnManagerResourceReplaced;
            this.onlineStateChangedHandler = this.OnOnlineStateChanged;
            if (manager != null)
            {
                this.Manager = manager;
                this.AttachManager(manager);
            }
        }

        public void SetManager(ResourceManager manager)
        {
            this.EnsureNotDisposed();
            this.EnsureNotDisposing();
            this.EnsureManagerNotChanging("Cannot set manager while it is already being set");
            ResourceManager oldManager = this.Manager;
            if (ReferenceEquals(oldManager, manager))
            {
                if (manager != null)
                {
                    Debugger.Break();
                    AppLogger.WriteLine($"[{this.GetType().Name}] Attempted to set the same manager instance:\n{new StackTrace(true)}");
                }

                return;
            }

            this.isManagerChanging = true;
            this.ClearInternalResource();
            if (oldManager != null)
            {
                this.DetachManager(oldManager);
            }

            this.Manager = manager;
            if (manager != null)
            {
                this.AttachManager(manager);
            }

            this.isManagerChanging = false;
        }

        private void SetInternalResource(ResourceItem item, bool fireEvent = true)
        {
            ResourceItem oldItem = this.cached;
            if (this.IsValid == true)
            {
                if (oldItem == null)
                    throw new Exception("Expected non-null cached item when state is valid");
                this.IsValid = null;
            }
            else if (oldItem != null)
            {
                throw new Exception("Expected null cached item when state is invalid or unknown");
            }

            if (ReferenceEquals(oldItem, item))
            {
                AppLogger.WriteLine($"Attempted to set resource to the same instance: {oldItem} -> {item}");
                AppLogger.WriteLine(Environment.StackTrace);
                return;
            }

            this.SetInternalResourceUnsafe(oldItem, item, fireEvent);
        }

        private void SetInternalResourceUnsafe(ResourceItem oldItem, ResourceItem newItem, bool fireEvent = true)
        {
            this.cached = newItem;
            this.IsValid = newItem != null ? (bool?) true : null;
            this.OnResourceChanged(oldItem, newItem);
            if (fireEvent)
            {
                this.ResourceChanged?.Invoke(oldItem, newItem);
            }
        }

        private void OnResourceChanged(ResourceItem oldItem, ResourceItem newItem)
        {
            if (oldItem != null)
                oldItem.OnlineStateChanged -= this.onlineStateChangedHandler;
            if (newItem != null)
                newItem.OnlineStateChanged += this.onlineStateChangedHandler;
        }

        private ResourceItem GetInternalResource()
        {
            ResourceItem item = this.cached;
            if (this.IsValid == true)
            {
                if (item == null)
                    throw new Exception("Expected non-null cached item when state is valid");
                return item;
            }

            if (item != null)
                throw new Exception("Expected null cached item when state is invalid or unknown");
            return null;
        }

        public void SetResourceId(ulong uniqueId, bool fireResourceChanged = true)
        {
            this.EnsureNotDisposed();
            if (uniqueId == ResourceManager.EmptyId)
            {
                throw new ArgumentException("Unique id cannot be 0 (null)");
            }

            if (this.ResourceId == uniqueId)
            {
                AppLogger.WriteLine($"[{this.GetType().Name}] Attempted to set the same resource ID");
                return;
            }

            this.ResourceId = uniqueId;
            if (this.IsValid == true)
            {
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
        public bool TryGetResource<T>(out T resource, bool requireIsOnline = true) where T : ResourceItem
        {
            this.EnsureNotDisposed();
            this.EnsureManagerNotChanging("Cannot attempt to get resource while manager is being set");
            switch (this.IsValid)
            {
                case false:
                {
                    if (this.cached != null)
                        throw new Exception("Expected null cached item when state is invalid");
                    resource = null;
                    return false;
                }
                case true:
                {
                    if (this.cached == null)
                        throw new Exception("Expected non-null cached item when state is valid");
                    if (this.cached is T t)
                        return (resource = t).IsOnline || !requireIsOnline;
                    this.SetInternalResourceUnsafe(this.cached, resource = null);
                    return false;
                }
                default:
                {
                    ResourceManager manager = this.Manager;
                    if (manager != null && manager.TryGetEntryItem(this.ResourceId, out ResourceItem res) && res is T t)
                    {
                        this.SetInternalResource(resource = t);
                        return t.IsOnline || !requireIsOnline;
                    }

                    this.IsValid = false;
                    resource = null;
                    return false;
                }
            }
        }

        private void ClearInternalResource(bool fireResourceChanged = true)
        {
            if (this.GetInternalResource() != null)
            {
                this.SetInternalResource(null, fireResourceChanged);
            }
        }

        private void OnManagerResourceAdded(ResourceManager manager, ResourceItem item)
        {
            if (this.isDisposed)
            {
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

        private void OnManagerResourceRemoved(ResourceManager manager, ResourceItem item)
        {
            if (this.isDisposed)
            {
                AppLogger.WriteLine("RESOURCE IS DISPOSED BUT RECEIVED RESOURCE REMOVED EVENT!!!!!!!!!!!!!!!!!!!!!!!");
                Debugger.Break();
                return;
            }

            if (item.UniqueId != this.ResourceId)
            {
                return;
            }

            if (this.IsValid == true)
            {
                if (this.cached == null)
                    throw new Exception("Expected our cached item to not be null");
                if (!ReferenceEquals(this.cached, item))
                    throw new Exception("Expected the cached item to equal the removed item");
            }

            this.SetInternalResource(null);
        }

        private void OnManagerResourceReplaced(ResourceManager manager, ulong id, ResourceItem oldItem, ResourceItem newItem)
        {
            if (this.isDisposed)
            {
                AppLogger.WriteLine("RESOURCE IS DISPOSED BUT RECEIVED RESOURCE REPLACED EVENT!!!!!!!!!!!!!!!!!!!!!!!");
                Debugger.Break();
                return;
            }

            if (id != this.ResourceId)
            {
                return;
            }

            if (this.IsValid == true)
            {
                if (this.cached == null)
                    throw new Exception("Expected our cached item to not be null");
                if (!ReferenceEquals(this.cached, oldItem))
                    throw new Exception("Expected the cached item to equal the new item");
            }

            this.SetInternalResource(newItem);
        }

        private void OnOnlineStateChanged(ResourceManager manager, ResourceItem item)
        {
            this.OnlineStateChanged?.Invoke(manager, item);
        }

        public bool IsCachedItemEqualTo(ResourceItem item)
        {
            return ReferenceEquals(this.cached, item);
        }

        private void AttachManager(ResourceManager manager)
        {
            manager.ResourceAdded += this.resourceAddedHandler;
            manager.ResourceRemoved += this.resourceRemovedHandler;
            manager.ResourceReplaced += this.resourceReplacedHandler;
        }

        private void DetachManager(ResourceManager manager)
        {
            manager.ResourceAdded -= this.resourceAddedHandler;
            manager.ResourceRemoved -= this.resourceRemovedHandler;
            manager.ResourceReplaced -= this.resourceReplacedHandler;
        }

        /// <summary>
        /// Disposes this resource path. This first clears the resource (causing the <see cref="ResourceChanged"/>
        /// event to be fired), removes the <see cref="ResourceManager"/> handlers and finally sets the manager
        /// to null
        /// </summary>
        public void Dispose()
        {
            this.isDisposing = true;
            this.ClearInternalResource();
            ResourceManager manager = this.Manager;
            if (manager != null)
            {
                this.Manager = null;
                this.DetachManager(manager);
            }

            this.isDisposed = true;
            this.isDisposing = false;
        }

        private void EnsureNotDisposed(string message = null)
        {
            if (this.isDisposed)
                throw new ObjectDisposedException(this.GetType().Name, message ?? "This resource path is disposed");
        }

        private void EnsureNotDisposing(string message = null)
        {
            if (this.isDisposing)
                throw new ObjectDisposedException(this.GetType().Name, message ?? "This resource path is being disposed");
        }

        private void EnsureManagerNotChanging(string message)
        {
            if (this.isManagerChanging)
                throw new InvalidOperationException(message);
        }

        #region Serialisation

        public static void WriteToRBE(ResourcePath resource, RBEDictionary data)
        {
            data.SetULong(nameof(resource.ResourceId), resource.ResourceId);
        }

        public static ResourcePath ReadFromRBE(RBEDictionary data)
        {
            ulong id = data.GetULong(nameof(ResourceId));
            if (id == ResourceManager.EmptyId)
                throw new ArgumentException("Resource ID from the data was 0 (null)");
            return new ResourcePath(id);
        }

        #endregion
    }
}