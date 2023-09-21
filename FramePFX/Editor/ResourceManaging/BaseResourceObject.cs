using System;
using FramePFX.Editor.Registries;
using FramePFX.RBC;
using FramePFX.Utils;

namespace FramePFX.Editor.ResourceManaging {
    /// <summary>
    /// Base class for resource items and groups
    /// </summary>
    public abstract class BaseResourceObject {
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
        public string FactoryId => ResourceTypeRegistry.Instance.GetTypeIdForModel(this.GetType());

        public string DisplayName { get; set; }

        protected BaseResourceObject() {
        }

        /// <summary>
        /// Called when a <see cref="ResourceGroup"/> adds the current instance to its internal list
        /// </summary>
        /// <param name="group">The new group</param>
        public virtual void SetParent(ResourceGroup group) {
            this.Parent = group;
            this.OnParentChainChanged();
        }

        /// <summary>
        /// Called when this resource's parent chain is modified, e.g., a resource group is moved into another resource group,
        /// this method is called for every single child of the group that was moved (recursively)
        /// </summary>
        protected internal virtual void OnParentChainChanged() {
        }

        /// <summary>
        /// Called when the current instance is associated with a new manager
        /// </summary>
        /// <param name="manager">The new manager</param>
        public virtual void SetManager(ResourceManager manager) {
            this.Manager = manager;
        }

        public static BaseResourceObject ReadSerialisedWithId(RBEDictionary dictionary) {
            string registryId = dictionary.GetString(nameof(FactoryId), null);
            if (string.IsNullOrEmpty(registryId))
                throw new Exception("Missing the registry ID for item");
            RBEDictionary data = dictionary.GetDictionary("Data");
            BaseResourceObject resource = ResourceTypeRegistry.Instance.CreateModel(registryId);
            resource.ReadFromRBE(data);
            return resource;
        }

        public static void WriteSerialisedWithId(RBEDictionary dictionary, BaseResourceObject item) {
            if (!(item.FactoryId is string id))
                throw new Exception("Unknown resource item type: " + item.GetType());
            dictionary.SetString(nameof(FactoryId), id);
            item.WriteToRBE(dictionary.CreateDictionary("Data"));
        }

        public static RBEDictionary WriteSerialisedWithId(BaseResourceObject clip) {
            RBEDictionary dictionary = new RBEDictionary();
            WriteSerialisedWithId(dictionary, clip);
            return dictionary;
        }

        public virtual void WriteToRBE(RBEDictionary data) {
            if (!string.IsNullOrEmpty(this.DisplayName))
                data.SetString(nameof(this.DisplayName), this.DisplayName);
        }

        public virtual void ReadFromRBE(RBEDictionary data) {
            this.DisplayName = data.GetString(nameof(this.DisplayName), null);
        }

        /// <summary>
        /// Invoked when this resource is about to be completely deleted. It will not have a parent object nor
        /// a manager associated with it. Dispose of unmanaged resources, unregister event handlers, etc.
        /// <para>
        /// Any errors should be logged to the application logger
        /// </para>
        /// <para>
        /// If this object is a <see cref="ResourceItem"/>, it will not be unregistered from the resource manager; that must be done manually
        /// </para>
        /// </summary>
        public virtual void Dispose() {

        }
    }
}