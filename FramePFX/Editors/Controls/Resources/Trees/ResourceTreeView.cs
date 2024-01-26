using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using FramePFX.Editors.Controls.Resources.Explorers;
using FramePFX.Editors.Controls.TreeViews.Controls;
using FramePFX.Editors.ResourceManaging;
using FramePFX.Editors.ResourceManaging.Events;
using FramePFX.Interactivity;
using FramePFX.Utils;

namespace FramePFX.Editors.Controls.Resources.Trees {
    public class ResourceTreeView : MultiSelectTreeView, IResourceTreeControl {
        public static readonly DependencyProperty IsDroppableTargetOverProperty = DependencyProperty.Register("IsDroppableTargetOver", typeof(bool), typeof(ResourceTreeView), new PropertyMetadata(BoolBox.False));
        public static readonly DependencyProperty ResourceManagerProperty = DependencyProperty.Register("ResourceManager", typeof(ResourceManager), typeof(ResourceTreeView), new PropertyMetadata(null, (d, e) => ((ResourceTreeView) d).OnResourceManagerChanged((ResourceManager) e.OldValue, (ResourceManager) e.NewValue)));

        public ResourceManager ResourceManager {
            get => (ResourceManager) this.GetValue(ResourceManagerProperty);
            set => this.SetValue(ResourceManagerProperty, value);
        }

        public bool IsDroppableTargetOver {
            get => (bool) this.GetValue(IsDroppableTargetOverProperty);
            set => this.SetValue(IsDroppableTargetOverProperty, value.Box());
        }

        ResourceTreeView IResourceTreeControl.ResourceTree => this;

        ResourceTreeViewItem IResourceTreeControl.ParentNode => null;

        public MovedResource MovedResource { get; set; }

        BaseResource IResourceTreeControl.Resource => this.rootFolder;

        private readonly Dictionary<ResourceTreeViewItem, BaseResource> controlToModel;
        private readonly Dictionary<BaseResource, ResourceTreeViewItem> modelToControl;
        private readonly Stack<ResourceTreeViewItem> itemCache;
        private bool isProcessingAsyncDrop;
        private ResourceFolder rootFolder;
        private BaseResource targetDropResourceFolder; // the drop target for DragDrop
        private IResourceTreeControl targetDropNodeFolder; // the control associated with the drop resource

        public ResourceTreeView() {
            this.itemCache = new Stack<ResourceTreeViewItem>();
            this.controlToModel = new Dictionary<ResourceTreeViewItem, BaseResource>();
            this.modelToControl = new Dictionary<BaseResource, ResourceTreeViewItem>();
            this.AllowDrop = true;
        }

