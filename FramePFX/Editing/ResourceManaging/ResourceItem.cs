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

using FramePFX.Editing.ResourceManaging.Autoloading;
using FramePFX.Editing.ResourceManaging.Events;
using FramePFX.Editing.ResourceManaging.NewResourceHelper;
using PFXToolKitUI.DataTransfer;

namespace FramePFX.Editing.ResourceManaging;

/// <summary>
/// The base class for all resource items that can be used by clip objects to store and access
/// data shareable across multiple clips.
/// <para>
/// Resource items have an online status, which can be used to for example reduce memory usage when
/// resources aren't in use, such as Image and AVMedia resources.
/// While a resource can force itself online via <see cref="Enable"/>, the general idea is that
/// resources can load themselves based on their state, and if there was a problem or missing data,
/// they add error entries called <see cref="InvalidResourceEntry"/> objects which get added to
/// a <see cref="ResourceLoader"/> which is used to present a collection of errors to the user
/// </para>
/// </summary>
public abstract class ResourceItem : BaseResource, ITransferableData {
    public const ulong EmptyId = ResourceManager.EmptyId;
    protected bool doNotProcessUniqueIdForSerialisation;
    private readonly List<IResourceHolder> references;

    /// <summary>
    /// Gets if this resource is online (usable) or offline (not usable by clips)
    /// </summary>
    public bool IsOnline { get; private set; }

    /// <summary>
    /// Gets or sets if the reason the resource is offline is that a user forced it offline
    /// </summary>
    public bool IsOfflineByUser { get; set; }

    /// <summary>
    /// This resource item's current unique identifier. This is only set by our <see cref="ResourceManager"/> (to
    /// a valid value when registering, or <see cref="EmptyId"/> if unregistering) or during the deserialisation
    /// phase of this resource
    /// </summary>
    public ulong UniqueId { get; private set; }

    /// <summary>
    /// Returns a read-only list of <see cref="IResourceHolder"/> objects that have referenced
    /// this resource (active resource link in <see cref="ResourceLink"/>)
    /// </summary>
    public IReadOnlyList<IResourceHolder> References => this.references;

    // TODO: this needs work, but it's a start. There needs to be a proper way of handling resource link limits,
    // and I need to get rid of the ResourceLink crap, since it's just gonna lead to problems implementing this

    /// <summary>
    /// Returns a value which represents the maximum number of references this resource can be linked to. -1 means unlimited
    /// </summary>
    public virtual int ResourceLinkLimit => -1;

    public TransferableData TransferableData { get; }

    /// <summary>
    /// An event fired when our <see cref="IsOnline"/> property changes. <see cref="IsOfflineByUser"/> may have changed too
    /// </summary>
    public event ResourceItemEventHandler? OnlineStateChanged;

    protected ResourceItem() {
        this.references = new List<IResourceHolder>();
        this.TransferableData = new TransferableData(this);
    }

    public bool HasReachedResourceLimit() {
        int limit = this.ResourceLinkLimit;
        return limit != -1 && this.references.Count >= limit;
    }

    protected internal override void OnAttachedToManager() {
        base.OnAttachedToManager();
        ResourceManager.InternalOnResourceItemAttachedToManager(this);
    }

    protected internal override void OnDetachedFromManager() {
        base.OnDetachedFromManager();
        ResourceManager.InternalOnResourceItemDetachedFromManager(this);
    }

    public bool IsRegistered() {
        return this.Manager != null && this.UniqueId != EmptyId;
    }

    /// <summary>
    /// A method that forces this resource offline, releasing any resources in the process
    /// </summary>
    /// <param name="user">
    /// Whether this resource was disabled by the user force-disabling
    /// the item. <see cref="IsOfflineByUser"/> is set to this parameter
    /// </param>
    /// <returns></returns>
    public void Disable(bool user) {
        if (this.IsOnline) {
            this.IsOnline = false;
            this.IsOfflineByUser = user;
            this.OnDisabled();
            this.RaiseOnlineStateChanged();
        }
    }

    /// <summary>
    /// Called by <see cref="Disable"/> when this resource item is about to be disabled. This can
    /// do things like release file locks/handles, unload bitmaps to reduce memory usage, etc.
    /// <para>
    /// Any errors should just be logged to the console
    /// </para>
    /// </summary>
    protected virtual void OnDisabled() {
    }

