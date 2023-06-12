using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FramePFX.Core.Editor.ResourceManaging.Events;
using FramePFX.Core.RBC;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.ResourceManaging {
    public class ResourceManager : IRBESerialisable {
        private const string EmptyIdErrorMessage = "ID cannot be null, empty or consist of entirely whitespaces";
        private readonly Dictionary<string, ResourceItem> uuidToItem;

        public ProjectModel Project { get; }

        public IEnumerable<(string, ResourceItem)> Entries => this.uuidToItem.Select(x => (x.Key, x.Value));

        public event ResourceItemEventHandler ResourceRegistered;
        public event ResourceItemEventHandler ResourceUnregistered;
        public event ResourceRenamedEventHandler ResourceRenamed;
        public event ResourceReplacedEventHandler ResourceReplaced;

        /// <summary>
        /// This manager's root resource group, which contains the tree of items. Registered entries
        /// are stored in this tree and in an internal dictionary (for speed purposes), therefore it is
        /// important that this tree is not modified unless the internal dictionary is also modified accordingly
        /// </summary>
        public ResourceGroup RootGroup { get; }

        public ResourceManager(ProjectModel project) {
            this.uuidToItem = new Dictionary<string, ResourceItem>();
            this.Project = project ?? throw new ArgumentNullException(nameof(project));
            this.RootGroup = new ResourceGroup();
            BaseResourceObject.SetManager(this.RootGroup, this);
        }

        public void WriteToRBE(RBEDictionary data) {
            this.RootGroup.WriteToRBE(data.CreateDictionary(nameof(this.RootGroup)));

            // Old way; write the entry dictionary
            // RBEList list = data.CreateList("Resources");
            // foreach (KeyValuePair<string, ResourceItem> entry in this.uuidToItem) {
            //     Debug.Assert(entry.Key == entry.Value.UniqueId, "Dictionary and entry's ID do not match");
            //     RBEDictionary dictionary = list.AddDictionary();
            //     if (string.IsNullOrWhiteSpace(entry.Value.UniqueId))
            //         throw new Exception("Item does not have a valid unique ID");
            //     if (!(entry.Value.RegistryId is string registryId))
            //         throw new Exception($"Model Type is not registered: {entry.Value.GetType()}");
            //     dictionary.SetString(nameof(entry.Value.RegistryId), registryId);
            //     dictionary.SetString(nameof(entry.Value.UniqueId), entry.Value.UniqueId);
            //     entry.Value.WriteToRBE(dictionary.CreateDictionary("Data"));
            // }
        }

        public void ReadFromRBE(RBEDictionary data) {
            if (this.uuidToItem.Count > 0) {
                throw new Exception("Cannot read data while resources are still registered");
            }

            this.RootGroup.ReadFromRBE(data.GetDictionary(nameof(this.RootGroup)));
            GetEntriesRecursive(this.RootGroup, this.uuidToItem);

            // Old way; read the entry dictionary
            // foreach (RBEBase entry in data.GetList("Resources").List) {
            //     if (!(entry is RBEDictionary dictionary))
            //         throw new Exception($"Resource dictionary contained a non dictionary child: {entry.Type}");
            //     string registryId = dictionary.GetString(nameof(ResourceItem.RegistryId));
            //     string uniqueId = dictionary.GetString(nameof(ResourceItem.UniqueId));
            //     if (string.IsNullOrWhiteSpace(uniqueId))
            //         throw new Exception("Data does not contain a valid unique ID");
            //     ResourceItem item = ResourceTypeRegistry.Instance.CreateResourceItemModel(this, registryId);
            //     item.ReadFromRBE(dictionary.GetDictionary("Data"));
            //     this.RegisterEntry(uniqueId, item);
            // }
        }

        private static void GetEntriesRecursive(BaseResourceObject obj, Dictionary<string, ResourceItem> resources) {
            if (obj is ResourceItem item) {
                if (resources.TryGetValue(item.UniqueId, out ResourceItem entry))
                    throw new Exception($"A resource already exists with the id '{item.UniqueId}': {entry}");
                resources[item.UniqueId] = item;
            }
            else if (obj is ResourceGroup group) {
                foreach (BaseResourceObject subItem in group.Items) {
                    GetEntriesRecursive(subItem, resources);
                }
            }
            else {
                throw new Exception($"Unknown resource object type: {obj}");
            }
        }

        public void RegisterEntry(string id, ResourceItem item) {
            if (item == null)
                throw new ArgumentNullException(nameof(item), "Item cannot be null");
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException(EmptyIdErrorMessage, nameof(id));
            if (this.uuidToItem.TryGetValue(id, out ResourceItem oldItem))
                throw new Exception($"Resource already exists with the id '{id}': {oldItem.GetType()}");
            this.uuidToItem[id] = item;
            ResourceItem.SetUniqueId(item, id);
            BaseResourceObject.SetManager(item, this);
            this.ResourceRegistered?.Invoke(this, item);
        }

        /// <summary>
        /// Replaces
        /// </summary>
        /// <param name="id"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public ResourceItem ReplaceEntry(string id, ResourceItem item) {
            if (item == null)
                throw new ArgumentNullException(nameof(item), "Item cannot be null");
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException(EmptyIdErrorMessage, nameof(id));
            this.uuidToItem.TryGetValue(id, out ResourceItem oldItem);
            this.uuidToItem[id] = item;
            if (oldItem != null) {
                BaseResourceObject.SetManager(oldItem, null);
            }

            ResourceItem.SetUniqueId(item, id);
            BaseResourceObject.SetManager(item, this);
            this.ResourceReplaced?.Invoke(this, id, oldItem, item);
            return oldItem;
        }

        public ResourceItem DeleteEntryById(string id) {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException(EmptyIdErrorMessage, nameof(id));
            if (!this.uuidToItem.TryGetValue(id, out ResourceItem item))
                return null;
            Debug.Assert(item.UniqueId == id, "Existing resource's ID does not equal the given ID; Corrupted application?");
            this.uuidToItem.Remove(id);
            BaseResourceObject.SetManager(item, null);
            this.ResourceUnregistered?.Invoke(this, item);
            return item;
        }

        public bool DeleteEntryByItem(ResourceItem item) {
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
            BaseResourceObject.SetManager(item, null);
            this.ResourceUnregistered?.Invoke(this, item);
            return true;
        }

        public void RenameEntry(ResourceItem item, string newId) {
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
            if (!ReferenceEquals(item, idItem))
                throw new InvalidOperationException($"Resource has the same Id as an existing resource, but the references do not match. {item} != {idItem}");
            this.uuidToItem.Remove(oldId);
            this.uuidToItem[newId] = item;
            ResourceItem.SetUniqueId(item, newId);
            this.ResourceRenamed?.Invoke(this, item, oldId, newId);
        }

        public void RenameEntry(string oldId, string newId) {
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

        public ResourceItem GetEntryItem(string id) {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException(EmptyIdErrorMessage, nameof(id));
            return this.uuidToItem.TryGetValue(id, out ResourceItem item) ? item : null;
        }

        public bool TryGetEntryItem(string id, out ResourceItem resource) {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException(EmptyIdErrorMessage, nameof(id));
            return this.uuidToItem.TryGetValue(id, out resource);
        }

        /// <summary>
        /// Checks if the given item is registered
        /// </summary>
        /// <param name="id">The Id to check</param>
        /// <returns>Whether or not the id is registered in the manager</returns>
        /// <exception cref="ArgumentException">The ID is null, empty or only whitespaces</exception>
        public bool EntryExists(string id) {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException(EmptyIdErrorMessage, nameof(id));
            return this.uuidToItem.ContainsKey(id);
        }

        /// <summary>
        /// Checks if the given item is registered
        /// </summary>
        /// <param name="item">The item to check</param>
        /// <param name="isRefEqual">Whether the given item's reference is equal to the actual registered item. This should be true. False means something has gone horribly wrong at some point!</param>
        /// <returns>Whether or not the item's id is registered in the manager</returns>
        /// <exception cref="ArgumentNullException">The item is null</exception>
        /// <exception cref="ArgumentException">The item's manager does not match the current instance</exception>
        /// <exception cref="Exception">The item's unique ID is null, empty or only whitespaces</exception>
        public bool EntryExists(ResourceItem item, out bool isRefEqual) {
            if (item == null)
                throw new ArgumentNullException(nameof(item), "Item cannot be null");
            if (!ReferenceEquals(this, item.Manager))
                throw new ArgumentException("Item's manager does not equal the current instance", nameof(item));
            if (string.IsNullOrWhiteSpace(item.UniqueId))
                throw new Exception("Item's ID is null, empty or only whitespaces");
            if (!this.uuidToItem.TryGetValue(item.UniqueId, out ResourceItem entryItem))
                return isRefEqual = false; // one liner ;)
            isRefEqual = ReferenceEquals(entryItem, item);
            return true;
        }

        public void ClearEntries() {
            using (ExceptionStack stack = new ExceptionStack()) {
                foreach (ResourceItem item in this.uuidToItem.Values) {
                    BaseResourceObject.SetManager(item, null);
                    try {
                        this.ResourceUnregistered?.Invoke(this, item);
                    }
                    catch (Exception e) {
                        stack.Push(e);
                    }
                }

                this.uuidToItem.Clear();
            }
        }
    }
}