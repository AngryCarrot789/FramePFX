using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace FramePFX.FileBrowser.FileTree {
    /// <summary>
    /// The base class for an entry in a tree system. Supports both inheritance and composition data (via the Set/Get/TryGet data functions).
    /// <para>
    /// This class is meant to be a view model acting as both a model and view model
    /// </para>
    /// <para>
    /// Due of how WPF binding works, you're effectively forced to use sub-types of this class (e.g. physical files, zip files, etc.)
    /// in order to implement bindable properties. Otherwise you could simply just set specific data keys via the Get/Set/TryGet data
    /// functions in order to determine what type of file an instance of <see cref="TreeEntry"/> is
    /// </para>
    /// </summary>
    public class TreeEntry : BaseViewModel {
        private readonly Dictionary<string, object> dataKeys;
        private readonly ObservableCollection<TreeEntry> items;
        private bool isExpanded;
        private bool isExpanding; // async loading state
        public bool IsContentLoaded; // for lazily loading

        /// <summary>
        /// The parent file
        /// </summary>
        public TreeEntry Parent { get; private set; }

        /// <summary>
        /// This tree items' child items
        /// </summary>
        public ReadOnlyObservableCollection<TreeEntry> Items { get; }

        /// <summary>
        /// Whether this item is empty, as in, has no children. This will not throw even if <see cref="IsDirectory"/> is false
        /// </summary>
        public bool IsEmpty => this.items.Count < 1;

        /// <summary>
        /// The number of children in this item
        /// </summary>
        public int ItemCount => this.items.Count;

        /// <summary>
        /// Whether or not this item has been expanded at least once by the user
        /// </summary>
        public bool HasExpandedOnce { get; private set; }

        /// <summary>
        /// Whether or not this item is currently expanded and therefore the contents are loaded
        /// </summary>
        public bool IsExpanded {
            get => this.isExpanded;
            set {
                if (this.isExpanded == value) {
                    return;
                }

                if (this.isExpanding) {
                    this.RaisePropertyChanged(ref this.isExpanded, false);
                    return;
                }

                if (value) {
                    this.OnExpandInternal();
                }
                else {
                    this.RaisePropertyChanged(ref this.isExpanded, false);
                }
            }
        }

        /// <summary>
        /// Whether or not this file is currently being "expanded" (as in, browsed to in a tree)
        /// </summary>
        public bool IsExpanding {
            get => this.isExpanding;
            private set => this.RaisePropertyChanged(ref this.isExpanding, value);
        }

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
        /// The explorer associated with this tree entry. This is used to process things such as navigation, deletion, etc.
        /// </summary>
        public FileTreeViewModel FileTree { get; private set; }

        public FileExplorerViewModel FileExplorer { get; private set; }

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

        public TreeEntry(bool isDirectory) {
            this.IsDirectory = isDirectory;
            this.dataKeys = new Dictionary<string, object>();
            this.items = new ObservableCollection<TreeEntry>();
            this.Items = new ReadOnlyObservableCollection<TreeEntry>(this.items);
            this.items.CollectionChanged += this.OnChildrenCollectionChanged;
        }

        public virtual void SetFileTree(FileTreeViewModel tree) {
            this.FileTree = tree;
            this.RaisePropertyChanged(nameof(this.FileTree));
            if (this.IsDirectory && this.items.Count > 0) {
                foreach (TreeEntry item in this.items) {
                    item.SetFileTree(tree);
                }
            }
        }

        public virtual void SetFileExplorer(FileExplorerViewModel explorer) {
            this.FileExplorer = explorer;
            this.RaisePropertyChanged(nameof(this.FileTree));
            if (this.IsDirectory && this.items.Count > 0) {
                foreach (TreeEntry item in this.items) {
                    item.SetFileExplorer(explorer);
                }
            }
        }

        private async void OnExpandInternal() {
            this.IsExpanding = true;
            try {
                this.isExpanded = await this.OnExpandAsync();
            }
            finally {
                this.isExpanding = false;
            }

            this.RaisePropertyChanged(nameof(this.IsExpanding));
            if (this.HasExpandedOnce) {
                this.RaisePropertyChanged(nameof(this.IsExpanded));
            }
            else {
                this.HasExpandedOnce = true;
                this.RaisePropertyChanged(nameof(this.IsExpanded));
                this.RaisePropertyChanged(nameof(this.HasExpandedOnce));
            }
        }

        /// <summary>
        /// Called when this tree entry is expanded. By default, this accesses the file system and loads the child content
        /// </summary>
        /// <returns>A task to await for the expand to complete (mainly loading content, if possible)</returns>
        protected virtual async Task<bool> OnExpandAsync() {
            if (this.IsDirectory && this.FileSystem != null) {
                if (!this.IsContentLoaded) {
                    this.IsContentLoaded = true;
                    await this.FileSystem.LoadContent(this);
                }

                return this.items.Count > 0;
            }

            return false;
        }

        /// <summary>
        /// Refreshes this item, causing any data to be reloaded
        /// </summary>
        public virtual async Task RefreshAsync() {
            if (this.IsDirectory && this.FileSystem != null) {
                if (this.IsContentLoaded) {
                    await this.FileSystem.RefreshContent(this);
                }
            }
        }

        /// <summary>
        /// Called when the internal child collection list is modified. The base method (in <see cref="TreeEntry"/>) must be called.
        /// <para>
        /// This is called before the virtual change handlers (e.g. <see cref="OnItemAdded"/> and <see cref="OnItemRemoved"/>)
        /// </para>
        /// </summary>
        /// <param name="sender">The collection object</param>
        /// <param name="e">The event args</param>
        protected virtual void OnChildrenCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            this.RaisePropertyChanged(nameof(this.ItemCount));
            this.RaisePropertyChanged(nameof(this.IsEmpty));
        }

        /// <summary>
        /// Called when an entry is added to this entry. This is called after the entry's <see cref="OnAddedToParent"/> function
        /// </summary>
        /// <param name="index"></param>
        /// <param name="entry"></param>
        protected virtual void OnItemAdded(int index, TreeEntry entry) {
        }

        protected virtual void OnItemRemoved(int index, TreeEntry entry) {
        }

        /// <summary>
        /// Called when this item has been added to a new parent item. <see cref="Parent"/> will not be null.
        /// This is called after we are added to the parent's internal collection
        /// </summary>
        protected virtual void OnAddedToParent() {
            this.OnParentChanged(this.Parent);
            this.SetFileTree(this.Parent.FileTree);
            this.SetFileExplorer(this.Parent.FileExplorer);
        }

        /// <summary>
        /// Called before an item is about to be removed from its parent. <see cref="Parent"/> will remain the same as
        /// it was before this call. Is this called before we are removed from our parent's internal collection
        /// </summary>
        protected virtual void OnRemovingFromParent() {
            this.IsExpanded = false;
            if (this.IsDirectory) {
                this.ClearItemsRecursiveInternal();
            }
        }

        /// <summary>
        /// Called after an item was fully removed from its parent. Is this called after we
        /// are removed from our parent's internal collection
        /// </summary>
        /// <param name="parent">The previous parent</param>
        protected virtual void OnRemovedFromParent(TreeEntry parent) {
            this.OnParentChanged(parent);
            this.SetFileTree(null);
            this.SetFileExplorer(null);
        }

        /// <summary>
        /// Called by <see cref="OnAddedToParent"/> and <see cref="OnRemovedFromParent"/>. This
        /// is just used to fire property changed events
        /// </summary>
        protected virtual void OnParentChanged(TreeEntry parent) {
            this.RaisePropertyChanged(nameof(this.Parent));
            this.RaisePropertyChanged(nameof(this.IsRootContainer));
            this.RaisePropertyChanged(nameof(this.IsTopLevelFile));
        }

        public virtual void AddItemCore(TreeEntry item) {
            this.InsertItemCore(this.items.Count, item);
        }

        public void AddItemsCore(IEnumerable<TreeEntry> enumerable) {
            this.ValidateIsDirectory();
            int i = this.items.Count;
            foreach (TreeEntry file in enumerable) {
                this.InsertItemInternal(i++, file);
            }
        }

        public void InsertItemCore(int index, TreeEntry item) {
            this.ValidateIsDirectory();
            this.InsertItemInternal(index, item);
        }

        private void InsertItemInternal(int index, TreeEntry item) {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (item.Parent != null)
                throw new Exception("Item already exists in another parent item");
            if (item.ItemCount > 0 && this.IsPartOfParentHierarchy(item))
                throw new Exception("Cannot add an item which is already in our parent hierarchy chain");

            item.Parent = this;
            this.items.Insert(index, item);
            item.OnAddedToParent();
            this.OnItemAdded(index, item);
        }

        public bool RemoveItemCore(TreeEntry item) {
            this.ValidateIsDirectory();
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            int index = this.items.IndexOf(item);
            if (index == -1)
                return false;
            this.RemoveItemAtInternal(index);
            return true;
        }

        public void RemoveItemAtCore(int index) {
            this.ValidateIsDirectory();
            this.RemoveItemAtInternal(index);
        }

        private void RemoveItemAtInternal(int index) {
            TreeEntry item = this.items[index];
            if (item.Parent != this)
                throw new Exception("Expected item's parent to equal the current instance");

            item.OnRemovingFromParent();
            item.Parent = null;
            this.items.RemoveAt(index);
            item.OnRemovedFromParent(this);
            this.OnItemRemoved(index, item);
        }

        public void ClearItemsRecursiveCore() {
            this.ValidateIsDirectory();
            this.ClearItemsRecursiveInternal();
        }

        #region Composite Data

        public T GetDataValue<T>(string key) {
            return this.TryGetDataValue(key, out T value) ? value : throw new Exception("No such data with the key: " + key);
        }

        public T GetDataValue<T>(string key, T def) {
            return this.TryGetDataValue(key, out T value) ? value : def;
        }

        public bool TryGetDataValue<T>(string key, out T value) {
            if (this.dataKeys.TryGetValue(key, out object o)) {
                value = (T) o;
                return true;
            }

            value = default;
            return false;
        }

        public bool ContainsKey(string key) {
            return this.dataKeys.ContainsKey(key);
        }

        public void SetData(string key, object value) {
            this.OnDataChanging(key, value);
            if (value == null) {
                this.dataKeys.Remove(key);
            }
            else {
                this.dataKeys[key] = value;
            }

            this.OnDataChanged(key, value);
        }

        /// <summary>
        /// Called when <see cref="SetData"/> is invoked but before any data is modified
        /// </summary>
        /// <param name="key">The data key</param>
        /// <param name="value">The new data value</param>
        /// <returns>True to allow the data to change, false to stop the data changing</returns>
        protected virtual void OnDataChanging(string key, object value) {
        }

        /// <summary>
        /// Called after the underlying data map is modified via a call to <see cref="SetData"/>. This can
        /// be used to raise property changed notifications
        /// </summary>
        /// <param name="key">The data key</param>
        /// <param name="value">The new data value</param>
        protected virtual void OnDataChanged(string key, object value) {
        }

        #endregion

        private void ClearItemsRecursiveInternal() {
            for (int i = this.items.Count - 1; i >= 0; i--) {
                TreeEntry item = this.items[i];
                if (item.IsDirectory) {
                    item.ClearItemsRecursiveInternal();
                }

                this.RemoveItemAtInternal(i);
            }
        }

        private void ValidateIsDirectory() {
            if (!this.IsDirectory) {
                throw new InvalidOperationException("This item is not a directory");
            }
        }

        private bool IsPartOfParentHierarchy(TreeEntry item) {
            for (TreeEntry par = this; par != null; par = par.Parent) {
                if (par == item)
                    return true;
            }

            return false;
        }
    }
}