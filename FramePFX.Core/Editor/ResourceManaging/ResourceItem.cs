using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FramePFX.Core.Editor.ResourceManaging.Events;
using FramePFX.Core.RBC;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.ResourceManaging {
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
                if (value) {
                    this.IsOfflineByUser = false;
                }
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
            if (this.IsRegistered(out bool isReferenceValid)) {
                if (!isReferenceValid) {
                    #if DEBUG
                    System.Diagnostics.Debugger.Break();
                    #endif
                    throw new Exception("Registered resource is not reference-equal to the current instance");
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// A method that forces this resource offline, releasing any resources in the process. This may call <see cref="Dispose"/> or <see cref="DisposeCore"/>
        /// </summary>
        /// <param name="stack"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public void Disable(ExceptionStack stack, bool user) {
            this.OnDisableCore(stack, user);
            this.IsOnline = false;
            this.IsOfflineByUser = user;
            this.OnIsOnlineStateChanged();
        }

        protected virtual void OnDisableCore(ExceptionStack stack, bool user) {

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
        /// The core method for disposing resources used by a resource. This method really should not throw,
        /// and instead, exceptions should be added to the given <see cref="ExceptionStack"/>
        /// </summary>
        /// <param name="stack">The exception stack in which exception should be added into when encountered during disposal</param>
        protected override void DisposeCore(ExceptionStack stack) {
            base.DisposeCore(stack);
        }

        public static void SetUniqueId(ResourceItem item, ulong id) {
            item.UniqueId = id;
        }
    }
}