using System;
using System.Collections.Generic;
using System.Linq;
using FramePFX.Editors.ResourceManaging.Events;
using FramePFX.RBC;
using FramePFX.Utils;

namespace FramePFX.Editors.ResourceManaging {
    public delegate void CurrentFolderChangedEventHandler(ResourceManager manager, ResourceFolder oldFolder, ResourceFolder newFolder);

    /// <summary>
    /// Stores registered <see cref="ResourceItem"/> entries and maps <see cref="ResourceItem.UniqueId"/> to a <see cref="ResourceItem"/>
    /// </summary>
    public class ResourceManager {
        private ulong currId; // starts at 0, incremented by GetNextId()
        public const ulong EmptyId = 0UL;
        private const string EmptyIdErrorMessage = "ID cannot be zero (null)";
        private readonly Dictionary<ulong, ResourceItem> uuidToItem;
        private readonly HashSet<BaseResource> selectedItems;
        private ResourceFolder currentFolder;

        /// <summary>
        /// Gets the project that owns this resource manager
        /// </summary>
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
        public ResourceFolder RootContainer { get; }

        /// <summary>
        /// Gets or sets the current folder that is being displayed to the user. This value will never be null,
        /// and assigning it to null will result in it becoming <see cref="RootContainer"/>
        /// </summary>
        public ResourceFolder CurrentFolder {
            get => this.currentFolder;
            set {
                if (value == null)
                    value = this.RootContainer;
                ResourceFolder oldFolder = this.currentFolder;
                if (oldFolder == value)
                    return;
                this.currentFolder = value;
                this.CurrentFolderChanged?.Invoke(this, oldFolder, value);
            }
        }

        public IReadOnlyCollection<BaseResource> SelectedItems => this.selectedItems;

        /// <summary>
        /// A predicate that returns false when <see cref="EntryExists(ulong)"/> returns true
        /// </summary>
        public Predicate<ulong> IsResourceNotInUsePredicate { get; }

        /// <summary>
        /// A predicate that returns <see cref="EntryExists(ulong)"/>
        /// </summary>
        public Predicate<ulong> IsResourceInUsePredicate { get; }

        public event CurrentFolderChangedEventHandler CurrentFolderChanged;

        public ResourceManager(Project project) {
            this.Project = project ?? throw new ArgumentNullException(nameof(project));
            this.uuidToItem = new Dictionary<ulong, ResourceItem>();
            this.selectedItems = new HashSet<BaseResource>(64, new ReferenceEqualityComparer<BaseResource>());
            this.RootContainer = new ResourceFolder() {DisplayName = "<root>"};
            BaseResource.SetManagerForRootFolder(this.RootContainer, this);
            this.currentFolder = this.RootContainer;
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
            this.RootContainer.WriteToRBE(data.CreateDictionary(nameof(this.RootContainer)));
            data.SetULong("CurrId", this.currId);
        }

        public void ReadFromRBE(RBEDictionary data) {
            if (this.uuidToItem.Count > 0)
                throw new Exception("Cannot read data while resources are still registered");

            this.RootContainer.ReadFromRBE(data.GetDictionary(nameof(this.RootContainer)));
            this.AccumulateEntriesRecursive(this.RootContainer);
            this.currId = data.GetULong("CurrId", 0UL);
        }

        private void AccumulateEntriesRecursive(BaseResource obj) {
            if (obj is ResourceItem item) {
                if (item.UniqueId == EmptyId)
                    throw new Exception("Deserialised resource has an empty ID: " + item.GetType());
                if (this.uuidToItem.TryGetValue(item.UniqueId, out ResourceItem entry))
                    throw new Exception($"A resource already exists with the id '{item.UniqueId}': {entry}");
                this.uuidToItem[item.UniqueId] = item;
                this.ResourceAdded?.Invoke(this, item);
            }
            else if (obj is ResourceFolder group) {
                foreach (BaseResource subItem in group.Items) {
                    this.AccumulateEntriesRecursive(subItem);
                }
            }
            else {
                throw new Exception($"Unknown resource object type: {obj}");
            }
        }

