using System;
using System.Collections.Generic;
using System.Diagnostics;
using FramePFX.RBC;
using FramePFX.Utils;

namespace FramePFX.Editor.ResourceManaging {
    /// <summary>
    /// A group of resource items
    /// </summary>
    public sealed class ResourceGroup : BaseResourceObject {
        private readonly List<BaseResourceObject> items;

        public IReadOnlyList<BaseResourceObject> Items => this.items;

        public ResourceGroup() {
            this.items = new List<BaseResourceObject>();
        }

        public ResourceGroup(string displayName) : this() {
            this.DisplayName = displayName;
        }

        protected internal override void OnParentChainChanged() {
            base.OnParentChainChanged();
            foreach (BaseResourceObject obj in this.items)
                obj.OnParentChainChanged();
        }

        public override void SetManager(ResourceManager manager) {
            base.SetManager(manager);
            foreach (BaseResourceObject item in this.items) {
                item.SetManager(manager);
            }
        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            RBEList list = data.CreateList("Items");
            foreach (BaseResourceObject item in this.items) {
                list.Add(WriteSerialisedWithId(item));
            }
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            RBEList list = data.GetList("Items");
            foreach (RBEDictionary dictionary in list.OfType<RBEDictionary>()) {
                this.AddItem(ReadSerialisedWithId(dictionary));
            }
        }

        public void AddItem(BaseResourceObject item) {
            this.InsertItem(this.items.Count, item);
        }

        public void InsertItem(int index, BaseResourceObject item) {
            if (this.items.Contains(item))
                throw new Exception("Value already stored in this group");
            item.SetParent(this);
            this.items.Insert(index, item);
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
            item.SetParent(null);
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
    }
}