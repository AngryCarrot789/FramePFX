using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using FramePFX.AdvancedContextService.WPF;
using FramePFX.Editors.Contextual;
using FramePFX.Editors.ResourceManaging;
using FramePFX.Editors.ResourceManaging.Events;
using FramePFX.Interactivity;

namespace FramePFX.Editors.Controls.Resources.Explorers {
    /// <summary>
    /// The actual list of resource explorer items, which also manages the selection of items
    /// </summary>
    public class ResourceExplorerListControl : MultiSelector {
        public static readonly DependencyProperty ResourceManagerProperty = DependencyProperty.Register("ResourceManager", typeof(ResourceManager), typeof(ResourceExplorerListControl), new PropertyMetadata(null, (d, e) => ((ResourceExplorerListControl) d).OnResourceManagerChanged((ResourceManager) e.OldValue, (ResourceManager) e.NewValue)));
        public static readonly DependencyProperty CurrentFolderProperty = DependencyProperty.Register("CurrentFolder", typeof(ResourceFolder), typeof(ResourceExplorerListControl), new PropertyMetadata(null, (d, e) => ((ResourceExplorerListControl) d).OnCurrentFolderChanged((ResourceFolder) e.OldValue, (ResourceFolder) e.NewValue)));

        public ResourceManager ResourceManager {
            get => (ResourceManager) this.GetValue(ResourceManagerProperty);
            set => this.SetValue(ResourceManagerProperty, value);
        }

        /// <summary>
        /// Gets or sets the folder that this control is currently displaying the contents of.
        /// This may affect the <see cref="ItemsControl.Items"/> collection
        /// </summary>
        public ResourceFolder CurrentFolder {
            get => (ResourceFolder) this.GetValue(CurrentFolderProperty);
            set => this.SetValue(CurrentFolderProperty, value);
        }

        private const int MaxItemCacheSize = 64;
        private const int MaxItemContentCacheSize = 16;
        private readonly Stack<ResourceExplorerListItem> itemCache;
        private readonly Dictionary<Type, Stack<ResourceExplorerListItemContent>> itemContentCacheMap;
        private bool isProcessingAsyncDrop;
        private bool isProcessingManagerCurrentFolderChanged;

        public ResourceExplorerListItem lastSelectedItem;

        public ResourceExplorerListControl() {
            this.itemCache = new Stack<ResourceExplorerListItem>(MaxItemCacheSize);
            this.itemContentCacheMap = new Dictionary<Type, Stack<ResourceExplorerListItemContent>>();
            this.AllowDrop = true;
            this.CanSelectMultipleItems = true;
            AdvancedContextMenu.SetContextGenerator(this, ResourceContextRegistry.Instance);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            base.OnMouseLeftButtonDown(e);
            if (this.Items.Cast<ResourceExplorerListItem>().Any(x => x.ResourceExplorerList == this && x.IsMouseOver)) {
                return;
            }

            this.UnselectAll();
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e) {
            base.OnPreviewMouseDown(e);
            if (e.ChangedButton == MouseButton.XButton1) {
                // Go backwards in history
                this.CurrentFolder = this.CurrentFolder?.Parent ?? this.ResourceManager?.RootContainer;
            }
            // else if (e.ChangedButton == MouseButton.XButton2) {
            //     this.ResourceManager?.GoForward();
            // }
        }
        
        public void MakeRangedSelection(ResourceExplorerListItem a, ResourceExplorerListItem b) {
            if (a == b) {
                this.MakePrimarySelection(a);
            }
            else {
                int indexA = this.ItemContainerGenerator.IndexFromContainer(a);
                if (indexA == -1) {
                    return;
                }

                int indexB = this.ItemContainerGenerator.IndexFromContainer(b);
                if (indexB == -1) {
                    return;
                }

                if (indexA < indexB) {
                    this.UnselectAll();
                    for (int i = indexA; i <= indexB; i++) {
                        this.SetItemSelectedPropertyAtIndex(i, true);
                    }
                }
                else if (indexA > indexB) {
                    this.UnselectAll();
                    for (int i = indexB; i <= indexA; i++) {
                        this.SetItemSelectedPropertyAtIndex(i, true);
                    }
                }
                else {
                    this.SetItemSelectedPropertyAtIndex(indexA, true);
                }
            }
        }

