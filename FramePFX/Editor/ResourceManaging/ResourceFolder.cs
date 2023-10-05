using System;
using System.Collections.Generic;
using FramePFX.RBC;
using FramePFX.Utils;

namespace FramePFX.Editor.ResourceManaging {
    /// <summary>
    /// A group of resource items
    /// </summary>
    public sealed class ResourceFolder : BaseResource {
        private readonly List<BaseResource> items;

        public IReadOnlyList<BaseResource> Items => this.items;

        public ResourceFolder() {
            this.items = new List<BaseResource>();
        }

        public ResourceFolder(string displayName) : this() {
            this.DisplayName = displayName;
        }

        protected internal override void OnParentChainChanged() {
            base.OnParentChainChanged();
            foreach (BaseResource obj in this.items)
                obj.OnParentChainChanged();
        }

        public override void OnAttachedToManager() {
            base.OnAttachedToManager();
            foreach (BaseResource resource in this.items) {
                resource.OnAttachedToManager();
            }
        }

        public override void OnDetatchedFromManager() {
            base.OnDetatchedFromManager();
            foreach (BaseResource resource in this.items) {
                resource.OnDetatchedFromManager();
            }
        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            RBEList list = data.CreateList("Items");
            foreach (BaseResource item in this.items) {
                list.Add(WriteSerialisedWithType(item));
            }
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            RBEList list = data.GetList("Items");
            foreach (RBEDictionary dictionary in list.Cast<RBEDictionary>()) {
                this.AddItem(ReadSerialisedWithType(dictionary));
            }
        }

        protected override void LoadCloneDataFromObject(BaseResource obj) {
            base.LoadCloneDataFromObject(obj);
            foreach (BaseResource child in ((ResourceFolder) obj).items) {
                this.AddItem(Clone(child));
            }
        }

        public void AddItem(BaseResource item) {
            this.InsertItem(this.items.Count, item);
        }

        public void InsertItem(int index, BaseResource item) {
            if (this.items.Contains(item))
                throw new Exception("Value already stored in this group");
            bool isManagerDifferent = !ReferenceEquals(this.Manager, item.Manager);
            if (isManagerDifferent && item.Manager != null) {
                item.OnDetatchedFromManager();
                item.Manager = null;
            }

            this.items.Insert(index, item);
            SetParent(item, this);
            if (isManagerDifferent && this.Manager != null) {
                item.Manager = this.Manager;
                item.OnAttachedToManager();
            }
        }

        public bool RemoveItem(BaseResource item) {
            int index = this.items.IndexOf(item);
            if (index < 0)
                return false;
            this.RemoveItemAt(index);
            return true;
        }

        public void RemoveItemAt(int index) {
            BaseResource item = this.items[index];
            ExceptionUtils.Assert(item.Parent == this, "Expected item's parent to equal the us");
            ExceptionUtils.Assert(item.Manager == this.Manager, "Expected item's manager to equal the our manager");
            this.items.RemoveAt(index);
            if (item.Manager != null) {
                item.OnDetatchedFromManager();
                item.Manager = null;
            }

            SetParent(item, null);
        }

        public void MoveItemTo(int srcIndex, ResourceFolder target) {
            this.MoveItemTo(srcIndex, target, target.items.Count);
        }

        public void MoveItemTo(int srcIndex, ResourceFolder target, int dstIndex) {
            BaseResource item = this.items[srcIndex];
            ExceptionUtils.Assert(item.Parent == this, "Expected item's parent to equal the us");
            ExceptionUtils.Assert(item.Manager == this.Manager, "Expected item's manager to equal the our manager");
            this.items.RemoveAt(srcIndex);
            target.items.Insert(dstIndex, item);
            SetParent(item, target);
        }

        /// <summary>
        /// Recursively sets this items' manager and group to null, then clears the collection
        /// </summary>
        public void Clear() {
            for (int i = this.items.Count - 1; i >= 0; i--) {
                this.RemoveItemAt(i);
            }
        }

        /// <summary>
        /// Clears this group's items, without setting the items' parent group or manager to null
        /// </summary>
        public void UnsafeClear() => this.items.Clear();

        public override void Dispose() {
            base.Dispose();
            using (ErrorList list = new ErrorList("Exception disposing child resources", false)) {
                foreach (BaseResource resource in this.items) {
                    try {
                        resource.Dispose();
                    }
                    catch (Exception e) {
                        list.Add(new Exception("Exception while disposing " + resource.GetType(), e));
                    }
                }
            }
        }

        /// <summary>
        /// Registers all items in the hierarchy of the given resource if it's a group. If
        /// it is just a resource item, then the item is registered. This is a recursive function
        /// </summary>
        /// <param name="manager">The (non-null) manager to register items with</param>
        /// <param name="resource">Target item</param>
        public static void RegisterHierarchy(ResourceManager manager, BaseResource resource) {
            if (resource is ResourceItem) {
                manager.RegisterEntry((ResourceItem) resource);
            }
            else if (resource is ResourceFolder) {
                foreach (BaseResource obj in ((ResourceFolder) resource).items) {
                    RegisterHierarchy(manager, obj);
                }
            }
        }

        public override void OnProjectLoaded() {
            base.OnProjectLoaded();
            foreach (BaseResource resource in this.items) {
                resource.OnProjectLoaded();
            }
        }

        public override void OnProjectUnloaded() {
            base.OnProjectUnloaded();
            foreach (BaseResource resource in this.items) {
                resource.OnProjectUnloaded();
            }
        }
    }
}