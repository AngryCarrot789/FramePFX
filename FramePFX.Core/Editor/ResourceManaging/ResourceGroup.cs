using System;
using System.Collections.Generic;
using FramePFX.Core.Editor.ResourceManaging.Resources;
using FramePFX.Core.RBC;

namespace FramePFX.Core.Editor.ResourceManaging {
    /// <summary>
    /// A group of resource items
    /// </summary>
    public class ResourceGroup : BaseResourceObject {
        public List<BaseResourceObject> Items { get; }

        public ResourceGroup() {
            this.Items = new List<BaseResourceObject>();
        }

        protected override void OnManagerChanged(ResourceManager oldManager, ResourceManager newManager) {
            base.OnManagerChanged(oldManager, newManager);
            foreach (BaseResourceObject item in this.Items) {
                SetManager(item, newManager);
            }
        }

        public override void WriteToRBE(RBEDictionary data) {
            base.WriteToRBE(data);
            RBEList list = data.CreateList(nameof(this.Items));
            foreach (BaseResourceObject item in this.Items) {
                RBEDictionary dictionary = list.AddDictionary();
                if (!(item.RegistryId is string registryId))
                    throw new Exception($"Model Type is not registered: {this.GetType()}");
                dictionary.SetString(nameof(this.RegistryId), registryId);
                item.WriteToRBE(dictionary.CreateDictionary("Data"));
            }
        }

        public override void ReadFromRBE(RBEDictionary data) {
            base.ReadFromRBE(data);
            RBEList list = data.GetList(nameof(this.Items));
            foreach (RBEBase item in list.List) {
                if (!(item is RBEDictionary dictionary))
                    throw new Exception("Expected item list to contain only dictionaries, not " + item);
                string registryId = dictionary.GetString(nameof(this.RegistryId), null);
                if (string.IsNullOrEmpty(registryId))
                    throw new Exception("Missing the registry ID for item");
                RBEDictionary dataDictionary = dictionary.GetDictionary("Data");
                BaseResourceObject resource = ResourceTypeRegistry.Instance.CreateResourceItemModel(registryId);
                resource.ReadFromRBE(dataDictionary);
                this.Items.Add(resource);
            }
        }

        public void AddItemToList(BaseResourceObject value) {
            this.InsertItemIntoList(this.Items.Count, value);
        }

        public void InsertItemIntoList(int index, BaseResourceObject value) {
            this.Items.Insert(index, value);
            value.Group = this;
        }

        public bool RemoveItemFromList(BaseResourceObject value) {
            int index = this.Items.IndexOf(value);
            if (index < 0) {
                return false;
            }

            this.RemoveItemFromListAt(index);
            return true;
        }

        public void RemoveItemFromListAt(int index) {
            BaseResourceObject value = this.Items[index];
            this.Items.RemoveAt(index);
            value.Group = null;
        }

        public T Add<T>(T item) where T : BaseResourceObject {
            this.AddItemToList(item);
            return item;
        }
    }
}