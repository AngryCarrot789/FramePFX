using System;
using FramePFX.Core.Editor.Registries;
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
        public ResourceGroup Parent { get; private set; }

        /// <summary>
        /// This resource object's registry ID, used to reflectively create an instance of it while deserializing data
        /// </summary>
        public string RegistryId => ResourceTypeRegistry.Instance.GetTypeIdForModel(this.GetType());

        public string DisplayName { get; set; }

        protected BaseResourceObject() {

        }

        /// <summary>
        /// Called when a <see cref="ResourceGroup"/> adds the current instance to its internal list
        /// </summary>
        /// <param name="group">The new group</param>
        public virtual void SetParent(ResourceGroup group) {
            this.Parent = group;
        }

        /// <summary>
        /// Called when the current instance is associated with a new manager
        /// </summary>
        /// <param name="manager">The new manager</param>
        public virtual void SetManager(ResourceManager manager) {
            this.Manager = manager;
        }

        public virtual void WriteToRBE(RBEDictionary data) {
            if (!string.IsNullOrEmpty(this.DisplayName))
                data.SetString(nameof(this.DisplayName), this.DisplayName);
        }

        public virtual void ReadFromRBE(RBEDictionary data) {
            this.DisplayName = data.GetString(nameof(this.DisplayName), null);
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
                    stack.Add(new Exception($"Unexpected exception while invoking {nameof(this.DisposeCore)}", e));
                }
            }
        }

        /// <summary>
        /// Disposes this resource object's resources. Resources can be re-used after disposing, so this should
        /// just clean up anything that the resource originally did not own or have allocated when created
        /// <para>
        /// This method should not throw, and instead, exceptions should be added to the given stack
        /// </para>
        /// </summary>
        /// <param name="stack">Stack to add exceptions to</param>
        protected virtual void DisposeCore(ExceptionStack stack) {

        }
    }
}