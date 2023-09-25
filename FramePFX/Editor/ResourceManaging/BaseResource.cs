using System;
using FramePFX.Editor.Registries;
using FramePFX.Editor.ResourceManaging.ViewModels;
using FramePFX.RBC;

namespace FramePFX.Editor.ResourceManaging {
    /// <summary>
    /// Base class for resource items and groups
    /// </summary>
    public abstract class BaseResource {
        /// <summary>
        /// The manager that this resource belongs to. Null if the resource is unregistered
        /// </summary>
        public ResourceManager Manager { get; private set; }

        /// <summary>
        /// The group that this object is currently in, or null, if this is a root object
        /// </summary>
        public ResourceFolder Parent { get; private set; }

        /// <summary>
        /// This resource object's registry ID, used to reflectively create an instance of it while deserializing data
        /// </summary>
        public string FactoryId => ResourceTypeFactory.Instance.GetTypeIdForModel(this.GetType());

        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the view model associated with this resource item. This is a horrible hack around the
        /// fact that there's currently no other way to access a resource item view model by a resource ID
        /// </summary>
        public BaseResourceViewModel ViewModel { get; set; }

        protected BaseResource() {
        }

        /// <summary>
        /// Creates a clone of the item, and also any child items if the item is a group. This will not register
        /// anything with the resource manager, that must be done manually or via <see cref="CloneAndRegister"/>
        /// </summary>
        /// <param name="item">The item to clone</param>
        /// <returns>A cloned and fully registered but offline resource</returns>
        /// <exception cref="Exception">Internal error with the resource registry; cloned item type does not match the original item</exception>
        public static BaseResource Clone(BaseResource item) {
            BaseResource clone = ResourceTypeFactory.Instance.CreateModel(item.FactoryId);
            if (clone.GetType() != item.GetType())
                throw new Exception("Cloned object type does not match the item type");
            clone.LoadCloneDataFromObject(item);
            return clone;
        }

        /// <summary>
        /// Clones the item and registers the clone and any child items with the given item's resource manager, if available
        /// </summary>
        /// <param name="item">The item to clone</param>
        /// <returns>A cloned and fully registered but offline resource</returns>
        /// <exception cref="Exception">Internal error with the resource registry; cloned item type does not match the original item</exception>
        public static BaseResource CloneAndRegister(BaseResource item) {
            BaseResource clone = Clone(item);
            if (item.Manager != null)
                ResourceFolder.RegisterHierarchy(item.Manager, clone);
            return clone;
        }

        public static void SetParent(BaseResource obj, ResourceFolder parent) {
            obj.Parent = parent;
            obj.OnParentChainChanged();
        }

        /// <summary>
        /// Called when this resource's parent chain is modified, e.g., a resource folder is moved into another resource folder,
        /// this method is called for every single child of the group that was moved (recursively)
        /// </summary>
        protected internal virtual void OnParentChainChanged() {
        }

        /// <summary>
        /// Called when the current instance is associated with a new manager
        /// </summary>
        /// <param name="manager">The new manager</param>
        protected internal virtual void SetManager(ResourceManager manager) {
            this.Manager = manager;
        }

        public static BaseResource ReadSerialisedWithType(RBEDictionary dictionary) {
            string registryId = dictionary.GetString(nameof(FactoryId), null);
            if (string.IsNullOrEmpty(registryId))
                throw new Exception("Missing the registry ID for item");
            RBEDictionary data = dictionary.GetDictionary("Data");
            BaseResource resource = ResourceTypeFactory.Instance.CreateModel(registryId);
            resource.ReadFromRBE(data);
            return resource;
        }

        public static void WriteSerialisedWithType(RBEDictionary dictionary, BaseResource item) {
            if (!(item.FactoryId is string id))
                throw new Exception("Unknown resource item type: " + item.GetType());
            dictionary.SetString(nameof(FactoryId), id);
            item.WriteToRBE(dictionary.CreateDictionary("Data"));
        }

        public static RBEDictionary WriteSerialisedWithType(BaseResource clip) {
            RBEDictionary dictionary = new RBEDictionary();
            WriteSerialisedWithType(dictionary, clip);
            return dictionary;
        }

        public virtual void ReadFromRBE(RBEDictionary data) {
            this.DisplayName = data.GetString(nameof(this.DisplayName), null);
        }

        public virtual void WriteToRBE(RBEDictionary data) {
            if (!string.IsNullOrEmpty(this.DisplayName))
                data.SetString(nameof(this.DisplayName), this.DisplayName);
        }

        /// <summary>
        /// This is invoked during the clone process. The current instance is a new cloned object.
        /// load data from the given object into the current instance
        /// </summary>
        /// <param name="obj">An object to copy data from</param>
        protected virtual void LoadCloneDataFromObject(BaseResource obj) {
            this.DisplayName = obj.DisplayName;
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