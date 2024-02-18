using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using FramePFX.AdvancedContextService.WPF;
using FramePFX.Editors.Contextual;
using FramePFX.Editors.Controls.Binders;
using FramePFX.Editors.Controls.Resources.Explorers;
using FramePFX.Editors.Controls.TreeViews.Controls;
using FramePFX.Editors.ResourceManaging;
using FramePFX.Editors.ResourceManaging.Events;
using FramePFX.Interactivity;
using FramePFX.Interactivity.DataContexts;
using FramePFX.Logger;
using FramePFX.Utils;

namespace FramePFX.Editors.Controls.Resources.Trees {
    public class ResourceTreeViewItem : MultiSelectTreeViewItem, IResourceTreeControl {
        public static readonly DependencyProperty IsDroppableTargetOverProperty = DependencyProperty.Register("IsDroppableTargetOver", typeof(bool), typeof(ResourceTreeViewItem), new PropertyMetadata(BoolBox.False));

        public bool IsDroppableTargetOver {
            get => (bool) this.GetValue(IsDroppableTargetOverProperty);
            set => this.SetValue(IsDroppableTargetOverProperty, value.Box());
        }

        /// <summary>
        /// The resource tree that this node is placed in
        /// </summary>
        public ResourceTreeView ResourceTree { get; private set; }

        /// <summary>
        /// The parent node. This may be null, in which case, we are a root item and therefore should use <see cref="ResourceTree"/> instead
        /// </summary>
        public ResourceTreeViewItem ParentNode { get; private set; }

        /// <summary>
        /// Gets either our <see cref="ParentNode"/> or <see cref="ResourceTree"/>
        /// </summary>
        public ItemsControl ParentObject {
            get {
                if (this.ParentNode != null)
                    return this.ParentNode;
                return this.ResourceTree;
            }
        }

        /// <summary>
        /// The resource model we are attached to
        /// </summary>
        public BaseResource Resource { get; private set; }

        public MovedResource MovedResource { get; set; }

        private bool isProcessingAsyncDrop;
        private bool isDragDropping;
        private Point originMousePoint;
        private bool isDragActive;
        private bool CanExpandNextMouseUp;

        private readonly GetSetAutoEventPropertyBinder<BaseResource> displayNameBinder = new GetSetAutoEventPropertyBinder<BaseResource>(HeaderProperty, nameof(BaseResource.DisplayNameChanged), b => b.Model.DisplayName, (b, v) => b.Model.DisplayName = (string) v);
        private readonly GetSetAutoEventPropertyBinder<BaseResource> isSelectedBinder = new GetSetAutoEventPropertyBinder<BaseResource>(IsSelectedProperty, nameof(BaseResource.IsSelectedChanged), b => b.Model.IsSelected.Box(), (b, v) => b.Model.IsSelected = (bool) v);

        public ResourceTreeViewItem() {
            this.AllowDrop = true;
            AdvancedContextMenu.SetContextGenerator(this, ResourceContextRegistry.Instance);
        }

        static ResourceTreeViewItem() => DefaultStyleKeyProperty.OverrideMetadata(typeof(ResourceTreeViewItem), new FrameworkPropertyMetadata(typeof(ResourceTreeViewItem)));

        public void OnAdding(ResourceTreeView resourceTree, ResourceTreeViewItem parentNode, BaseResource resource) {
            this.ResourceTree = resourceTree;
            this.ParentNode = parentNode;
            this.Resource = resource;
            this.AllowDrop = resource is ResourceFolder;
        }

        public void OnAdded() {
            if (this.Resource is ResourceFolder folder) {
                folder.ResourceAdded += this.OnResourceAdded;
                folder.ResourceRemoved += this.OnResourceRemoved;
                folder.ResourceMoved += this.OnResourceMoved;

                int i = 0;
                foreach (BaseResource item in folder.Items) {
                    this.InsertNode(item, i++);
                }
            }

            this.displayNameBinder.Attach(this, this.Resource);
            this.isSelectedBinder.Attach(this, this.Resource);
            DataManager.SetContextData(this, new DataContext().Set(DataKeys.ResourceObjectKey, this.Resource));
        }

