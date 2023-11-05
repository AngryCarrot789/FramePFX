using System;
using System.Collections.Generic;
using FramePFX.Editor.ResourceManaging.Events;
using FramePFX.Logger;
using FramePFX.RBC;
using FramePFX.Utils;

namespace FramePFX.Editor.ResourceManaging {
    /// <summary>
    /// Stores registered <see cref="ResourceItem"/> entries and maps <see cref="ResourceItem.UniqueId"/> to a <see cref="ResourceItem"/>
    /// </summary>
    public class ResourceManager {
        private ulong currId; // starts at 0, incremented by GetNextId()
        public const ulong EmptyId = 0UL;
        private const string EmptyIdErrorMessage = "ID cannot be zero (null)";
        private readonly Dictionary<ulong, ResourceItem> uuidToItem;

        public Project Project { get; }

        public IEnumerable<KeyValuePair<ulong, ResourceItem>> Entries => this.uuidToItem;

        /// <summary>
        /// An event called when a resource is added to this manager
        /// </summary>
        public event ResourceAndManagerEventHandler ResourceAdded;

        /// <summary>
        /// An event called when a resource is removed from this manager
        /// </summary>
        public event ResourceAndManagerEventHandler ResourceRemoved;

        /// <summary>
        /// This manager's root resource folder, which contains the tree of resources. Registered entries are
        /// stored in this tree, and cached in an internal dictionary (for speed purposes), therefore it is
        /// important that this tree is not modified unless the internal dictionary is also modified accordingly
        /// </summary>
        public ResourceFolder RootFolder { get; }

        /// <summary>
        /// A predicate that returns false when <see cref="EntryExists(ulong)"/> returns true
        /// </summary>
        public Predicate<ulong> IsResourceNotInUsePredicate { get; }

        /// <summary>
        /// A predicate that returns <see cref="EntryExists(ulong)"/>
        /// </summary>
        public Predicate<ulong> IsResourceInUsePredicate { get; }

        public ResourceManager(Project project) {
            this.uuidToItem = new Dictionary<ulong, ResourceItem>();
            this.Project = project ?? throw new ArgumentNullException(nameof(project));
            this.RootFolder = new ResourceFolder() {DisplayName = "<root>"};
            this.RootFolder.manager = this;
            this.RootFolder.OnAttachedToManager();
            this.IsResourceNotInUsePredicate = s => !this.EntryExists(s);
            this.IsResourceInUsePredicate = this.EntryExists;
        }

        public ulong GetNextId() {
            // assuming a CPU can somehow call GetNextId() 3 billion times in 1 second, it
            // would take roughly 97 years to reach ulong.MaxValue. LOL
            // That is unless it gets set maliciously either via modifying the
            // saved config's data or through cheat engine or something
            ulong id = this.currId;
            do {
                id++;
            } while (this.uuidToItem.ContainsKey(id) && id != 0);
            return this.currId = id;
        }

        public void WriteToRBE(RBEDictionary data) {
            this.RootFolder.WriteToRBE(data.CreateDictionary(nameof(this.RootFolder)));
            data.SetULong("CurrId", this.currId);
        }

        public void ReadFromRBE(RBEDictionary data) {
            if (this.uuidToItem.Count > 0)
                throw new Exception("Cannot read data while resources are still registered");

            this.RootFolder.ReadFromRBE(data.GetDictionary(nameof(this.RootFolder)));
            AccumulateEntriesRecursive(this, this.RootFolder, this.uuidToItem);
            this.currId = data.GetULong("CurrId", 0UL);
        }

        private static void AccumulateEntriesRecursive(ResourceManager manager, BaseResource obj, Dictionary<ulong, ResourceItem> resources) {
            if (obj is ResourceItem item) {
                if (resources.TryGetValue(item.UniqueId, out ResourceItem entry))
                    throw new Exception($"A resource already exists with the id '{item.UniqueId}': {entry}");
                resources[item.UniqueId] = item;
                manager.ResourceAdded?.Invoke(manager, item);
            }
            else if (obj is ResourceFolder group) {
                foreach (BaseResource subItem in group.Items) {
                    AccumulateEntriesRecursive(manager, subItem, resources);
                }
            }
            else {
                throw new Exception($"Unknown resource object type: {obj}");
            }
        }

