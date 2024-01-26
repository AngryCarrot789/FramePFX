using System;
using FramePFX.Editors.ResourceManaging.Events;
using FramePFX.Editors.ResourceManaging.Resources;
using FramePFX.RBC;

namespace FramePFX.Editors.ResourceManaging {
    /// <summary>
    /// The base class for all resource items that can be used by clip objects
    /// to store and access data shareable across multiple clips
    /// </summary>
    public abstract class ResourceItem : BaseResource, IDisposable {
        public const ulong EmptyId = ResourceManager.EmptyId;

        /// <summary>
        /// Gets or sets if this resource is online (usable) or offline (not usable by clips).
        /// <see cref="OnIsOnlineStateChanged"/> must be called after modifying this value
        /// </summary>
        public bool IsOnline { get; private set; }

        /// <summary>
        /// Gets or sets if the reason the resource is offline is because a user forced it offline
        /// </summary>
        public bool IsOfflineByUser { get; set; }

        /// <summary>
        /// This resource item's unique identifier
        /// </summary>
        public ulong UniqueId { get; private set; }

        /// <summary>
        /// An event fired when our <see cref="IsOnline"/> property changes. <see cref="IsOfflineByUser"/> may have changed too
        /// </summary>
        public event ResourceItemEventHandler OnlineStateChanged;

        protected ResourceItem() {
        }

        public bool IsRegistered() {
            return this.Manager != null && this.UniqueId != EmptyId && this.Manager.TryGetEntryItem(this.UniqueId, out ResourceItem resource);
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

        private void SetOnlineStateHelper(bool newOnlineState) {
            if (this.IsOnline == newOnlineState)
                return;
            this.IsOnline = newOnlineState;
            if (newOnlineState) {
                this.IsOfflineByUser = false;
            }

            this.OnIsOnlineStateChanged();
        }

        /// <summary>
        /// Internal method for setting a resource item's unique ID
        /// </summary>
        internal static void SetUniqueId(ResourceItem item, ulong id) => item.UniqueId = id;

        protected static void SetOnlineHelper(ResourceImage resourceImage) {
            resourceImage.SetOnlineStateHelper(true);
        }
    }
}