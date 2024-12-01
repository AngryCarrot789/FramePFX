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

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using FramePFX.Editing.ResourceManaging.Events;
using FramePFX.Utils.RBC;

namespace FramePFX.Editing.ResourceManaging.ResourceHelpers;

/// <summary>
/// Represents a link/reference to a specific resource, identified by its unique ID. This class maintains a reference counter
/// for the associated resources and provides events for when the resource changes, is replaced, removed and/or added back.
/// <para>
/// This class makes managing a resource much simpler, though it still needs a bit of work done I think, as
/// it's very easy to accidentally create a loading loop (and therefore a crash)
/// </para>
/// </summary>
public sealed class ResourceLink : IDisposable {
    private readonly ResourceAndManagerEventHandler resourceAddedHandler;
    private readonly ResourceAndManagerEventHandler resourceRemovedHandler;
    private readonly ResourceItemEventHandler onlineStateChangedHandler;

    private ResourceItem? cached;
    private bool isDisposing; // are we currently disposing?
    private bool isDisposed; // are we disposed and cannot be used again?
    private bool isManagerChanging; // is the manager being changed? used for fail-fast exceptions
    private bool isReferencedCounted; // is this instance counted as a reference to our resource?

    public bool IsDisposing => this.isDisposing;
    public bool IsDisposed => this.isDisposed;
    public bool CanDispose => !this.isDisposed && !this.isDisposing;

    /// <summary>
    /// This resource path's manager, which is used to query resources from and listen to events
    /// </summary>
    public ResourceManager? Manager { get; private set; }

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
    public LinkState State { get; private set; }

    /// <summary>
    /// An event that gets fired when this path's resource changes. This can be due to the resource being deleted or
    /// added back (after deletion), <see cref="TryGetResource"/> being invoked and a resource being resolved, etc.
    /// </summary>
    public event ResourceChangedEventHandler? ResourceChanged;

    /// <summary>
    /// An event fired when the online state of our resource changes (as in, when <see cref="ResourceItem.IsOnline"/> changes)
    /// </summary>
    public event ResourceEventHandler? OnlineStateChanged;

    /// <summary>
    /// Gets the resource path key that owns this resource path
    /// </summary>
    public IBaseResourcePathKey Owner { get; }

    public ResourceLink(IBaseResourcePathKey owner, ulong resourceId) {
        this.ResourceId = resourceId == ResourceManager.EmptyId ? throw new ArgumentException("Unique id cannot be 0 (null)") : resourceId;
        this.Owner = owner ?? throw new ArgumentNullException(nameof(owner));
        this.resourceAddedHandler = this.OnManagerResourceAdded;
        this.resourceRemovedHandler = this.OnManagerResourceRemoved;
        this.onlineStateChangedHandler = this.OnOnlineStateChanged;
    }

    public bool IsItemApplicable(ResourceItem item) => this.Owner.IsItemTypeApplicable(item);

    /// <summary>
    /// Clears this resource's internal cached object and then sets/replaces the <see cref="ResourceManager"/>
    /// </summary>
    /// <param name="manager"></param>
    public void SetManager(ResourceManager? manager) {
        this.EnsureNotDisposed();
        this.EnsureNotDisposing();
        this.EnsureManagerNotChanging("Cannot set manager while it is already being set");
        ResourceManager? oldManager = this.Manager;
        if (ReferenceEquals(oldManager, manager)) {
            return;
        }

        this.isManagerChanging = true;

        if (oldManager != null)
            this.DetachManager(oldManager);

        this.ClearInternalResource();

        this.Manager = manager;
        if (manager != null)
            this.AttachManager(manager);

        this.isManagerChanging = false;
    }

    private void SetInternalResource(ResourceItem? newItem) {
        ResourceItem? oldItem = this.cached;
        if (newItem == null) {
            if (oldItem == null)
                return;
            this.cached = null;
            this.State = LinkState.NotLinked;
        }
        else {
            if (ReferenceEquals(oldItem, newItem))
                return;
            this.cached = newItem;
            this.State = LinkState.Linked;
        }

        this.OnResourceChanged(oldItem, newItem);
    }

    private void OnResourceChanged(ResourceItem? oldItem, ResourceItem? newItem) {
        if (oldItem != null) {
            oldItem.OnlineStateChanged -= this.onlineStateChangedHandler;
            this.SetReferenceCount(oldItem, false);
        }

        if (newItem != null) {
            newItem.OnlineStateChanged += this.onlineStateChangedHandler;
            this.SetReferenceCount(newItem, true);
        }

        this.ResourceChanged?.Invoke(oldItem, newItem);
    }

    public void SetReferenceCount(ResourceItem item, bool isReferenced) {
        if (isReferenced) {
            if (!this.isReferencedCounted) {
                ResourceItem.AddReference(item, this.Owner.ResourceHelper.Owner);
                this.isReferencedCounted = true;
            }
        }
        else if (this.isReferencedCounted) {
            ResourceItem.RemoveReference(item, this.Owner.ResourceHelper.Owner);
            this.isReferencedCounted = false;
        }
    }