        public void MakePrimarySelection(ResourceExplorerListItem item) {
            this.ResourceManager?.ClearSelection();
            this.SetItemSelectedProperty(item, true);
            this.lastSelectedItem = item;
        }

        public void SetItemSelectedProperty(ResourceExplorerListItem item, bool selected) {
            item.IsSelected = selected;
            object x = this.ItemContainerGenerator.ItemFromContainer(item);
            if (x == null || x == DependencyProperty.UnsetValue)
                x = item;

            if (selected) {
                this.SelectedItems.Add(x);
            }
            else {
                this.SelectedItems.Remove(x);
            }
        }

        public bool SetItemSelectedPropertyAtIndex(int index, bool selected) {
            if (index < 0 || index >= this.Items.Count) {
                return false;
            }

            if (this.ItemContainerGenerator.ContainerFromIndex(index) is ResourceExplorerListItem resource) {
                this.SetItemSelectedProperty(resource, selected);
                return true;
            }
            else {
                return false;
            }
        }

        protected override void OnDragEnter(DragEventArgs e) => this.OnDragOver(e);

        protected override void OnDragOver(DragEventArgs e) {
            ResourceFolder currentFolder;
            if (this.isProcessingAsyncDrop || (currentFolder = this.CurrentFolder) == null) {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            ResourceExplorerListItem.ProcessCanDragOver(currentFolder, e);
        }

        protected override async void OnDrop(DragEventArgs e) {
            e.Handled = true;
            if (this.isProcessingAsyncDrop || !(this.CurrentFolder is ResourceFolder currentFolder)) {
                return;
            }

            try {
                this.isProcessingAsyncDrop = true;
                if (ResourceExplorerListItem.GetDropResourceListForEvent(e, out List<BaseResource> list, out EnumDropType effects)) {
                    await ResourceDropRegistry.DropRegistry.OnDropped(currentFolder, list, effects);
                }
                else if (!await ResourceDropRegistry.DropRegistry.OnDroppedNative(currentFolder, new DataObjectWrapper(e.Data), effects)) {
                    IoC.MessageService.ShowMessage("Unknown data", "Unknown dropped item. Drop files here");
                    // await IoC.DialogService.ShowMessageAsync("Unknown data", "Unknown dropped item. Drop files here");
                }
            }
            finally {
                this.isProcessingAsyncDrop = false;
            }
        }

        private void OnCurrentFolderChanged(ResourceFolder oldFolder, ResourceFolder newFolder) {
            if (oldFolder != null) {
                oldFolder.ResourceAdded -= this.CurrentFolder_OnResourceAdded;
                oldFolder.ResourceRemoved -= this.CurrentFolder_OnResourceRemoved;
                oldFolder.ResourceMoved -= this.CurrentFolder_OnResourceMoved;
                for (int i = this.Items.Count - 1; i >= 0; i--) {
                    this.RemoveResourceInternal(i);
                }
            }

            if (newFolder != null) {
                newFolder.ResourceAdded += this.CurrentFolder_OnResourceAdded;
                newFolder.ResourceRemoved += this.CurrentFolder_OnResourceRemoved;
                newFolder.ResourceMoved += this.CurrentFolder_OnResourceMoved;
                int i = 0;
                foreach (BaseResource resource in newFolder.Items) {
                    this.InsertResourceInternal(resource, i++);
                }
            }

            ResourceManager manager;
            if (!this.isProcessingManagerCurrentFolderChanged && (manager = this.ResourceManager) != null) {
                manager.CurrentFolder = newFolder;
            }
        }

        private void CurrentFolder_OnResourceAdded(ResourceFolder parent, BaseResource item, int index) {
            this.InsertResourceInternal(item, index);
        }

        private void CurrentFolder_OnResourceRemoved(ResourceFolder parent, BaseResource item, int index) {
            this.RemoveResourceInternal(index);
        }

        private void CurrentFolder_OnResourceMoved(ResourceFolder sender, ResourceMovedEventArgs e) {
            if (e.IsSameFolder) { // Item was moved within the current folder itself
                this.MoveResourceInternal(e.OldIndex, e.NewIndex);
            }
            else if (e.NewFolder == sender) { // It was effectively added
                this.InsertResourceInternal(e.Item, e.NewIndex);
            }
            else { // It was effectively removed
                this.RemoveResourceInternal(e.OldIndex);
            }
        }

        private void OnResourceManagerChanged(ResourceManager oldManager, ResourceManager newManager) {
            if (oldManager != null) {
                oldManager.CurrentFolderChanged -= this.OnManagerCurrentFolderChanged;
            }

            if (newManager != null) {
                newManager.CurrentFolderChanged += this.OnManagerCurrentFolderChanged;
            }

            this.CurrentFolder = newManager?.CurrentFolder;
        }

        private void OnManagerCurrentFolderChanged(ResourceManager manager, ResourceFolder oldFolder, ResourceFolder newFolder) {
            try {
                this.isProcessingManagerCurrentFolderChanged = true;
                this.CurrentFolder = newFolder;
            }
            finally {
                this.isProcessingManagerCurrentFolderChanged = false;
            }
        }

        private void InsertResourceInternal(BaseResource resource, int index) {
            ResourceExplorerListItem control = this.itemCache.Count > 0 ? this.itemCache.Pop() : new ResourceExplorerListItem();
            control.OnAddingToList(this, resource);
            this.Items.Insert(index, control);
            control.OnAddedToList();
            control.InvalidateMeasure();
            this.InvalidateMeasure();
        }

        private void RemoveResourceInternal(int index) {
            ResourceExplorerListItem control = (ResourceExplorerListItem) this.Items[index];
            control.OnRemovingFromList();
            this.Items.RemoveAt(index);
            control.OnRemovedFromList();
            if (this.itemCache.Count < MaxItemCacheSize)
                this.itemCache.Push(control);
            this.InvalidateMeasure();
        }

        private void MoveResourceInternal(int oldIndex, int newIndex) {
            ResourceExplorerListItem control = (ResourceExplorerListItem) this.Items[oldIndex];
            // control.OnIndexMoving(oldIndex, newIndex);
            this.Items.RemoveAt(oldIndex);
            this.Items.Insert(newIndex, control);
            // control.OnIndexMoved(oldIndex, newIndex);
            this.InvalidateMeasure();
        }

        /// <summary>
        /// Either returns a cached content object from resource type, or creates a new instance of it.
        /// <see cref="ReleaseContentObject"/> should be called after the returned object is no longer needed,
        /// in order to help with performance (saves re-creating the object and applying styles)
        /// </summary>
        /// <param name="resourceType">The resource object type</param>
        /// <returns>A reused or new content object</returns>
        public ResourceExplorerListItemContent GetContentObject(Type resourceType) {
            ResourceExplorerListItemContent content;
            if (this.itemContentCacheMap.TryGetValue(resourceType, out Stack<ResourceExplorerListItemContent> stack) && stack.Count > 0) {
                content = stack.Pop();
            }
            else {
                content = ResourceExplorerListItemContent.NewInstance(resourceType);
            }

            return content;
        }

        /// <summary>
        /// Adds the given content object to our internal cache (keyed by the given resource type) if the cache
        /// is small enough, otherwise the object is forgotten and garbage collected (at least, that's the intent;
        /// bugs in the disconnection code may prevent that).
        /// The content object should not be used after this call, instead use <see cref="GetContentObject"/>
        /// </summary>
        /// <param name="resourceType">The resource object type</param>
        /// <param name="content">The content object type that is no longer in use</param>
        /// <returns>True when the object was cached, false when the cache is too large and could not fit the object in</returns>
        public bool ReleaseContentObject(Type resourceType, ResourceExplorerListItemContent content) {
            if (!this.itemContentCacheMap.TryGetValue(resourceType, out Stack<ResourceExplorerListItemContent> stack)) {
                this.itemContentCacheMap[resourceType] = stack = new Stack<ResourceExplorerListItemContent>();
            }
            else if (stack.Count == MaxItemContentCacheSize) {
                return false;
            }

            stack.Push(content);
            return true;
        }

        public IEnumerable<ResourceExplorerListItem> GetSelectedControls() {
            return this.SelectedItems.Cast<ResourceExplorerListItem>();
        }

        public IEnumerable<BaseResource> GetSelectedResources() {
            return this.ResourceManager?.SelectedItems ?? new List<BaseResource>();
        }
    }
}