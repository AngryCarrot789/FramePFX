using System;
using FramePFX.Editors.Factories;
using FramePFX.Editors.ResourceManaging.Events;
using FramePFX.RBC;
using FramePFX.Utils.Destroying;

namespace FramePFX.Editors.ResourceManaging {
    /// <summary>
    /// Base class for resource items and groups
    /// </summary>
    public abstract class BaseResource : IDestroy {
        private string displayName;
        private bool isSelected;

        /// <summary>
        /// The manager that this resource belongs to. This will be non-null when <see cref="Parent"/> is non-null
        /// </summary>
        public ResourceManager Manager { get; private set; }

        /// <summary>
        /// The folder that this object is currently in. This will be null or either the root folder in a resource manager,
        /// or if this resource just isn't in a resource tree. If this is null, then <see cref="Manager"/> will also be null
        /// </summary>
        public ResourceFolder Parent { get; private set; }

        /// <summary>
        /// This resource object's registry ID, used to reflectively create an instance of it while deserializing data
        /// </summary>
        public string FactoryId => ResourceTypeFactory.Instance.GetId(this.GetType());

        public string DisplayName {
            get => this.displayName;
            set {
                if (this.displayName == value)
                    return;
                this.displayName = value;
                this.DisplayNameChanged?.Invoke(this);
            }
        }

        public bool IsSelected {
            get => this.isSelected;
            set {
                if (this.isSelected == value)
                    return;
                this.isSelected = value;
                ResourceManager.InternalProcessResourceSelectionChanged(this);
                this.IsSelectedChanged?.Invoke(this);
            }
        }

        public event ResourceEventHandler DisplayNameChanged;
        public event ResourceEventHandler IsSelectedChanged;

        protected BaseResource() {
            this.displayName = "A Resource";
        }

        /// <summary>
        /// Creates a clone of the item, and also any child items if the item is a group
        /// </summary>
        /// <param name="item">The item to clone</param>
        /// <returns>A cloned and fully registered but offline resource</returns>
        /// <exception cref="Exception">Internal error with the resource registry; cloned item type does not match the original item</exception>
        public static BaseResource Clone(BaseResource item) {
            BaseResource clone = ResourceTypeFactory.Instance.NewResource(item.FactoryId);
            if (clone.GetType() != item.GetType())
                throw new Exception("Cloned object type does not match the item type");
            item.LoadDataIntoClone(clone);
            return clone;
        }

        /// <summary>
        /// Invoked when this resource is added to a resource manager's resource hierarchy.
        /// <see cref="Manager"/> is set to a non-null value prior to this call.
        /// <para>
        /// The cause of this call is either this resource being added, or a folder containing
        /// this resource (possibly deep in the hierarchy) was added (assigning the manager
        /// reference for all sub-resources recursively)
        /// </para>
        /// </summary>
        protected internal virtual void OnAttachedToManager() {
        }

        /// <summary>
        /// Invoked when this resource is removed from a resource manager's resource hierarchy.
        /// <see cref="Manager"/> is set to null after this call.
        /// <para>
        /// The cause of this call is either this resource being removed, or a folder containing
        /// this resource (possibly deep in the hierarchy) was removed (clearing the manager
        /// reference for all sub-resources recursively)
        /// </para>
        /// </summary>
        protected internal virtual void OnDetatchedFromManager() {
        }

        public static BaseResource ReadSerialisedWithType(RBEDictionary dictionary) {
            string registryId = dictionary.GetString(nameof(FactoryId), null);
            if (string.IsNullOrEmpty(registryId))
                throw new Exception("Missing the registry ID for item");
            RBEDictionary data = dictionary.GetDictionary("Data");
            BaseResource resource = ResourceTypeFactory.Instance.NewResource(registryId);
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
        /// <param name="clone">An object to copy data from</param>
        protected virtual void LoadDataIntoClone(BaseResource clone) {
            clone.DisplayName = this.DisplayName;
        }

        /// <summary>
        /// Destroys this resource's data, such as disposing objects, unregistering event handlers, etc.
        /// <para>
        /// Any errors should be logged to the application logger
        /// </para>
        /// <para>
        /// If this object is a <see cref="ResourceFolder"/> then the child hierarchy will NOT be
        /// destroyed, use <see cref="ResourceFolder.ClearHierarchy"/> instead
        /// </para>
        /// <para>
        /// If this object is a <see cref="ResourceItem"/> then <see cref="ResourceItem.Disable"/> is
        /// called to disable the object
        /// </para>
        /// </summary>
        public virtual void Destroy() {

        }

        /// <summary>
        /// An internal method used to set a manager's root folder's manager
        /// </summary>
        internal static void InternalSetManagerForRootFolder(ResourceFolder root, ResourceManager owner) {
            // root folder selection should not be processed
            root.Manager = owner;
            root.OnAttachedToManager();
        }

        protected static void InternalOnItemAdded(BaseResource obj, ResourceFolder parent) {
            obj.Parent = parent;
            ResourceManager manager = parent.Manager;
            if (manager != null) {
                InternalSetResourceManager(obj, manager);
            }
        }

        protected static void InternalOnItemRemoved(BaseResource obj, ResourceFolder parent) {
            obj.Parent = null;
            if (obj.Manager != null) {
                ResourceManager.InternalProcessResourceOnDetatched(obj);
                obj.OnDetatchedFromManager();
                obj.Manager = null;
            }
        }

        protected static void InternalOnItemMoved(BaseResource obj, ResourceFolder newParent) {
            if (obj.Manager != newParent.Manager)
                throw new Exception("Manager was different");
            obj.Parent = newParent;
        }

        protected static void InternalSetResourceManager(BaseResource resource, ResourceManager manager) {
            if (ReferenceEquals(resource.Manager, manager)) {
                throw new InvalidOperationException("Cannot set manager to same instance");
            }

            if (manager != null) {
                resource.Manager = manager;
                ResourceManager.InternalProcessResourceOnAttached(resource, manager);
                resource.OnAttachedToManager();
            }
            else {
                ResourceManager.InternalProcessResourceOnDetatched(resource);
                resource.OnDetatchedFromManager();
                resource.Manager = null;
            }
        }
    }
}