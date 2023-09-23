using System;
using System.Collections.Generic;
using FramePFX.RBC;
using FramePFX.Utils;

namespace FramePFX.Editor.ResourceManaging {
    /// <summary>
    /// A group of resource items
    /// </summary>
    public sealed class ResourceFolder : BaseResourceObject {
        private readonly List<BaseResourceObject> items;

        public IReadOnlyList<BaseResourceObject> Items => this.items;

        public ResourceFolder() {
            this.items = new List<BaseResourceObject>();
        }

        public ResourceFolder(string displayName) : this() {
            this.DisplayName = displayName;
        }

        protected internal override void OnParentChainChanged() {
            base.OnParentChainChanged();
            foreach (BaseResourceObject obj in this.items)
                obj.OnParentChainChanged();
        }

        protected internal override void SetManager(ResourceManager manager) {
            base.SetManager(manager);
            foreach (BaseResourceObject item in this.items) {
                item.SetManager(manager);
            }
        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            RBEList list = data.CreateList("Items");
            foreach (BaseResourceObject item in this.items) {
                list.Add(WriteSerialisedWithType(item));
            }
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            RBEList list = data.GetList("Items");
            foreach (RBEDictionary dictionary in list.OfType<RBEDictionary>()) {
                this.AddItem(ReadSerialisedWithType(dictionary));
            }
        }

        protected override void LoadCloneDataFromObject(BaseResourceObject obj) {
            base.LoadCloneDataFromObject(obj);
            foreach (BaseResourceObject child in ((ResourceFolder) obj).items) {
                this.AddItem(Clone(child));
            }
        }

        public void AddItem(BaseResourceObject item) {
            this.InsertItem(this.items.Count, item);
        }

        public void InsertItem(int index, BaseResourceObject item) {
            if (this.items.Contains(item))
                throw new Exception("Value already stored in this group");
            this.items.Insert(index, item);
            SetParent(item, this);
            item.SetManager(this.Manager);
        }

        public bool RemoveItem(BaseResourceObject item) {
            int index = this.items.IndexOf(item);
            if (index < 0)
                return false;
            this.RemoveItemAt(index);
            return true;
        }

        public void RemoveItemAt(int index) {
            BaseResourceObject item = this.items[index];
            ExceptionUtils.Assert(item.Parent == this, "Expected item's parent to equal the us");
            ExceptionUtils.Assert(item.Manager == this.Manager, "Expected item's manager to equal the our manager");
            this.items.RemoveAt(index);
            item.SetManager(null);
            SetParent(item, null);
        }

        public void MoveItemTo(int srcIndex, ResourceFolder target) {
            this.MoveItemTo(srcIndex, target, target.items.Count);
        }

        public void MoveItemTo(int srcIndex, ResourceFolder target, int dstIndex) {
            BaseResourceObject item = this.items[srcIndex];
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
                foreach (BaseResourceObject resource in this.items) {
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
        public static void RegisterHierarchy(ResourceManager manager, BaseResourceObject resource) {
            if (resource is ResourceItem) {
                manager.RegisterEntry((ResourceItem) resource);
            }
            else if (resource is ResourceFolder) {
                foreach (BaseResourceObject obj in ((ResourceFolder) resource).items) {
                    RegisterHierarchy(manager, obj);
                }
            }
        }
    }
}