        public void OnRemoving() {
            if (this.Resource is ResourceFolder folder) {
                folder.ResourceAdded -= this.OnResourceAdded;
                folder.ResourceRemoved -= this.OnResourceRemoved;
                folder.ResourceMoved -= this.OnResourceMoved;
            }

            int count = this.Items.Count;
            for (int i = count - 1; i >= 0; i--) {
                this.RemoveNode(i);
            }

            this.displayNameBinder.Detatch();
            this.isSelectedBinder.Detatch();
            DataManager.ClearContextData(this);
        }

        public void OnRemoved() {
            this.ResourceTree = null;
            this.ParentNode = null;
            this.Resource = null;
        }

        private void OnResourceAdded(ResourceFolder parent, BaseResource item, int index) => this.InsertNode(item, index);

        private void OnResourceRemoved(ResourceFolder parent, BaseResource item, int index) => this.RemoveNode(index);

        private void OnResourceMoved(ResourceFolder sender, ResourceMovedEventArgs e) => HandleMoveEvent(this, e);

        public static void HandleMoveEvent(IResourceTreeControl self, ResourceMovedEventArgs e) {
            if (e.OldFolder == self.Resource) {
                // The item in our collection is being moved
                IResourceTreeControl dstTrack = ResourceTreeView.FindNodeForResource(self, e.NewFolder);
                if (dstTrack == null) {
                    // Instead of throwing, we could just remove the track or insert a new track, instead of
                    // trying to re-use existing controls, at the cost of performance.
                    // However, moving clips between tracks in different timelines is not directly supported
                    // so there's no need to support it here
                    throw new Exception("Could not find destination tree node. Is the UI corrupted?");
                }

                ResourceTreeViewItem control = self.GetNodeAt(e.OldIndex);
                self.RemoveNode(e.OldIndex, false);
                dstTrack.MovedResource = new MovedResource(control, e.Item);
            }
            else if (e.NewFolder == self.Resource) {
                if (!(self.MovedResource is MovedResource moved)) {
                    throw new Exception("Clip control being moved is null. Is the UI timeline corrupted or did the clip move between timelines?");
                }

                self.InsertNode(moved.Control, moved.Resource, e.NewIndex);
                self.MovedResource = null;
            }
        }

        public ResourceTreeViewItem GetNodeAt(int index) => (ResourceTreeViewItem) this.Items[index];

        public void InsertNode(BaseResource item, int index) {
            this.InsertNode(null, item, index);
        }

        public void InsertNode(ResourceTreeViewItem control, BaseResource resource, int index) {
            ResourceTreeView tree = this.ResourceTree;
            if (tree == null)
                throw new InvalidOperationException("Cannot add children when we have no resource tree associated");
            if (control == null)
                control = tree.GetCachedItemOrNew();

            control.OnAdding(tree, this, resource);
            this.Items.Insert(index, control);
            tree.AddResourceMapping(control, resource);
            control.ApplyTemplate();
            control.OnAdded();
        }

        public void RemoveNode(int index, bool canCache = true) {
            ResourceTreeView tree = this.ResourceTree;
            if (tree == null)
                throw new InvalidOperationException("Cannot remove children when we have no resource tree associated");

            ResourceTreeViewItem control = (ResourceTreeViewItem) this.Items[index];
            BaseResource resource = control.Resource ?? throw new Exception("Invalid application state");
            control.OnRemoving();
            this.Items.RemoveAt(index);
            tree.RemoveResourceMapping(control, resource);
            control.OnRemoved();
            if (canCache)
                tree.PushCachedItem(control);
        }

