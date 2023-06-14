using System;
using System.Collections.Generic;
using FramePFX.Core.Editor.Registries;
using FramePFX.Core.RBC;

namespace FramePFX.Core.Editor.ResourceManaging {
    /// <summary>
    /// A group of resource items
    /// </summary>
    public class ResourceGroup : BaseResourceObject {
        private readonly List<BaseResourceObject> items;

        public IEnumerable<BaseResourceObject> Items => this.items;

        public ResourceGroup() {
            this.items = new List<BaseResourceObject>();
        }

        public ResourceGroup(string displayName) : this() {
            this.DisplayName = displayName;
        }

        public override void SetGroup(ResourceGroup group) {
            base.SetGroup(group);
            foreach (BaseResourceObject item in this.items) {
                item.SetGroup(group);
            }
        }

        public override void SetManager(ResourceManager manager) {
            base.SetManager(manager);
            foreach (BaseResourceObject item in this.items) {
                item.SetManager(manager);
            }
        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            RBEList list = data.CreateList(nameof(this.items));
            foreach (BaseResourceObject item in this.items) {
                RBEDictionary dictionary = list.AddDictionary();
                if (!(item.RegistryId is string registryId))
                    throw new Exception($"Model Type is not registered: {this.GetType()}");
                dictionary.SetString(nameof(this.RegistryId), registryId);
                item.WriteToRBE(dictionary.CreateDictionary("Data"));
            }
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            RBEList list = data.GetList(nameof(this.items));
            foreach (RBEBase item in list.List) {
                if (!(item is RBEDictionary dictionary))
                    throw new Exception("Expected item list to contain only dictionaries, not " + item);
                string registryId = dictionary.GetString(nameof(this.RegistryId), null);
                if (string.IsNullOrEmpty(registryId))
                    throw new Exception("Missing the registry ID for item");
                RBEDictionary dataDictionary = dictionary.GetDictionary("Data");
                BaseResourceObject resource = ResourceTypeRegistry.Instance.CreateResourceItemModel(registryId);
                resource.ReadFromRBE(dataDictionary);
                this.items.Add(resource);
            }
        }

        public void AddItemToList(BaseResourceObject value) {
            this.InsertItemIntoList(this.items.Count, value);
        }

        public T Add<T>(T item) where T : BaseResourceObject {
            this.AddItemToList(item);
            return item;
        }

        public void InsertItemIntoList(int index, BaseResourceObject value) {
            this.items.Insert(index, value);
            value.SetGroup(this);
        }

        public BaseResourceObject GetItemAt(int index) {
            return this.items[index];
        }

        public bool RemoveItemFromList(BaseResourceObject value) {
            int index = this.items.IndexOf(value);
            if (index < 0) {
                return false;
            }

            this.RemoveItemFromListAt(index);
            return true;
        }

        public void RemoveItemFromListAt(int index) {
            BaseResourceObject value = this.items[index];
            this.items.RemoveAt(index);
            value.SetManager(null);
            value.SetGroup(null);
        }

        public void CleanupAndClear() {
            foreach (BaseResourceObject item in this.items) {
                item.SetManager(null);
                item.SetGroup(null);
            }

            this.items.Clear();
        }

        public void ClearFast() {
            this.items.Clear();
        }
    }
}