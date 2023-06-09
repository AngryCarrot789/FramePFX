using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FramePFX.Core.Editor.ResourceManaging.Events;
using FramePFX.Core.RBC;

namespace FramePFX.Core.Editor.ResourceManaging {
    public class ResourceManager : IRBESerialisable {
        private const string EmptyIdErrorMessage = "ID cannot be null, empty or consist of entirely whitespaces";
        private readonly Dictionary<string, ResourceItem> uuidToItem;

        public ProjectModel Project { get; }

        public IEnumerable<(string, ResourceItem)> Items => this.uuidToItem.Select(x => (x.Key, x.Value));

        public event ResourceItemEventHandler ResourceAdded;
        public event ResourceItemEventHandler ResourceDeleted;
        public event ResourceRenamedEventHandler ResourceRenamed;
        public event ResourceReplacedEventHandler ResourceReplaced;

        public ResourceManager(ProjectModel project) {
            this.uuidToItem = new Dictionary<string, ResourceItem>();
            this.Project = project ?? throw new ArgumentNullException(nameof(project));
        }

        public void WriteToRBE(RBEDictionary data) {
            RBEList list = data.CreateList("Resources");
            foreach (KeyValuePair<string, ResourceItem> entry in this.uuidToItem) {
                Debug.Assert(entry.Key == entry.Value.UniqueId, "Dictionary and entry's ID do not match");
                RBEDictionary dictionary = list.AddDictionary();
                if (string.IsNullOrWhiteSpace(entry.Value.UniqueId))
                    throw new Exception("Item does not have a valid unique ID");
                if (!(entry.Value.RegistryId is string registryId))
                    throw new Exception($"Model Type is not registered: {entry.Value.GetType()}");
                dictionary.SetString(nameof(entry.Value.RegistryId), registryId);
                dictionary.SetString(nameof(entry.Value.UniqueId), entry.Value.UniqueId);
                entry.Value.WriteToRBE(dictionary.CreateDictionary("Data"));
            }
        }

        public void ReadFromRBE(RBEDictionary data) {
            foreach (RBEBase entry in data.GetList("Resources").List) {
                if (!(entry is RBEDictionary dictionary))
                    throw new Exception($"Resource dictionary contained a non dictionary child: {entry.Type}");
                string registryId = dictionary.GetString(nameof(ResourceItem.RegistryId));
                string uniqueId = dictionary.GetString(nameof(ResourceItem.UniqueId));
                if (string.IsNullOrWhiteSpace(uniqueId))
                    throw new Exception("Data does not contain a valid unique ID");
                ResourceItem item = ResourceTypeRegistry.Instance.CreateResourceModel(this, registryId);
                item.ReadFromRBE(dictionary.GetDictionary("Data"));
                this.AddResource(uniqueId, item);
            }
        }

        public void AddResource(string id, ResourceItem item) {
            if (item == null)
                throw new ArgumentNullException(nameof(item), "Item cannot be null");
            if (!ReferenceEquals(this, item.Manager))
                throw new ArgumentException("Item's manager does not equal the current instance", nameof(item));
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException(EmptyIdErrorMessage, nameof(id));
            if (this.uuidToItem.TryGetValue(id, out ResourceItem oldItem))
                throw new Exception($"Resource already exists with the id '{id}': {oldItem.GetType()}");
            this.uuidToItem[id] = item;
            ResourceItem.SetUniqueId(item, id);
            this.ResourceAdded?.Invoke(this, item);
        }

        /// <summary>
        /// Replaces
        /// </summary>
        /// <param name="id"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public ResourceItem ReplaceResource(string id, ResourceItem item) {
            if (item == null)
                throw new ArgumentNullException(nameof(item), "Item cannot be null");
            if (!ReferenceEquals(this, item.Manager))
                throw new ArgumentException("Item's manager does not equal the current instance", nameof(item));
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException(EmptyIdErrorMessage, nameof(id));
            this.uuidToItem.TryGetValue(id, out ResourceItem oldItem);
            this.uuidToItem[id] = item;
            ResourceItem.SetUniqueId(item, id);
            this.ResourceReplaced?.Invoke(this, id, oldItem, item);
            return oldItem;
        }

        public ResourceItem DeleteItemById(string id) {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException(EmptyIdErrorMessage, nameof(id));
            if (!this.uuidToItem.TryGetValue(id, out ResourceItem item))
                return null;
            Debug.Assert(item.UniqueId == id, "Existing resource's ID does not equal the given ID; Corrupted application?");
            this.uuidToItem.Remove(id);
            this.ResourceDeleted?.Invoke(this, item);
            return item;
        }