        public static bool CanBeginDragDrop() {
            return !KeyboardUtils.AreAnyModifiersPressed(ModifierKeys.Control, ModifierKeys.Shift);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left) {
                ResourceTreeView tree = this.ResourceTree;
                if (tree.SelectedItems.Count < 1 || !this.IsSelected && !KeyboardUtils.AreAnyModifiersPressed(ModifierKeys.Control)) {
                    tree.ClearSelection();
                    this.IsSelected = true;
                }

                if (e.ClickCount > 1) {
                    // this.CanExpandNextMouseUp = true;
                    e.Handled = true;
                }
                else {
                    if (Keyboard.Modifiers == ModifierKeys.None && this.Resource is ResourceFolder folder && this.ResourceTree?.ResourceManager is ResourceManager manager) {
                        manager.CurrentFolder = folder;
                    }

                    if (CanBeginDragDrop() && !e.Handled) {
                        if ((this.IsFocused || this.Focus()) && !this.isDragDropping) {
                            this.CaptureMouse();
                            this.originMousePoint = e.GetPosition(this);
                            this.isDragActive = true;
                            e.Handled = true;
                            return;
                        }
                    }
                }
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e) {
            if (this.isDragActive && (e.ChangedButton == MouseButton.Left || e.ChangedButton == MouseButton.Right)) {
                this.isDragActive = false;
                if (this.IsMouseCaptured) {
                    this.ReleaseMouseCapture();
                }

                ResourceTreeView parent = this.ResourceTree;
                e.Handled = true;
                if (!this.IsSelected) {
                    if (e.ChangedButton == MouseButton.Left) {
                        parent?.Selection.Select(this);
                    }
                    else if (!this.IsSelected) {
                        parent?.Selection.Select(this);
                    }
                }
                else if (parent != null && parent.SelectedItems.Count > 1) {
                    parent.ClearSelection();
                    this.IsSelected = true;
                }
            }

            if (this.CanExpandNextMouseUp) {
                this.CanExpandNextMouseUp = false;
                this.IsExpanded = !this.IsExpanded;
            }

            base.OnMouseUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);
            if (e.LeftButton != MouseButtonState.Pressed) {
                if (ReferenceEquals(e.MouseDevice.Captured, this)) {
                    this.ReleaseMouseCapture();
                }

                this.isDragActive = false;
                this.originMousePoint = new Point(0, 0);
                return;
            }

            if (!this.isDragActive || this.isDragDropping) {
                return;
            }

            Point posA = e.GetPosition(this);
            Point posB = this.originMousePoint;
            Point change = new Point(Math.Abs(posA.X - posB.X), Math.Abs(posA.X - posB.X));
            if (change.X > 5 || change.Y > 5) {
                if (!(this.Resource is BaseResource resource) || resource.Manager == null) {
                    return;
                }

                IReadOnlyCollection<BaseResource> selection = resource.Manager.SelectedItems;
                if (selection.Count < 1 || !selection.Contains(resource)) {
                    this.IsSelected = true;
                }

                List<BaseResource> list = selection.ToList();

                try {
                    this.isDragDropping = true;
                    DragDrop.DoDragDrop(this, new DataObject(ResourceDropRegistry.ResourceDropType, list), DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link);
                }
                catch (Exception ex) {
                    AppLogger.Instance.WriteLine("Exception while executing resource tree item drag drop: " + ex.GetToString());
                }
                finally {
                    this.isDragDropping = false;
                }
            }
        }

        protected override void OnDragEnter(DragEventArgs e) => this.OnDragOver(e);

        protected override void OnDragOver(DragEventArgs e) {
            if (this.Resource is ResourceFolder self) {
                this.IsDroppableTargetOver = ResourceExplorerListItem.ProcessCanDragOver(self, e);
            }
        }

        protected override async void OnDrop(DragEventArgs e) {
            e.Handled = true;
            if (this.isProcessingAsyncDrop || !(this.Resource is ResourceFolder self)) {
                return;
            }

            try {
                this.isProcessingAsyncDrop = true;
                if (ResourceExplorerListItem.GetDropResourceListForEvent(e, out List<BaseResource> list, out EnumDropType effects)) {
                    await ResourceDropRegistry.DropRegistry.OnDropped(self, list, effects);
                }
                else if (!await ResourceDropRegistry.DropRegistry.OnDroppedNative(self, new DataObjectWrapper(e.Data), effects)) {
                    IoC.MessageService.ShowMessage("Unknown Data", "Unknown dropped item. Drop files here");
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
    }
}