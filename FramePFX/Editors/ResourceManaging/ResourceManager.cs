//
// Copyright (c) 2023-2024 REghZy
//
// This file is part of FramePFX.
//
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
//
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FramePFX.Editors.ResourceManaging.Events;
using FramePFX.RBC;
using FramePFX.Utils;
using FramePFX.Utils.Destroying;

namespace FramePFX.Editors.ResourceManaging {
    public delegate void CurrentFolderChangedEventHandler(ResourceManager manager, ResourceFolder oldFolder, ResourceFolder newFolder);

    /// <summary>
    /// Stores registered <see cref="ResourceItem"/> entries and maps <see cref="ResourceItem.UniqueId"/> to a <see cref="ResourceItem"/>
    /// </summary>
    public class ResourceManager : IDestroy {
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

        /// <summary>
        /// Maps a <see cref="ResourceItem"/>'s <see cref="ResourceItem.UniqueId"/> to the resource itself.
        /// This dictionary is updated whenever a resource is added or removed from this resource manager folder
        /// hierarchy. We use a dictionary for performance reasons, as traversing the folder hierarchy to find a
        /// resource from its ID can take a long time if there are lots of resources
        /// </summary>
        public IReadOnlyDictionary<ulong, ResourceItem> EntryMap => this.uuidToItem;

        /// <summary>
        /// An event called when a resource is added to this manager
        /// </summary>
        public event ResourceAndManagerEventHandler ResourceAdded;

        /// <summary>
        /// An event called when a resource is removed from this manager
        /// </summary>
        public event ResourceAndManagerEventHandler ResourceRemoved;

        /// <summary>
        /// This manager's root resource folder, which contains the tree of resources. All
        /// <see cref="ResourceItem"/> objects in this tree are cached in an internal dictionary
        /// (for speed purposes). See the <see cref="EntryMap"/> docs for more info
        /// </summary>
        public ResourceFolder RootContainer { get; }

        /// <summary>
        /// Gets or sets the current folder that is being displayed to the user. This value will never be null,
        /// and assigning it to null will result in <see cref="RootContainer"/> being used instead
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
            BaseResource.InternalSetManagerForRootFolder(this.RootContainer, this);
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
            } while (this.uuidToItem.ContainsKey(id));

            return this.currId = id;
        }

        public void WriteToRBE(RBEDictionary data) {
            BaseResource.SerialisationRegistry.Serialise(this.RootContainer, data.CreateDictionary(nameof(this.RootContainer)));
            data.SetULong("CurrId", this.currId);
        }

        public void ReadFromRBE(RBEDictionary data) {
            if (this.uuidToItem.Count > 0)
                throw new Exception("Cannot read data while resources are still registered");

            this.currId = data.GetULong("CurrId", 0UL);
            BaseResource.SerialisationRegistry.Deserialise(this.RootContainer, data.GetDictionary(nameof(this.RootContainer)));
        }

        private void RegisterEntryInternal(ulong id, ResourceItem item) {
            this.uuidToItem[id] = item;
            ResourceItem.SetUniqueId(item, id);
            this.ResourceAdded?.Invoke(this, item);
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

            this.ResourceRemoved?.Invoke(this, item);
            ResourceItem.SetUniqueId(item, EmptyId);
            return true;
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
        /// Destroys and clears our <see cref="RootContainer"/>
        /// </summary>
        public void Clear() {
            ResourceFolder.ClearHierarchy(this.RootContainer, true);
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

        internal static void InternalProcessResourceSelectionChanged(BaseResource resource) {
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

        internal static void InternalProcessResourceOnAttached(BaseResource resource, ResourceManager manager) {
            if (resource.IsSelected)
                manager.selectedItems.Add(resource);
            manager.Project.MarkModified();
        }

        internal static void InternalProcessResourceOnDetatched(BaseResource resource) {
            ResourceManager manager = resource.Manager;
            if (manager == null)
                throw new InvalidOperationException("Expected resource item to have a manager associated with it when it was being detatched...");
            if (resource.IsSelected)
                manager.selectedItems.Remove(resource);
            manager.Project.MarkModified();
        }

        internal static void InternalOnResourceItemAttachedToManager(ResourceItem item) {
            ResourceManager manager = item.Manager;
            if (manager == null)
                throw new InvalidOperationException("Expected resource item to have a manager associated with it when it was being attached...");
            manager.RegisterEntryInternal(item.UniqueId == EmptyId ? manager.GetNextId() : item.UniqueId, item);
        }

        internal static void InternalOnResourceItemDetatchedFromManager(ResourceItem item) {
            item.Manager.UnregisterItem(item);
        }

        public void ClearSelection() {
            foreach (BaseResource selected in this.selectedItems.ToList()) {
                Debug.Assert(selected.IsSelected, "Expected item in selectedItems set to be selected!!!");
                selected.IsSelected = false;
            }
        }

        public void Destroy() {
        }
    }
}