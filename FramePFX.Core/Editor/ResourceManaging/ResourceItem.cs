using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FramePFX.Core.Editor.ResourceManaging.Events;
using FramePFX.Core.RBC;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.ResourceManaging {
    public abstract class ResourceItem : IRBESerialisable, IDisposable {
        public string RegistryId => ResourceTypeRegistry.Instance.GetTypeIdForModel(this.GetType());

        /// <summary>
        /// This resource item's unique identifier
        /// </summary>
        public string UniqueId { get; private set; }

        /// <summary>
        /// Whether or not this resource item's ID is valid and is registered with the manager associated with it
        /// </summary>
        public bool IsRegistered => !string.IsNullOrWhiteSpace(this.UniqueId) && this.Manager.ResourceExists(this.UniqueId);

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

        public ResourceManager Manager { get; }

        public event ResourceModifiedEventHandler DataModified;
        public event ResourceItemEventHandler OnlineStateChanged;

        protected ResourceItem(ResourceManager manager) {
            this.Manager = manager ?? throw new ArgumentNullException(nameof(manager));
            this.IsOnline = true;
        }

        /// <summary>
        /// A method that forces this resource offline, releasing any resources in the process. This may call <see cref="Dispose"/> or <see cref="DisposeCore"/>
        /// </summary>
        /// <param name="stack"></param>
        /// <returns></returns>
        public virtual Task SetOfflineAsync(ExceptionStack stack) {
            this.IsOnline = false;
            this.IsOfflineByUser = true;
            this.OnIsOnlineStateChanged();
            return Task.CompletedTask;
        }

        public virtual void WriteToRBE(RBEDictionary data) {

        }

        public virtual void ReadFromRBE(RBEDictionary data) {

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
        /// Disposes this resource. This should NOT be called from the destructor/finalizer
        /// </summary>
        public void Dispose() {
            using (ExceptionStack stack = new ExceptionStack()) {
                try {
                    this.DisposeCore(stack);
                }
                catch (Exception e) {
                    stack.Push(new Exception($"Unexpected exception while invoking {nameof(this.DisposeCore)}", e));
                }
            }
        }

        /// <summary>
        /// The core method for disposing resources used by a resource. This method really should not throw,
        /// and instead, exceptions should be added to the given <see cref="ExceptionStack"/>
        /// <para>
        /// This method may not be called when the item is about to be deleted/removed, and
        /// may be called when it's about to be set offline
        /// </para>
        /// </summary>
        /// <param name="stack">The exception stack in which exception should be added into when encountered during disposal</param>
        protected virtual void DisposeCore(ExceptionStack stack) {

        }

        public static void SetUniqueId(ResourceItem item, string id) {
            item.UniqueId = id;
        }
    }
}