        static ResourceTreeView() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ResourceTreeView), new FrameworkPropertyMetadata(typeof(ResourceTreeView)));
        }

        public void AddResourceMapping(ResourceTreeViewItem control, BaseResource resource) {
            // use add so that it throws for an actual error where one or
            // more resources are associated with a control, and vice versa
            // Should probably use debug condition here
            // if (this.controlToModel.ContainsKey(control))
            //     throw new Exception("Control already exists in the map: " + control);
            // if (this.modelToControl.ContainsKey(resource))
            //     throw new Exception("Resource already exists in the map: " + resource);
            this.controlToModel.Add(control, resource);
            this.modelToControl.Add(resource, control);
        }

        public void RemoveResourceMapping(ResourceTreeViewItem control, BaseResource resource) {
            if (!this.controlToModel.Remove(control))
                throw new Exception("Control did not exist in the map: " + control);
            if (!this.modelToControl.Remove(resource))
                throw new Exception("Resource did not exist in the map: " + resource);
        }

        private void OnResourceManagerChanged(ResourceManager oldManager, ResourceManager newManager) {
            if (oldManager != null) {
                this.rootFolder = oldManager.RootContainer;
                this.rootFolder.ResourceAdded -= this.OnResourceAdded;
                this.rootFolder.ResourceRemoved -= this.OnResourceRemoved;
                this.rootFolder.ResourceMoved -= this.OnResourceMoved;
                for (int i = this.Items.Count - 1; i >= 0; i--) {
                    this.RemoveNode(i);
                }
            }

            if (newManager != null) {
                this.rootFolder = newManager.RootContainer;
                this.rootFolder.ResourceAdded += this.OnResourceAdded;
                this.rootFolder.ResourceRemoved += this.OnResourceRemoved;
                this.rootFolder.ResourceMoved += this.OnResourceMoved;
                int i = 0;
                foreach (BaseResource resource in this.rootFolder.Items) {
                    this.InsertNode(resource, i++);
                }
            }
        }

        private void OnResourceAdded(ResourceFolder parent, BaseResource item, int index) => this.InsertNode(item, index);

        private void OnResourceRemoved(ResourceFolder parent, BaseResource item, int index) => this.RemoveNode(index);

        private void OnResourceMoved(ResourceFolder sender, ResourceMovedEventArgs e) => ResourceTreeViewItem.HandleMoveEvent(this, e);

        public ResourceTreeViewItem GetNodeAt(int index) {
            return (ResourceTreeViewItem) this.Items[index];
        }

        public void InsertNode(BaseResource item, int index) {
            this.InsertNode(this.itemCache.Count > 0 ? this.itemCache.Pop() : new ResourceTreeViewItem(), item, index);
        }

        public void InsertNode(ResourceTreeViewItem control, BaseResource resource, int index) {
            control.OnAdding(this, null, resource);
            this.Items.Insert(index, control);
            this.AddResourceMapping(control, resource);
            control.ApplyTemplate();
            control.OnAdded();
        }

        public void RemoveNode(int index, bool canCache = true) {
            ResourceTreeViewItem control = (ResourceTreeViewItem) this.Items[index];
            BaseResource model = control.Resource ?? throw new Exception("Expected node to have a resource");
            control.OnRemoving();
            this.Items.RemoveAt(index);
            this.RemoveResourceMapping(control, model);
            control.OnRemoved();
            if (canCache && this.itemCache.Count < 16)
                this.itemCache.Push(control);
        }

        protected override void OnDragEnter(DragEventArgs e) => this.OnDragOver(e);

        protected override void OnDragOver(DragEventArgs e) {
            if (this.ResourceManager is ResourceManager manager) {
                this.IsDroppableTargetOver = ResourceExplorerListItem.ProcessCanDragOver(manager.RootContainer, e);
            }
        }

        protected override async void OnDrop(DragEventArgs e) {
            e.Handled = true;
            if (this.isProcessingAsyncDrop || !(this.ResourceManager is ResourceManager manager)) {
                return;
            }

            try {
                this.isProcessingAsyncDrop = true;
                if (ResourceExplorerListItem.GetDropResourceListForEvent(e, out List<BaseResource> list, out EnumDropType effects)) {
                    await ResourceDropRegistry.DropRegistry.OnDropped(manager.RootContainer, list, effects);
                }
                else if (!await ResourceDropRegistry.DropRegistry.OnDroppedNative(manager.RootContainer, new DataObjectWrapper(e.Data), effects)) {
                    MessageBox.Show("Unknown dropped item. Drop files here", "Unknown data");
                    // await IoC.DialogService.ShowMessageAsync("Unknown data", "Unknown dropped item. Drop files here");
                }
            }
            finally {
                this.IsDroppableTargetOver = false;
                this.isProcessingAsyncDrop = false;
            }
        }

        protected override void OnDragLeave(DragEventArgs e) {
            this.Dispatcher.Invoke(() => this.IsDroppableTargetOver = false, DispatcherPriority.Loaded);
        }

        protected override bool IsItemItsOwnContainerOverride(object item) {
            return item is ResourceTreeViewItem;
        }

        protected override DependencyObject GetContainerForItemOverride() {
            return new ResourceTreeViewItem();
        }

        /// <summary>
        /// Finds a resource tree item (or the tree it self) whose <see cref="IResourceTreeControl.Resource"/>
        /// matches the given resource. The current implementation uses a dictionary to map directly to and from
        /// controls and models, so this method is extremely quick in contrast to having to scan the entire
        /// tree hierarchy until you find a match, which can be extremely slow for complex trees
        /// </summary>
        /// <param name="self">The current instance, aka 'this'</param>
        /// <param name="resource">The resource to match</param>
        /// <returns>The found model</returns>
        public static IResourceTreeControl FindNodeForResource(IResourceTreeControl self, BaseResource resource) {
            ResourceTreeView root = self.ResourceTree;
            if (root != null) {
                if (root.rootFolder == resource) {
                    return root;
                }
                else if (root.targetDropResourceFolder == resource) {
                    return root.targetDropNodeFolder;
                }

                return root.modelToControl[resource];
            }

            ItemCollection list = ((ItemsControl) self).Items;
            for (int i = 0, count = list.Count; i < count; i++) {
                ResourceTreeViewItem control = (ResourceTreeViewItem) list[i];
                if (control.Resource == resource) {
                    return control;
                }
            }

            // Oh well
            return null;
        }
    }
}