    public void SetResourceId(ulong uniqueId, bool autoLink = false) {
        this.EnsureNotDisposed("Cannot set resource ID on a disposed resource link");
        this.EnsureNotDisposing("Cannot set resource ID while disposing this resource link");
        if (this.ResourceId != uniqueId) {
            this.ClearInternalResource();
            this.ResourceId = uniqueId;
            if (autoLink) {
                this.LinkResource();
            }
            else {
                this.State = LinkState.NotLinked;
            }
        }
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
    public bool TryGetResource<T>([NotNullWhen(true)] out T? resource, bool requireIsOnline = true) where T : ResourceItem {
        if (!this.TryGetResource(out ResourceItem? item, requireIsOnline) || (resource = item as T) == null)
            resource = null;
        return resource != null;
    }

    public bool TryGetResource([NotNullWhen(true)] out ResourceItem? resource, bool requireIsOnline = true) {
        bool success = this.LinkResource(requireIsOnline);
        resource = success ? this.cached : null;
        return success;
    }

    /// <summary>
    /// Attempts to link to an actual resource object, using our <see cref="Manager"/> and <see cref="ResourceId"/>
    /// </summary>
    /// <param name="requireIsOnline">A filter applied to the return value</param>
    /// <returns>True when a resource was found with an acceptable type, and its online state matches the requireIsOnline parameter, otherwise false</returns>
    /// <exception cref="Exception">Invalid object state</exception>
    public bool LinkResource(bool requireIsOnline = true) {
        this.EnsureNotDisposed();
        this.EnsureManagerNotChanging("Cannot attempt to get resource while manager is being set");
        switch (this.State) {
            case LinkState.Linked:
                // assert: this.cached != null
                return this.cached!.IsOnline || !requireIsOnline;
            case LinkState.NotLinked: {
                // assert: this.cached == null
                if (this.Manager != null && this.Manager.TryGetEntryItem(this.ResourceId, out ResourceItem resource)) {
                    if (this.IsItemApplicable(resource)) {
                        this.SetInternalResource(resource);
                        return resource.IsOnline || !requireIsOnline;
                    }
                    else {
                        this.State = LinkState.IncompatibleResource;
                    }
                }
                else {
                    this.State = LinkState.NoSuchResource;
                }

                return false;
            }
            default:
                // assert: this.cached == null
                return false;
        }
    }

    private void ClearInternalResource() {
        if (this.State == LinkState.Linked) {
            this.SetInternalResource(null);
        }
    }

    private void OnManagerResourceAdded(ResourceManager manager, ResourceItem item) {
        if (this.isDisposed) {
            // AppLogger.Instance.WriteLine("RESOURCE IS DISPOSED BUT RECEIVED RESOURCE ADDED EVENT!!!!!!!!!!!!!!!!!!!!!!!");
            Debugger.Break();
            return;
        }

        if (item.UniqueId != this.ResourceId)
            return;
        if (this.State == LinkState.Linked)
            throw new Exception("Expected the state to be invalid or unknown, not valid");
        this.SetInternalResource(item);
    }

    private void OnManagerResourceRemoved(ResourceManager manager, ResourceItem item) {
        if (item.UniqueId != this.ResourceId) {
            return;
        }

        this.SetInternalResource(null);
    }

    private void OnOnlineStateChanged(ResourceItem item) {
        this.OnlineStateChanged?.Invoke(item);
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
        ResourceManager? manager = this.Manager;
        if (manager != null) {
            this.Manager = null;
            this.DetachManager(manager);
        }

        this.isDisposed = true;
        this.isDisposing = false;
    }

    private void EnsureNotDisposed(string? message = null) {
        if (this.isDisposed)
            throw new ObjectDisposedException(this.GetType().Name, message ?? "This resource path is disposed");
    }

    private void EnsureNotDisposing(string? message = null) {
        if (this.isDisposing)
            throw new ObjectDisposedException(this.GetType().Name, message ?? "This resource path is being disposed");
    }

    private void EnsureManagerNotChanging(string message) {
        if (this.isManagerChanging)
            throw new InvalidOperationException(message);
    }

    #region Serialisation

    public static void WriteToRBE(ResourceLink resource, RBEDictionary data) {
        data.SetULong(nameof(resource.ResourceId), resource.ResourceId);
    }

    public static ResourceLink ReadFromRBE(IBaseResourcePathKey owner, RBEDictionary data) {
        ulong id = data.GetULong(nameof(ResourceId));
        if (id == ResourceManager.EmptyId)
            throw new ArgumentException("Resource ID from the data was 0 (null)");
        return new ResourceLink(owner, id);
    }

    #endregion
}