        public bool DeleteItem(ResourceItem item) {
            if (item == null)
                throw new ArgumentNullException(nameof(item), "Item cannot be null");
            if (!ReferenceEquals(this, item.Manager))
                throw new ArgumentException("Item's manager does not equal the current instance", nameof(item));
            if (string.IsNullOrWhiteSpace(item.UniqueId))
                throw new ArgumentException("Item ID cannot be null, empty or consist of entirely whitespaces", nameof(item));
            if (!this.uuidToItem.TryGetValue(item.UniqueId, out ResourceItem oldItem))
                return false;
            Debug.Assert(ReferenceEquals(oldItem, item), "Existing resource does not equal the given resource; Corrupted application?");
            this.uuidToItem.Remove(item.UniqueId);
            this.ResourceDeleted?.Invoke(this, item);
            return true;
        }

        public void RenameResource(ResourceItem item, string newId) {
            if (item == null)
                throw new ArgumentNullException(nameof(item), "Item cannot be null");
            if (!ReferenceEquals(this, item.Manager))
                throw new ArgumentException("Item's manager does not equal the current instance", nameof(item));
            string oldId = item.UniqueId;
            if (string.IsNullOrWhiteSpace(oldId))
                throw new ArgumentException("Old Item ID cannot be null, empty or consist of entirely whitespaces. Did you mean to add the resource?", nameof(item));
            if (string.IsNullOrWhiteSpace(newId))
                throw new ArgumentException("New ID cannot be null, empty or consist of entirely whitespaces", nameof(newId));
            if (this.uuidToItem.TryGetValue(newId, out ResourceItem existing))
                throw new InvalidOperationException($"Resource already exists with the new id '{newId}': {existing.GetType()}");
            if (!this.uuidToItem.TryGetValue(oldId, out ResourceItem idItem))
                throw new InvalidOperationException($"Resource does not exist with the old id '{oldId}'");
            Debug.Assert(ReferenceEquals(item, idItem), "Target resource does not match the existing item but they have the same ID");
            this.uuidToItem.Remove(oldId);
            this.uuidToItem[newId] = item;
            ResourceItem.SetUniqueId(item, newId);
            this.ResourceRenamed?.Invoke(this, item, oldId, newId);
        }

        public void RenameResource(string oldId, string newId) {
            if (string.IsNullOrWhiteSpace(oldId))
                throw new ArgumentException("Old ID cannot be null, empty or consist of entirely whitespaces", nameof(newId));
            if (string.IsNullOrWhiteSpace(newId))
                throw new ArgumentException("New ID cannot be null, empty or consist of entirely whitespaces", nameof(newId));
            if (!this.uuidToItem.TryGetValue(oldId, out ResourceItem item))
                throw new InvalidOperationException($"Resource does not exist with the old id '{oldId}'");
            if (this.uuidToItem.TryGetValue(newId, out ResourceItem existing))
                throw new InvalidOperationException($"Resource already exists with the new id '{newId}': {existing.GetType()}");
            this.uuidToItem.Remove(oldId);
            this.uuidToItem[newId] = item;
            ResourceItem.SetUniqueId(item, newId);
            this.ResourceRenamed?.Invoke(this, item, oldId, newId);
        }

        public ResourceItem GetResource(string id) {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException(EmptyIdErrorMessage, nameof(id));
            return this.uuidToItem.TryGetValue(id, out ResourceItem item) ? item : null;
        }

        public bool TryGetResource(string id, out ResourceItem resource) {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException(EmptyIdErrorMessage, nameof(id));
            return this.uuidToItem.TryGetValue(id, out resource);
        }

        public bool ResourceExists(string id) {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException(EmptyIdErrorMessage, nameof(id));
            return this.uuidToItem.ContainsKey(id);
        }

        public bool ResourceExists(ResourceItem item) {
            if (item == null)
                throw new ArgumentNullException(nameof(item), "Item cannot be null");
            if (!ReferenceEquals(this, item.Manager))
                throw new ArgumentException("Item's manager does not equal the current instance", nameof(item));
            return !string.IsNullOrWhiteSpace(item.UniqueId) && this.uuidToItem.ContainsKey(item.UniqueId);
        }
    }
}