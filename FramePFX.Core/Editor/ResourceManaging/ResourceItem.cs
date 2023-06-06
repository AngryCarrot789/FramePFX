using System;
using System.Runtime.CompilerServices;
using FramePFX.Core.Editor.ResourceManaging.Events;
using FramePFX.Core.RBC;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.ResourceManaging {
    public abstract class ResourceItem : IRBESerialisable, IDisposable {
        public string RegistryId => ResourceTypeRegistry.Instance.GetTypeIdForModel(this.GetType());

        /// <summary>
        /// This resource item's unique identifier
        /// </summary>
        public string UniqueId { get; set; }

        /// <summary>
        /// Whether or not this resource item's ID is valid and is registered with the manager associated with it
        /// </summary>
        public bool IsRegistered => !string.IsNullOrWhiteSpace(this.UniqueId) && this.Manager.ResourceExists(this.UniqueId);

        public ResourceManager Manager { get; }

        public event ResourceModifiedEventHandler DataModified;

        protected ResourceItem(ResourceManager manager) {
            this.Manager = manager ?? throw new ArgumentNullException(nameof(manager));
        }

        public virtual void WriteToRBE(RBEDictionary data) {
            if (!(this.RegistryId is string id))
                throw new Exception($"Model Type is not registered: {this.GetType()}");
            data.SetString(nameof(this.RegistryId), id);
        }

        public virtual void ReadFromRBE(RBEDictionary data) {
            string registryId = this.RegistryId;
            if (!data.TryGetString(nameof(this.RegistryId), out string id) || id != registryId) {
                if (registryId == null) {
                    throw new Exception($"Model Type is not registered: {this.GetType()}");
                }
                else {
                    throw new Exception($"Model Registry Id mis match. Data contained '{id}' but the registered type is {registryId}");
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="DataModified"/> event for this resource item with the given property, allowing listeners
        /// to invalidate their objects that relied on the previous state of the property that changed e.g. text blobs)
        /// </summary>
        /// <param name="propertyName"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void RaiseDataModified([CallerMemberName] string propertyName = null) {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));
            this.DataModified?.Invoke(this, propertyName);
        }

        /// <summary>
        /// Disposes this IO model. This should NOT be called from the destructor/finalizer
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
        /// The core method for disposing of sources and outputs. This method really should not throw,
        /// and instead, exceptions should be added to the given <see cref="ExceptionStack"/>
        /// </summary>
        /// <param name="stack">The exception stack in which exception should be added into when encountered during disposal</param>
        /// <param name="isDisposing"></param>
        protected virtual void DisposeCore(ExceptionStack stack) {

        }
    }
}