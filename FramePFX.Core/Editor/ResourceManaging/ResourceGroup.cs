using System;
using System.Collections.Generic;
using FramePFX.Core.Editor.Registries;
using FramePFX.Core.RBC;
using FramePFX.Core.Utils;

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
                if (!(item.RegistryId is string registryId))
                    throw new Exception($"Resource type is not registered: {this.GetType()}");
                RBEDictionary dictionary = list.AddDictionary();
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

        public void AddItemToList(BaseResourceObject value, bool setManager = true) {
            this.InsertItemIntoList(this.items.Count, value, setManager);
        }

        /// <summary>
        /// Helper function for calling <see cref="AddItemToList"/> and returning the parameter value
        /// </summary>
        /// <param name="item"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Add<T>(T item) where T : BaseResourceObject {
            this.AddItemToList(item);
            return item;
        }

        public void InsertItemIntoList(int index, BaseResourceObject value, bool setManager = true) {
            this.items.Insert(index, value);
            value.SetParent(this);
            if (setManager) {
                value.SetManager(this.Manager);
            }
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
            value.SetParent(null);
        }

        /// <summary>
        /// Recursively sets this items' manager and group to null, then clears the collection
        /// </summary>
        public void Clear() {
            foreach (BaseResourceObject item in this.items) {
                item.SetManager(null);
                item.SetParent(null);
            }

            this.items.Clear();
        }

        /// <summary>
        /// Clears this group's items, without setting the items' parent group or manager to null
        /// </summary>
        public void UnsafeClear() {
            this.items.Clear();
        }

        protected override void DisposeCore(ExceptionStack stack) {
            base.DisposeCore(stack);
            foreach (BaseResourceObject resource in this.items) {
                try {
                    resource.Dispose();
                }
                catch (Exception e) {
                    stack.Add(new Exception("Disposing resource", e));
                }
            }
        }
    }
}