using System;
using FramePFX.Core.Editor.Registries;
using FramePFX.Core.RBC;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.ResourceManaging
{
    /// <summary>
    /// Base class for resource items and groups
    /// </summary>
    public abstract class BaseResourceObject : IRBESerialisable, IDisposable
    {
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

        protected BaseResourceObject()
        {
        }

        /// <summary>
        /// Called when a <see cref="ResourceGroup"/> adds the current instance to its internal list
        /// </summary>
        /// <param name="group">The new group</param>
        public virtual void SetParent(ResourceGroup group)
        {
            this.Parent = group;
            this.OnParentChainChanged();
        }

        /// <summary>
        /// Called when this resource's parent chain is modified, e.g., a resource group is moved into another resource group,
        /// this method is called for every single child of the group that was moved (recursively)
        /// </summary>
        protected internal virtual void OnParentChainChanged()
        {

        }

        /// <summary>
        /// Called when the current instance is associated with a new manager
        /// </summary>
        /// <param name="manager">The new manager</param>
        public virtual void SetManager(ResourceManager manager)
        {
            this.Manager = manager;
        }

        public virtual void WriteToRBE(RBEDictionary data)
        {
            if (!string.IsNullOrEmpty(this.DisplayName))
                data.SetString(nameof(this.DisplayName), this.DisplayName);
        }

        public virtual void ReadFromRBE(RBEDictionary data)
        {
            this.DisplayName = data.GetString(nameof(this.DisplayName), null);
        }

        /// <summary>
        /// Called when this resource object is about to be "deleted", as in, the user wanted to delete
        /// the resource. This should close any open file handles, unregister any event handlers, etc
        /// </summary>
        public void Dispose()
        {
            using (ErrorList list = new ErrorList())
            {
                this.DisposeCore(list);
            }
        }

        /// <summary>
        /// Called by <see cref="Dispose"/> to dispose of any unmanaged resources, unregister event handlers, etc.
        /// <para>
        /// This method should not throw, but instead, exceptions should be added to the given <see cref="ErrorList"/>
        /// </para>
        /// <para>
        /// If this object is a <see cref="ResourceItem"/>, it will not be unregistered from the resource manager; that must be done manually
        /// </para>
        /// </summary>
        /// <param name="list">A list to add exceptions to</param>
        protected virtual void DisposeCore(ErrorList list)
        {
        }
    }
}