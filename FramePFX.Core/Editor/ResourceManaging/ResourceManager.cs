using System;
using System.Collections.Generic;
using System.Linq;

namespace FramePFX.Core.Editor.ResourceManaging {
    public class ResourceManager {
        public delegate void ResourceEventHandler(ResourceManager manager, ResourceItem item);
        public delegate void ResourceRenamedEventHandler(ResourceManager manager, ResourceItem item, string oldId, string newId);

        private readonly Dictionary<string, ResourceItem> uuidToItem;

        public ProjectModel Project { get; }

        public IEnumerable<(string, ResourceItem)> Items => this.uuidToItem.Select(x => (x.Key, x.Value));

        public event ResourceEventHandler ResourceAdded;
        public event ResourceEventHandler ResourceRemoved;
        public event ResourceRenamedEventHandler ResourceRenamed;

        public ResourceManager(ProjectModel project) {
            this.uuidToItem = new Dictionary<string, ResourceItem>();
            this.Project = project ?? throw new ArgumentNullException(nameof(project));
        }

        public ResourceItem AddResource(string id, ResourceItem item) {
            if (item == null)
                throw new ArgumentNullException(nameof(item), "ResourceItem cannot be null");
            ValidateId(id);
            this.uuidToItem.TryGetValue(id, out ResourceItem oldItem);
            this.uuidToItem[id] = item;
            item.UniqueId = id;
            this.ResourceAdded?.Invoke(this, item);
            return oldItem;
        }

        public ResourceItem RemoveItem(string id) {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("ID cannot be null, empty or consist of entirely whitespaces", nameof(id));
            if (this.uuidToItem.TryGetValue(id, out ResourceItem item)) {
                if (item.UniqueId != id) {
                    throw new Exception($"Resource manager corrupt; mapped {id} to {item.GetType()} but the item's unique ID was {item.UniqueId}");
                }

                this.uuidToItem.Remove(id);
                this.ResourceRemoved?.Invoke(this, item);
                return item;
            }
            else {
                return null;
            }
        }

        public bool RemoveItem(ResourceItem item) {
            if (item == null)
                throw new ArgumentNullException(nameof(item), "ResourceItem cannot be null");
            if (string.IsNullOrWhiteSpace(item.UniqueId))
                throw new ArgumentException("Item ID cannot be null, empty or consist of entirely whitespaces", nameof(item));
            if (this.uuidToItem.Remove(item.UniqueId)) {
                this.ResourceRemoved?.Invoke(this, item);
            }

            return false;
        }

        public bool RenameResource(ResourceItem item, string newId) {
            if (item == null)
                throw new ArgumentNullException(nameof(item), "ResourceItem cannot be null");
            string oldId = item.UniqueId;
            if (string.IsNullOrWhiteSpace(oldId))
                throw new ArgumentException("Old Item ID cannot be null, empty or consist of entirely whitespaces. Did you mean to add the resource?", nameof(item));
            if (string.IsNullOrWhiteSpace(newId))
                throw new ArgumentException("New ID cannot be null, empty or consist of entirely whitespaces", nameof(newId));
            if (this.uuidToItem.TryGetValue(oldId, out ResourceItem idItem)) {
                if (ReferenceEquals(item, idItem)) {
                    this.uuidToItem.Remove(oldId);
                    this.uuidToItem[newId] = item;
                    item.UniqueId = newId;
                    this.ResourceRenamed?.Invoke(this, item, oldId, newId);
                    return true;
                }
                else {
                    throw new Exception($"Existing item and parameter item have the same ID but are not reference equal. Id = {item.UniqueId}");
                }
            }
            else {
                return false;
            }
        }

        public bool RenameResource(string oldId, string newId) {
            if (string.IsNullOrWhiteSpace(oldId))
                throw new ArgumentException("Old ID cannot be null, empty or consist of entirely whitespaces", nameof(oldId));
            if (string.IsNullOrWhiteSpace(newId))
                throw new ArgumentException("New ID cannot be null, empty or consist of entirely whitespaces", nameof(newId));
            if (this.uuidToItem.TryGetValue(oldId, out ResourceItem item)) {
                item.UniqueId = newId;
                return true;
            }
            else {
                return false;
            }
        }

        public static void ValidateId(string id, string paramName = "id") {
            if (string.IsNullOrWhiteSpace(id)) {
                throw new ArgumentException("ID cannot be null, empty or consist of entirely whitespaces", paramName ?? "id");
            }
        }

        public ResourceItem GetResource(string id) {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("ID cannot be null, empty or consist of entirely whitespaces", nameof(id));
            return this.uuidToItem.TryGetValue(id, out ResourceItem item) ? item : null;
        }

        public bool TryGetResource(string id, out ResourceItem resource) {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("ID cannot be null, empty or consist of entirely whitespaces", nameof(id));
            return this.uuidToItem.TryGetValue(id, out resource);
        }

        public bool ResourceExists(string id) {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("ID cannot be null, empty or consist of entirely whitespaces", nameof(id));
            return this.uuidToItem.ContainsKey(id);
        }

        public bool ResourceExists(ResourceItem item) {
            if (item == null)
                throw new ArgumentNullException(nameof(item), "ResourceItem cannot be null");
            return this.ResourceExists(item.UniqueId);
        }

        public void AddHandler(string id, ResourceEventHandler handler) {
            
        }
    }
}