        public ulong RegisterEntry(ResourceItem item) {
            if (item == null)
                throw new ArgumentNullException(nameof(item), "Item cannot be null");
            if (item.UniqueId != EmptyId) {
                if (this.uuidToItem.TryGetValue(item.UniqueId, out ResourceItem oldItem))
                    throw new Exception($"Resource is already registered in this manager with ID '{item.UniqueId}': {oldItem.GetType()}");
                if (item.Manager != null && item.Manager.uuidToItem.TryGetValue(item.UniqueId, out oldItem))
                    throw new Exception($"Resource is already registered in another manager with ID '{item.UniqueId}': {oldItem.GetType()}");
            }

            if (item.Manager != null && item.Manager != this)
                throw new ArgumentException("Item's manager was non-null and did not equal the current instance");

            ulong id = this.GetNextId();
            this.uuidToItem[id] = item;
            ResourceItem.SetUniqueId(item, id);
            this.ResourceAdded?.Invoke(this, item);
            return id;
        }

        public void RegisterEntry(ulong id, ResourceItem item) {
            if (item == null)
                throw new ArgumentNullException(nameof(item), "Item cannot be null");
            if (id == EmptyId)
                throw new ArgumentException(EmptyIdErrorMessage, nameof(id));
            if (this.uuidToItem.TryGetValue(id, out ResourceItem oldItem))
                throw new Exception($"Resource already exists with the id '{id}': {oldItem.GetType()}");
            this.uuidToItem[id] = item;
            ResourceItem.SetUniqueId(item, id);
            this.ResourceAdded?.Invoke(this, item);
        }

        public ResourceItem DeleteEntryById(ulong id) {
            if (id == EmptyId)
                throw new ArgumentException(EmptyIdErrorMessage, nameof(id));
            if (!this.uuidToItem.TryGetValue(id, out ResourceItem item))
                return null;
#if DEBUG
            if (item.UniqueId != id) {
                System.Diagnostics.Debugger.Break();
                throw new Exception("Existing resource's ID does not equal the given ID; Corrupted application?");
            }
#endif
            this.uuidToItem.Remove(id);
            this.ResourceRemoved?.Invoke(this, item);
            return item;
        }

        public bool RemoveEntryByItem(ResourceItem item) {
            if (item == null)
                throw new ArgumentNullException(nameof(item), "Item cannot be null");
            if (!ReferenceEquals(this, item.Manager))
                throw new ArgumentException("Item's manager does not equal the current instance", nameof(item));
            if (item.UniqueId == EmptyId)
                throw new ArgumentException("Item ID cannot be zero (null)", nameof(item));
            if (!this.uuidToItem.TryGetValue(item.UniqueId, out ResourceItem oldItem))
                return false;
            if (!ReferenceEquals(oldItem, item))
                throw new Exception("Existing resource does not reference equal the given resource; Corrupted application?");

            this.uuidToItem.Remove(item.UniqueId);
            this.ResourceRemoved?.Invoke(this, item);
            return true;
        }

        public ResourceItem GetEntryItem(ulong id) {
            if (id == EmptyId)
                throw new ArgumentException(EmptyIdErrorMessage, nameof(id));
            return this.uuidToItem[id];
        }

        public bool TryGetEntryItem(ulong id, out ResourceItem resource) {
            if (id == EmptyId)
                throw new ArgumentException(EmptyIdErrorMessage, nameof(id));
            return this.uuidToItem.TryGetValue(id, out resource);
        }

        /// <summary>
        /// Checks if the given item is registered
        /// </summary>
        /// <param name="id">The Id to check</param>
        /// <returns>Whether or not the id is registered in the manager</returns>
        /// <exception cref="ArgumentException">The ID is null, empty or only whitespaces</exception>
        public bool EntryExists(ulong id) {
            if (id == EmptyId)
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
            if (item.UniqueId == EmptyId)
                throw new Exception("Item's ID cannot be zero (null)");
            if (!this.uuidToItem.TryGetValue(item.UniqueId, out ResourceItem entryItem))
                return isRefEqual = false; // one liner ;)
            isRefEqual = ReferenceEquals(entryItem, item);
            return true;
        }

        public void ClearEntries() {
            using (ErrorList stack = new ErrorList()) {
                foreach (ResourceItem item in this.uuidToItem.Values) {
                    try {
                        this.ResourceRemoved?.Invoke(this, item);
                    }
                    catch (Exception e) {
                        stack.Add(e);
                    }
                }

                this.uuidToItem.Clear();
            }
        }

        #region Static Helper Functions

        // For the most part, these functions below should never return false due the the fact that a user would
        // need millions of added resources. Their system would run out of RAM before these functions fail

        public static bool GetDisplayNameForMediaStream(Predicate<string> accept, out string output, string filePath, string streamName) {
            if (!TextIncrement.GenerateFileString(accept, filePath, out string file)) {
                output = null;
                return true;
            }

            return TextIncrement.GetIncrementableString(accept, $"{file}::{streamName}", out output, 10000);
        }

        #endregion

        public void OnProjectLoaded() {
            this.RootFolder.OnProjectLoaded();
        }

        public void OnProjectUnloaded() {
            this.RootFolder.OnProjectUnloaded();
        }
    }
}