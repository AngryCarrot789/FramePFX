using System;
using System.Runtime.CompilerServices;
using FramePFX.Editor.ResourceManaging.Events;
using FramePFX.RBC;
using FramePFX.Utils;

namespace FramePFX.Editor.ResourceManaging {
    public abstract class ResourceItem : BaseResourceObject, IRBESerialisable, IDisposable {
        public const ulong EmptyId = ResourceManager.EmptyId;
        private bool isOnline;

        /// <summary>
        /// Whether or not this resource is online or not
        /// </summary>
        public bool IsOnline {
            get => this.isOnline;
            set {
                this.isOnline = value;
                if (value)
                    this.IsOfflineByUser = false;
            }
        }

        /// <summary>
        /// Whether this resource was forced offline by the user
        /// </summary>
        public bool IsOfflineByUser { get; set; }

        /// <summary>
        /// This resource item's unique identifier
        /// </summary>
        public ulong UniqueId { get; private set; }

        public event ResourceModifiedEventHandler DataModified;
        public event ResourceItemEventHandler OnlineStateChanged;

        protected ResourceItem() {
            this.IsOnline = true;
        }

        /// <summary>
        /// Gets whether or not this resource item has a manager associated, a valid unique ID and is registered with that manager
        /// </summary>
        /// <param name="isReferenceValid">
        /// The registered resource is reference-equal to the current instance. This should always be true. If it's false, then something terrible has happened
        /// </param>
        /// <returns>True if this instance has a manager, a valid ID and is registered</returns>
        public bool IsRegistered(out bool isReferenceValid) {
            if (this.Manager != null && this.UniqueId != EmptyId && this.Manager.TryGetEntryItem(this.UniqueId, out ResourceItem resource)) {
                isReferenceValid = ReferenceEquals(this, resource);
                return true;
            }

            return isReferenceValid = false;
        }

        public bool IsRegistered() {
            if (!this.IsRegistered(out bool isReferenceValid))
                return false;
            if (!isReferenceValid)
                throw new Exception("Registered resource is not reference-equal to the current instance");
            return true;
        }

        /// <summary>
        /// A method that forces this resource offline, releasing any resources in the process. This may call <see cref="Dispose"/> or <see cref="DisposeCore"/>
        /// </summary>
        /// <param name="list">A list to add encountered exceptions to</param>
        /// <param name="user">
        /// Whether or not this resource was disabled by the user force disabling
        /// the item. <see cref="IsOfflineByUser"/> is set to this parameter
        /// </param>
        /// <returns></returns>
        public void Disable(ErrorList list, bool user) {
            if (!this.IsOnline)
                return;

            this.OnDisableCore(list, user);
            this.IsOnline = false;
            this.IsOfflineByUser = user;
            this.OnIsOnlineStateChanged();
        }

        /// <summary>
        /// Called by <see cref="Disable"/> when this resource item should be disabled
        /// </summary>
        /// <param name="list"></param>
        /// <param name="user"></param>
        protected virtual void OnDisableCore(ErrorList list, bool user) {
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
            if (data.TryGetBool(nameof(this.IsOnline), out bool b) && !b) {
                this.isOnline = false;
                this.IsOfflineByUser = true;
            }
        }

        /// <summary>
        /// Raises the <see cref="DataModified"/> event for this resource item with the given property, allowing listeners
        /// to invalidate their objects that relied on the previous state of the property that changed e.g. text blobs)
        /// </summary>
        /// <param name="propertyName"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual void OnDataModified([CallerMemberName] string propertyName = null) {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));
            this.DataModified?.Invoke(this, propertyName);
        }

        public virtual void OnIsOnlineStateChanged() {
            this.OnlineStateChanged?.Invoke(this.Manager, this);
        }

        /// <summary>
        /// Internal method for setting a resource item's unique ID
        /// </summary>
        internal static void SetUniqueId(ResourceItem item, ulong id) => item.UniqueId = id;
    }
}