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
using FramePFX.Editors.ResourceManaging.Autoloading;
using FramePFX.Editors.ResourceManaging.Events;

namespace FramePFX.Editors.ResourceManaging {
    /// <summary>
    /// The base class for all resource items that can be used by clip objects to store and access
    /// data shareable across multiple clips.
    /// <para>
    /// Resource items have an online status, which can be used to for example reduce memory usage when
    /// resources aren't in use, such as Image and AVMedia resources.
    /// While a resource can force itself online via <see cref="EnableCore"/>, the general idea is that
    /// resources can load themselves based on their state, and if there was a problem or missing data,
    /// they add error entries called <see cref="InvalidResourceEntry"/> objects which get added to
    /// a <see cref="ResourceLoader"/> which is used to present a collection of errors to the user
    /// </para>
    /// </summary>
    public abstract class ResourceItem : BaseResource {
        public const ulong EmptyId = ResourceManager.EmptyId;
        protected bool doNotProcessUniqueIdForSerialisation;

        /// <summary>
        /// Gets if this resource is online (usable) or offline (not usable by clips)
        /// </summary>
        public bool IsOnline { get; private set; }

        /// <summary>
        /// Gets or sets if the reason the resource is offline is because a user forced it offline
        /// </summary>
        public bool IsOfflineByUser { get; set; }

        /// <summary>
        /// This resource item's current unique identifier. This is only set by our <see cref="ResourceManager"/> (to
        /// a valid value when registering, or <see cref="EmptyId"/> if unregistering) or during the deserialisation
        /// phase of this resource
        /// </summary>
        public ulong UniqueId { get; private set; }

        /// <summary>
        /// An event fired when our <see cref="IsOnline"/> property changes. <see cref="IsOfflineByUser"/> may have changed too
        /// </summary>
        public event ResourceItemEventHandler OnlineStateChanged;

        protected ResourceItem() {
        }

        protected internal override void OnAttachedToManager() {
            base.OnAttachedToManager();
            ResourceManager.InternalOnResourceItemAttachedToManager(this);
        }

        protected internal override void OnDetatchedFromManager() {
            base.OnDetatchedFromManager();
            ResourceManager.InternalOnResourceItemDetatchedFromManager(this);
        }

        public bool IsRegistered() {
            return this.Manager != null && this.UniqueId != EmptyId;
        }

        /// <summary>
        /// A method that forces this resource offline, releasing any resources in the process
        /// </summary>
        /// <param name="user">
        /// Whether or not this resource was disabled by the user force disabling
        /// the item. <see cref="IsOfflineByUser"/> is set to this parameter
        /// </param>
        /// <returns></returns>
        public void Disable(bool user) {
            if (!this.IsOnline) {
                return;
            }

            this.OnDisableCore(user);
            this.IsOnline = false;
            this.IsOfflineByUser = user;
            this.OnOnlineStateChanged();
        }

        /// <summary>
        /// Called by <see cref="Disable"/> when this resource item is about to be disabled. This can
        /// do things like release file locks/handles, unload bitmaps to reduce memory usage, etc.
        /// <para>
        /// Any errors should just be logged to the console
        /// </para>
        /// </summary>
        /// <param name="user">
        /// True if this was disabled forcefully by the user via the UI. False if it was disabled by something
        /// else, such as an error or this resource being deleted
        /// </param>
        protected virtual void OnDisableCore(bool user) {
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
        public bool TryAutoEnable(ResourceLoader loader) {
            if (this.IsOnline) {
                return true;
            }

            if (this.OnTryAutoEnable(loader)) {
                this.EnableCore();
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
        protected virtual bool OnTryAutoEnable(ResourceLoader loader) {
            return true;
        }

        /// <summary>
        /// Called from a resource loader to try and load this resource from an entry this resource created.
        /// This method calls <see cref="EnableCore"/>, which enables this resource. Overriding methods
        /// should not call the base method if they could not be loaded from the given entry.
        /// <para>
        /// If <see cref="OnTryAutoEnable"/> added multiple entries, then you must implement your own way
        /// of identifying which entry is which (e.g. an ID property). Typically, you would just check
        /// the entry type and process it accordingly
        /// </para>
        /// </summary>
        /// <param name="entry">The entry that we created</param>
        /// <returns>True if the resource was successfully enabled, otherwise false</returns>
        public virtual bool TryEnableForLoaderEntry(InvalidResourceEntry entry) {
            this.EnableCore();
            return true;
        }

        /// <summary>
        /// Forcefully enables this resource item, if the <see cref="TryAutoEnable"/> method is unnecessary,
        /// e.g. because resources were loaded in a non-standard or direct way
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        protected void EnableCore() {
            if (this.IsOnline) {
                throw new InvalidOperationException("Already enabled");
            }

            this.IsOnline = true;
            this.IsOfflineByUser = false;
            this.OnOnlineStateChanged();
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

        public virtual void OnOnlineStateChanged() {
            this.OnlineStateChanged?.Invoke(this);
        }

        public override void Destroy() {
            if (this.IsOnline) {
                this.Disable(false);
            }

            base.Destroy();
        }

        /// <summary>
        /// Internal method for setting a resource item's unique ID
        /// </summary>
        internal static void SetUniqueId(ResourceItem item, ulong id) => item.UniqueId = id;
    }
}