    /// <summary>
    /// Tries to enable this resource based on its current state. If there were errors, they
    /// will be added to the given <see cref="ResourceLoader"/> (if it is non-null).
    /// If already online, then nothing happens and true is returned
    /// </summary>
    /// <param name="loader">
    /// The loader in which error entries can be added to which can be used by the user to
    /// fix this resource. May be null, in which case, errors are ignored
    /// </param>
    /// <returns>
    /// True if the resource is already online or is now online, or false meaning the resource could not enable itself
    /// </returns>
    public async ValueTask<bool> TryAutoEnable(ResourceLoader? loader) {
        if (this.IsOnline) {
            return true;
        }

        if (await this.OnTryAutoEnable(loader)) {
            this.Enable();
            return true;
        }
        else {
            return false;
        }
    }

    /// <summary>
    /// Called by <see cref="TryAutoEnable"/> to try and enable this resource based on its current state.
    /// This could load any external things, open files, etc. ready for clips to use them. If that stuff
    /// is already loaded, then this method can return true to signal to put this resource into an online
    /// state, as this method is only called when offline
    /// <para>
    /// If there are errors, then add entries to the resource loader (if it is non-null)
    /// </para>
    /// </summary>
    /// <param name="loader">The loader to add error entries to. May be null if errors are not processed</param>
    protected virtual ValueTask<bool> OnTryAutoEnable(ResourceLoader? loader) {
        return ValueTask.FromResult(true);
    }

    /// <summary>
    /// Called from a resource loader to try and load this resource from an entry this resource created.
    /// This method calls <see cref="Enable"/>, which enables this resource. Overriding methods
    /// should not call the base method if we could not become loaded/enabled from the given entry.
    /// <para>
    /// If <see cref="OnTryAutoEnable"/> added multiple entries, then you must implement your own way
    /// of identifying which entry is which (e.g. an ID property). Typically, you would just check
    /// the entry type and process it accordingly
    /// </para>
    /// </summary>
    /// <param name="entry">The entry that we created</param>
    /// <returns>True if the resource was successfully enabled, otherwise false</returns>
    public virtual ValueTask<bool> TryEnableForLoaderEntry(InvalidResourceEntry entry) {
        this.Enable();
        return ValueTask.FromResult(true);
    }

    /// <summary>
    /// Forcefully enables this resource item, if the <see cref="TryAutoEnable"/> method is
    /// unnecessary, e.g. because resources were loaded in a non-standard or direct way.
    /// <para>
    /// This method is protected because if something loads this resource 'for fun', it could cause
    /// crashing or hard to track bugs (maybe this resource requires a file path to be enabled)
    /// </para>
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Already enabled. Check <see cref="IsOnline"/> first
    /// </exception>
    protected void Enable() {
        if (this.IsOnline) {
            throw new InvalidOperationException("Already enabled");
        }

        this.IsOnline = true;
        this.IsOfflineByUser = false;
        this.RaiseOnlineStateChanged();
    }

    static ResourceItem() {
        SerialisationRegistry.Register<ResourceItem>(0, (resource, data, ctx) => {
            ctx.DeserialiseBaseType(data);
            if (!resource.doNotProcessUniqueIdForSerialisation)
                resource.UniqueId = data.GetULong(nameof(resource.UniqueId), EmptyId);
            if (data.TryGetBool(nameof(resource.IsOnline), out bool isOnline) && !isOnline) {
                resource.IsOnline = false;
                resource.IsOfflineByUser = true;
            }
        }, (resource, data, ctx) => {
            ctx.SerialiseBaseType(data);
            if (resource.UniqueId != EmptyId && !resource.doNotProcessUniqueIdForSerialisation)
                data.SetULong(nameof(resource.UniqueId), resource.UniqueId);
            if (!resource.IsOnline)
                data.SetBool(nameof(resource.IsOnline), false);
        });
    }

    private void RaiseOnlineStateChanged() {
        this.OnOnlineStateChanged();
        this.OnlineStateChanged?.Invoke(this);
    }

    /// <summary>
    /// Invoked when our <see cref="IsOnline"/> state changes. This is called before the <see cref="OnlineStateChanged"/> event
    /// </summary>
    protected virtual void OnOnlineStateChanged() {
    }

    public override void Destroy() {
        if (this.IsOnline)
            this.Disable(false);

        base.Destroy();
    }

    /// <summary>
    /// Internal method for setting a resource item's unique ID
    /// </summary>
    internal static void SetUniqueId(ResourceItem item, ulong id) => item.UniqueId = id;

    internal static void AddReference(ResourceItem item, IResourceHolder owner) {
        if (item.references.Contains(owner))
            throw new InvalidOperationException("Object already referenced");
        if (item.HasReachedResourceLimit())
            throw new InvalidOperationException("Resource limit reached: cannot reference more than" + item.ResourceLinkLimit);
        item.references.Add(owner);
    }

    internal static void RemoveReference(ResourceItem item, IResourceHolder owner) {
        if (!item.references.Remove(owner))
            throw new InvalidOperationException("Object was not referenced");
    }
}