        /// <summary>
        /// Registers the resource item with a randomly generated unique ID. The resource must exist in a resource
        /// tree (as in, its <see cref="BaseResource.Parent"/> cannot be null) and its <see cref="ResourceItem.UniqueId"/>
        /// must be empty (meaning, unregistered) otherwise an exception will be thrown
        /// </summary>
        /// <param name="item">The item to register</param>
        /// <returns></returns>
        private ulong RegisterEntry(ResourceItem item) {
            if (item == null)
                throw new ArgumentNullException(nameof(item), "Item cannot be null");
            if (item.UniqueId != EmptyId)
                throw new Exception("Item already has an ID associated with it");
            if (item.Manager == null)
                throw new Exception("Item does not exist in a resource tree; it cannot be registered");
            this.RegisterEntryInternal(this.GetNextId(), item);
            return item.UniqueId;
        }

        private void RegisterEntry(ulong id, ResourceItem item) {
            if (id == EmptyId)
                throw new ArgumentException(EmptyIdErrorMessage, nameof(id));
            if (item == null)
                throw new ArgumentNullException(nameof(item), "Item cannot be null");
            if (item.UniqueId != EmptyId)
                throw new Exception("Item already has an ID associated with it");
            if (item.Manager == null)
                throw new Exception("Item does not exist in a resource tree; it cannot be registered");
            this.RegisterEntryInternal(id, item);
        }

        private void RegisterEntryInternal(ulong id, ResourceItem item) {
            this.uuidToItem[id] = item;
            ResourceItem.SetUniqueId(item, id);
            this.ResourceAdded?.Invoke(this, item);
        }

        private ResourceItem UnregisterItemById(ulong id) {
            if (id == EmptyId)
                throw new ArgumentException(EmptyIdErrorMessage, nameof(id));
            if (!this.uuidToItem.TryGetValue(id, out ResourceItem item))
                return null;
            if (item.UniqueId != id) {
                System.Diagnostics.Debugger.Break();
                throw new Exception("Existing resource's ID does not equal the given ID; Corrupted application?");
            }

            this.uuidToItem.Remove(id);
            ResourceItem.SetUniqueId(item, EmptyId);
            this.ResourceRemoved?.Invoke(this, item);
            return item;
        }

        private bool UnregisterItem(ResourceItem item) {
            if (item == null)
                throw new ArgumentNullException(nameof(item), "Item cannot be null");
            if (item.UniqueId == EmptyId)
                return false;
            if (!ReferenceEquals(item.Manager, this))
                return false;
            if (!this.uuidToItem.Remove(item.UniqueId)) {
                System.Diagnostics.Debugger.Break();
                throw new Exception("Corrupted application data");
            }

            ResourceItem.SetUniqueId(item, EmptyId);
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
            isRefEqual = false;
            if (item == null)
                throw new ArgumentNullException(nameof(item), "Item cannot be null");
            if (item.UniqueId == EmptyId)
                return false;
            if (!ReferenceEquals(this, item.Manager))
                return false;
            if (!this.uuidToItem.TryGetValue(item.UniqueId, out ResourceItem entryItem))
                throw new Exception("Corrupted application: entry's manager equals this instance but does not exist in the map");
            isRefEqual = ReferenceEquals(entryItem, item);
            return true;
        }

        public void ClearEntries() {
            using (ErrorList stack = new ErrorList()) {
                foreach (KeyValuePair<ulong, ResourceItem> entry in this.uuidToItem.ToList()) {
                    try {
                        this.UnregisterItem(entry.Value);
                    }
                    catch (Exception e) {
                        stack.Add(e);
                    }
                }
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

        public static void UpdateSelection(BaseResource resource) {
            ResourceManager manager = resource.Manager;
            if (manager != null) {
                if (resource.IsSelected) {
                    manager.selectedItems.Add(resource);
                }
                else {
                    manager.selectedItems.Remove(resource);
                }
            }
        }

        #endregion

        internal static void InternalRegister(ResourceItem item) {
            item.Manager.RegisterEntry(item);
        }

        internal static void InternalUnregister(ResourceItem item) {
            item.Manager.UnregisterItem(item);
        }
    }
}