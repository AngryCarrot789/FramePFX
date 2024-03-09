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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace FramePFX.FileBrowser.FileTree
{
    public delegate void TreeEntryEventHandler(VFSFileEntry entry);

    public delegate void TreeEntryChildIndexEventHandler(VFSFileEntry entry, VFSFileEntry item, int index);

    public delegate void TreeEntryParentChangedEventHandler(VFSFileEntry entry, VFSFileEntry oldParent, VFSFileEntry newParent);

    /// <summary>
    /// The base class for an entry in a tree system
    /// </summary>
    public class VFSFileEntry
    {
        private readonly ObservableCollection<VFSFileEntry> items;
        private bool isContentLoaded;
        private bool isContentLoading; // async loading state
        private string fileName;

        /// <summary>
        /// The parent file
        /// </summary>
        public VFSFileEntry Parent { get; private set; }

        /// <summary>
        /// This tree items' child items
        /// </summary>
        public ReadOnlyObservableCollection<VFSFileEntry> Items { get; }

        /// <summary>
        /// Whether this item is empty, as in, has no children. This will not throw even if <see cref="IsDirectory"/> is false
        /// </summary>
        public bool IsEmpty => this.items.Count < 1;

        /// <summary>
        /// The number of children in this item
        /// </summary>
        public int ItemCount => this.items.Count;

        /// <summary>
        /// Whether or not this tree item can hold child tree items (stored in <see cref="Items"/>).
        /// Attempting to add files when this is false will cause an exception to be thrown.
        /// <para>
        /// This value remains constant for the lifetime of the object.
        /// </para>
        /// </summary>
        public bool IsDirectory { get; }

        /// <summary>
        /// The file system associated with this entry. Will be null for the absolute tree root (which just acts as a container for entries)
        /// </summary>
        public TreeFileSystem FileSystem { get; set; }

        /// <summary>
        /// Whether or not this entry is the root container object for a file explorer. This simply just
        /// checks if the parent is null, which will be true for a root entry object
        /// </summary>
        public bool IsRootContainer => this.Parent == null; // FileSystem == null == true

        /// <summary>
        /// Whether or not this entry is a child of the root container. If this returns true,
        /// then <see cref="Parent"/> is a root container and will have no parent.
        /// This returns false if <see cref="Parent"/> is null
        /// </summary>
        public bool IsTopLevelFile => this.Parent?.IsRootContainer ?? false;

        public string FileName {
            get => this.fileName;
            protected set
            {
                if (this.fileName == value)
                    return;
                this.fileName = value;
                this.FileNameChanged?.Invoke(this);
            }
        }

        public event TreeEntryParentChangedEventHandler ParentChanged;
        public event TreeEntryChildIndexEventHandler ItemAdded;
        public event TreeEntryChildIndexEventHandler ItemRemoved;
        public event TreeEntryEventHandler FileNameChanged;

        public VFSFileEntry(bool isDirectory)
        {
            this.IsDirectory = isDirectory;
            this.items = new ObservableCollection<VFSFileEntry>();
            this.Items = new ReadOnlyObservableCollection<VFSFileEntry>(this.items);
        }

        public bool LoadContent()
        {
            if (this.isContentLoaded || this.isContentLoading)
            {
                return true;
            }

            try
            {
                this.isContentLoading = true;
                if (this.IsDirectory && this.FileSystem != null)
                {
                    return this.FileSystem.LoadContent(this);
                }
            }
            finally
            {
                this.isContentLoaded = true;
                this.isContentLoading = false;
            }

            return false;
        }

        /// <summary>
        /// Refreshes this item, causing any data to be reloaded
        /// </summary>
        public void RefreshContent()
        {
            if (this.IsDirectory && this.FileSystem != null)
            {
                this.FileSystem.RefreshContent(this);
            }
        }

        /// <summary>
        /// Called when an entry is added to this entry. This is called after the entry's <see cref="OnAddedToParent"/> function
        /// </summary>
        /// <param name="index"></param>
        /// <param name="entry"></param>
        protected virtual void OnItemAdded(int index, VFSFileEntry entry)
        {
        }

        protected virtual void OnItemRemoved(int index, VFSFileEntry entry)
        {
        }

        /// <summary>
        /// Called when this item has been added to a new parent item. <see cref="Parent"/> will not be null.
        /// This is called after we are added to the parent's internal collection
        /// </summary>
        protected virtual void OnAddedToParent()
        {
            this.ParentChanged?.Invoke(this, null, this.Parent);
        }

        /// <summary>
        /// Called before an item is about to be removed from its parent. <see cref="Parent"/> will remain the same as
        /// it was before this call. Is this called before we are removed from our parent's internal collection
        /// </summary>
        protected virtual void OnRemovingFromParent()
        {
            if (this.IsDirectory)
            {
                this.ClearItemsRecursiveInternal();
            }
        }

        /// <summary>
        /// Called after an item was fully removed from its parent. Is this called after we
        /// are removed from our parent's internal collection
        /// </summary>
        /// <param name="parent">The previous parent</param>
        protected virtual void OnRemovedFromParent(VFSFileEntry parent)
        {
            this.ParentChanged?.Invoke(this, parent, null);
        }

        public virtual void AddItemCore(VFSFileEntry item)
        {
            this.InsertItemCore(this.items.Count, item);
        }

        public void AddItemsCore(IEnumerable<VFSFileEntry> enumerable)
        {
            this.ValidateIsDirectory();
            int i = this.items.Count;
            foreach (VFSFileEntry file in enumerable)
            {
                this.InsertItemInternal(i++, file);
            }
        }

        public void InsertItemCore(int index, VFSFileEntry item)
        {
            this.ValidateIsDirectory();
            this.InsertItemInternal(index, item);
        }

        private void InsertItemInternal(int index, VFSFileEntry item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (item.Parent != null)
                throw new Exception("Item already exists in another parent item");
            if (item.ItemCount > 0 && this.IsPartOfParentHierarchy(item))
                throw new Exception("Cannot add an item which is already in our parent hierarchy chain");

            item.Parent = this;
            this.items.Insert(index, item);
            item.OnAddedToParent();
            this.ItemAdded?.Invoke(this, item, index);
            this.OnItemAdded(index, item);
        }

        public bool RemoveItemCore(VFSFileEntry item)
        {
            this.ValidateIsDirectory();
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            int index = this.items.IndexOf(item);
            if (index == -1)
                return false;
            this.RemoveItemAtInternal(index);
            return true;
        }

        public void RemoveItemAtCore(int index)
        {
            this.ValidateIsDirectory();
            this.RemoveItemAtInternal(index);
        }

        private void RemoveItemAtInternal(int index)
        {
            VFSFileEntry item = this.items[index];
            if (item.Parent != this)
                throw new Exception("Expected item's parent to equal the current instance");

            item.OnRemovingFromParent();
            item.Parent = null;
            this.items.RemoveAt(index);
            item.OnRemovedFromParent(this);
            this.ItemRemoved?.Invoke(this, item, index);
            this.OnItemRemoved(index, item);
        }

        public void ClearItemsRecursiveCore()
        {
            this.ValidateIsDirectory();
            this.ClearItemsRecursiveInternal();
        }

        private void ClearItemsRecursiveInternal()
        {
            for (int i = this.items.Count - 1; i >= 0; i--)
            {
                VFSFileEntry item = this.items[i];
                if (item.IsDirectory)
                {
                    item.ClearItemsRecursiveInternal();
                }

                this.RemoveItemAtInternal(i);
            }
        }

        private void ValidateIsDirectory()
        {
            if (!this.IsDirectory)
            {
                throw new InvalidOperationException("This item is not a directory");
            }
        }

        private bool IsPartOfParentHierarchy(VFSFileEntry item)
        {
            for (VFSFileEntry par = this; par != null; par = par.Parent)
            {
                if (par == item)
                    return true;
            }

            return false;
        }
    }
}