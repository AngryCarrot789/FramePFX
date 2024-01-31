using System;
using FramePFX.Editors.ResourceManaging.Autoloading;
using FramePFX.Editors.ResourceManaging.Events;
using FramePFX.RBC;

namespace FramePFX.Editors.ResourceManaging {
    /// <summary>
    /// The base class for all resource items that can be used by clip objects to store and access
    /// data shareable across multiple clips
    /// </summary>
    public abstract class ResourceItem : BaseResource {
        public const ulong EmptyId = ResourceManager.EmptyId;

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
        /// phase of this resource, in which case a special registration will occur that uses this property directly
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
            ResourceManager.InternalRegister(this);
        }

        protected internal override void OnDetatchedFromManager() {
            base.OnDetatchedFromManager();
            ResourceManager.InternalUnregister(this);
        }

        public bool IsRegistered() {
            return this.Manager != null && this.UniqueId != EmptyId;
        }

        /// <summary>
        /// A method that forces this resource offline, releasing any resources in the process. This may call <see cref="Dispose"/> or <see cref="DisposeCore"/>
        /// </summary>
        /// <param name="user">
        /// Whether or not this resource was disabled by the user force disabling
        /// the item. <see cref="IsOfflineByUser"/> is set to this parameter
        /// </param>
        /// <returns></returns>
        public void Disable(bool user) {
            if (!this.IsOnline)
                return;

            this.OnDisableCore(user);
            this.IsOnline = false;
            this.IsOfflineByUser = user;
            this.OnIsOnlineStateChanged();
        }

        /// <summary>
        /// Called by <see cref="Disable"/> when this resource item is about to be disabled. This can
        /// do things like release file locks/handles, unload image bitmap data to reduce memory usage, etc.
        /// </summary>
        /// <param name="user">
        /// True if this was disabled forcefully by the user via the UI,
        /// False if it was disabled by something else, such as an error
        /// </param>
        protected virtual void OnDisableCore(bool user) {
        }

        /// <summary>
        /// Tries to enable this resource. If there were errors, they will be caught and added to the loader.
        /// If already online, then nothing happens and true is returned
        /// </summary>
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
        /// If there are errors, then add entries to the resource loader if it is non-null
        /// </para>
        /// </summary>
        /// <param name="loader"></param>
        protected virtual bool OnTryAutoEnable(ResourceLoader loader) {
            return true;
        }

        /// <summary>
        /// Called from a resource loader to try and load this resource from the entry we created.
        /// This method calls <see cref="EnableCore"/>, which enables this resource. Overriding methods
        /// should not call the base method if they could not be loaded from the given entry
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
            this.OnIsOnlineStateChanged();
        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            if (this.UniqueId != 0)
                data.SetULong(nameof(this.UniqueId), this.UniqueId);
            if (!this.IsOnline)
                data.SetBool(nameof(this.IsOnline), false);
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            this.UniqueId = data.GetULong(nameof(this.UniqueId), EmptyId);
            if (data.TryGetBool(nameof(this.IsOnline), out bool isOnline) && !isOnline) {
                this.IsOnline = false;
                this.IsOfflineByUser = true;
            }
        }

        public virtual void OnIsOnlineStateChanged() {
            this.OnlineStateChanged?.Invoke(this);
        }

        /// <summary>
        /// Internal method for setting a resource item's unique ID
        /// </summary>
        internal static void SetUniqueId(ResourceItem item, ulong id) => item.UniqueId = id;
    }
}