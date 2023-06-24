using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FramePFX.Core.Editor.ResourceManaging.Events;
using FramePFX.Core.RBC;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.ResourceManaging {
    public abstract class ResourceItem : BaseResourceObject, IRBESerialisable, IDisposable {
        private bool isOnline;

        /// <summary>
        /// This resource item's unique identifier
        /// </summary>
        public string UniqueId { get; private set; }

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
            if (this.Manager != null && !string.IsNullOrWhiteSpace(this.UniqueId) && this.Manager.TryGetEntryItem(this.UniqueId, out ResourceItem resource)) {
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
        public async Task DisableAsync(ExceptionStack stack, bool user) {
            await this.DisableCoreAsync(stack, user);
            this.IsOnline = false;
            this.IsOfflineByUser = user;
            this.OnIsOnlineStateChanged();
        }

        protected virtual Task DisableCoreAsync(ExceptionStack stack, bool user) {
            return Task.CompletedTask;
        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            if (string.IsNullOrWhiteSpace(this.UniqueId))
                throw new Exception("Item does not have a valid unique ID");
            data.SetString(nameof(this.UniqueId), this.UniqueId);
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            string uniqueId = data.GetString(nameof(this.UniqueId));
            if (string.IsNullOrWhiteSpace(uniqueId))
                throw new Exception("Data does not contain a valid unique ID");
            this.UniqueId = uniqueId;
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

        public static void SetUniqueId(ResourceItem item, string id) {
            item.UniqueId = id;
        }
    }
}