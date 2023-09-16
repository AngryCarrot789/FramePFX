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
                this.items.Add(ReadSerialisedWithId(dictionary));
            }
        }

        public void AddItem(BaseResourceObject value) {
            this.InsertItem(this.items.Count, value);
        }

        public void InsertItem(int index, BaseResourceObject value) {
            if (this.items.Contains(value))
                throw new Exception("Value already stored in this group");
            this.items.Insert(index, value);
            value.SetManager(this.Manager);
            value.SetParent(this);
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
            Debug.Assert(item.Parent == this, "Expected item's parent to equal the current group");
            Debug.Assert(item.Manager == this.Manager, "Expected item's parent to equal the current group");
            this.items.RemoveAt(index);
            item.SetParent(null);
            item.SetManager(null);
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

        protected override void DisposeCore(ErrorList list) {
            base.DisposeCore(list);
            using (ErrorList innerList = new ErrorList("Exception disposing child resources", false)) {
                foreach (BaseResourceObject resource in this.items) {
                    try {
                        resource.Dispose();
                    }
                    catch (Exception e) {
                        innerList.Add(new Exception("Exception while disposing " + resource.GetType(), e));
                    }
                }

                if (innerList.TryGetException(out Exception exception)) {
                    list.Add(exception);
                }
            }
        }
    }
}