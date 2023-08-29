using System;
using System.Collections.Generic;
using FramePFX.Editor.Registries;
using FramePFX.RBC;
using FramePFX.Utils;

namespace FramePFX.Editor.ResourceManaging {
    /// <summary>
    /// A group of resource items
    /// </summary>
    public class ResourceGroup : BaseResourceObject {
        private readonly List<BaseResourceObject> items;

        public IEnumerable<BaseResourceObject> Items => this.items;

        public BaseResourceObject this[int index] => this.items[index];

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
                if (!(item.RegistryId is string registryId))
                    throw new Exception($"Resource type is not registered: {this.GetType()}");
                RBEDictionary dictionary = list.AddDictionary();
                dictionary.SetString(nameof(this.RegistryId), registryId);
                item.WriteToRBE(dictionary.CreateDictionary("Data"));
            }
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            RBEList list = data.GetList("Items");
            foreach (RBEDictionary dictionary in list.OfType<RBEDictionary>()) {
                string registryId = dictionary.GetString(nameof(this.RegistryId), null);
                if (string.IsNullOrEmpty(registryId))
                    throw new Exception("Missing the registry ID for item");
                RBEDictionary resdata = dictionary.GetDictionary("Data");
                BaseResourceObject resource = ResourceTypeRegistry.Instance.CreateResourceItemModel(registryId);
                resource.ReadFromRBE(resdata);
                this.items.Add(resource);
            }
        }

        public void AddItem(BaseResourceObject value, bool setManager = true) {
            this.InsertItem(this.items.Count, value, setManager);
        }

        public void InsertItem(int index, BaseResourceObject value, bool setManager = true) {
            this.items.Insert(index, value);
            value.SetParent(this);
            if (setManager) {
                value.SetManager(this.Manager);
            }
        }

        public bool RemoveItem(BaseResourceObject item) {
            int index = this.items.IndexOf(item);
            if (index < 0) {
                return false;
            }

            this.RemoveItemAt(index);
            return true;
        }

        public void RemoveItemAt(int index) {
            BaseResourceObject value = this.items[index];
            this.items.RemoveAt(index);
            value.SetParent(null);
            value.SetManager(null);
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