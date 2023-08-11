using System;
using System.Collections.Generic;
using FramePFX.Core.Editor.ResourceManaging.Events;
using FramePFX.Core.RBC;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.ResourceManaging {
    /// <summary>
    /// Stores registered <see cref="ResourceItem"/> entries and maps <see cref="ResourceItem.UniqueId"/> to a <see cref="ResourceItem"/>
    /// </summary>
    public class ResourceManager : IRBESerialisable {
        private ulong currId; // starts at 0, incremented by GetNextId()
        public const ulong EmptyId = 0UL;
        private const string EmptyIdErrorMessage = "ID cannot be zero (null)";
        private readonly Dictionary<ulong, ResourceItem> uuidToItem;

        public Project Project { get; }

        public IEnumerable<KeyValuePair<ulong, ResourceItem>> Entries => this.uuidToItem;

        /// <summary>
        /// An event called when a resource is added to this manager
        /// </summary>
        public event ResourceItemEventHandler ResourceAdded;

        /// <summary>
        /// An event called when a resource is removed from this manager
        /// </summary>
        public event ResourceItemEventHandler ResourceRemoved;

        /// <summary>
        /// An event called when a resource is replaced with another resource
        /// </summary>
        public event ResourceReplacedEventHandler ResourceReplaced;

        /// <summary>
        /// This manager's root resource group, which contains the tree of resources. Registered entries are
        /// stored in this tree, and cached in an internal dictionary (for speed purposes), therefore it is
        /// important that this tree is not modified unless the internal dictionary is also modified accordingly
        /// </summary>
        public ResourceGroup RootGroup { get; }

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
            this.RootGroup = new ResourceGroup() {
                DisplayName = "<root>"
            };
            this.RootGroup.SetManager(this);

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
            this.RootGroup.WriteToRBE(data.CreateDictionary(nameof(this.RootGroup)));
            data.SetULong("CurrId", this.currId);
        }

        public void ReadFromRBE(RBEDictionary data) {
            if (this.uuidToItem.Count > 0)
                throw new Exception("Cannot read data while resources are still registered");

            this.RootGroup.ReadFromRBE(data.GetDictionary(nameof(this.RootGroup)));
            AccumulateEntriesRecursive(this.RootGroup, this.uuidToItem);
            this.currId = data.GetULong("CurrId", 0UL);
        }

        private static void AccumulateEntriesRecursive(BaseResourceObject obj, Dictionary<ulong, ResourceItem> resources) {
            if (obj is ResourceItem item) {
                if (resources.TryGetValue(item.UniqueId, out ResourceItem entry))
                    throw new Exception($"A resource already exists with the id '{item.UniqueId}': {entry}");
                resources[item.UniqueId] = item;
            }
            else if (obj is ResourceGroup group) {
                foreach (BaseResourceObject subItem in group.Items) {
                    AccumulateEntriesRecursive(subItem, resources);
                }
            }
            else {
                throw new Exception($"Unknown resource object type: {obj}");
            }
        }

        public ulong RegisterEntry(ResourceItem item) {
            if (item == null)
                throw new ArgumentNullException(nameof(item), "Item cannot be null");
            if (item.UniqueId != EmptyId && this.uuidToItem.TryGetValue(item.UniqueId, out ResourceItem oldItem))
                throw new Exception($"Resource is already registered with ID '{item.UniqueId}': {oldItem.GetType()}");
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

        /// <summary>
        /// Replaces an existing resource item instance with another instance, while maintaining the same ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public ResourceItem ReplaceEntry(ulong id, ResourceItem item) {
            if (item == null)
                throw new ArgumentNullException(nameof(item), "Item cannot be null");
            if (id == EmptyId)
                throw new ArgumentException(EmptyIdErrorMessage, nameof(id));
            this.uuidToItem.TryGetValue(id, out ResourceItem oldItem);
            this.uuidToItem[id] = item;
            ResourceItem.SetUniqueId(item, id);
            this.ResourceReplaced?.Invoke(this, id, oldItem, item);
            return oldItem;
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

        public bool DeleteEntryByItem(ResourceItem item) {
            if (item == null)
                throw new ArgumentNullException(nameof(item), "Item cannot be null");
            if (!ReferenceEquals(this, item.Manager))
                throw new ArgumentException("Item's manager does not equal the current instance", nameof(item));
            if (item.UniqueId == EmptyId)
                throw new ArgumentException("Item ID cannot be zero (null)", nameof(item));
            if (!this.uuidToItem.TryGetValue(item.UniqueId, out ResourceItem oldItem))
                return false;
#if DEBUG
            if (!ReferenceEquals(oldItem, item)) {
                System.Diagnostics.Debugger.Break();
                throw new Exception("Existing resource does not reference equal the given resource; Corrupted application?");
            }
#endif
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
            if (!TextIncrement.GenerateFileString(accept, filePath, out string file))
                return (output = null) == null;
            return TextIncrement.GetIncrementableString(accept, $"{file}::{streamName}", out output, 10000);
        }

        #endregion
    }
}