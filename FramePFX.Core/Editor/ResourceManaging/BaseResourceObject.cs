using System;
using System.Diagnostics;
using FramePFX.Core.RBC;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.ResourceManaging {
    /// <summary>
    /// Base class for resource items and groups
    /// </summary>
    public abstract class BaseResourceObject : IRBESerialisable, IDisposable {
        /// <summary>
        /// The manager that this resource belongs to. Null if the resource is unregistered
        /// </summary>
        public ResourceManager Manager { get; private set; }

        /// <summary>
        /// The group that this object is currently in, or null, if this is a root object
        /// </summary>
        public ResourceGroup Group { get; set; }

        /// <summary>
        /// This resource object's registry ID, used to reflectively create an instance of it while deserializing data
        /// </summary>
        public string RegistryId => ResourceTypeRegistry.Instance.GetTypeIdForModel(this.GetType());

        public string DisplayName { get; set; }

        protected BaseResourceObject() {

        }

        public virtual void WriteToRBE(RBEDictionary data) {
            if (!string.IsNullOrEmpty(this.DisplayName))
                data.SetString(nameof(this.DisplayName), this.DisplayName);
        }

        public virtual void ReadFromRBE(RBEDictionary data) {
            this.DisplayName = data.GetString(nameof(this.DisplayName), null);
        }

        protected virtual void OnManagerChanged(ResourceManager oldManager, ResourceManager newManager) {

        }

        public static void SetManager(BaseResourceObject resource, ResourceManager manager, bool fireManagerChanged = true) {
            ResourceManager old = resource.Manager;
            if (ReferenceEquals(old, manager)) {
                Debug.WriteLine($"Attempted to set the manager to the same instance\n{new StackTrace(true)}");
                return;
            }

            resource.Manager = manager;
            if (fireManagerChanged) {
                resource.OnManagerChanged(old, manager);
            }
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

        protected virtual void DisposeCore(ExceptionStack stack) {

        }
    }
}