using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using FramePFX.Editor.ResourceManaging.ViewModels;
using FramePFX.Interactivity;
using FramePFX.Utils;
using FramePFX.WPF.Controls.TreeViews.Controls;
using FramePFX.WPF.Editor.Timeline.Utils;

namespace FramePFX.WPF.Editor.Resources {
    internal class ResourceTreeViewItem : MultiSelectTreeViewItem {
        public static readonly DependencyProperty IsDroppableTargetOverProperty = DependencyProperty.Register("IsDroppableTargetOver", typeof(bool), typeof(ResourceTreeViewItem), new PropertyMetadata(BoolBox.False));

        public ResourceTreeView MyResourceTree {
            get {
                ItemsControl parent = ItemsControlFromItemContainer(this);
                for (; parent != null; parent = ItemsControlFromItemContainer(parent))
                    if (parent is ResourceTreeView tree)
                        return tree;
                return null;
            }
        }

        public ResourceTreeViewItem MyParentItem => ItemsControlFromItemContainer(this) as ResourceTreeViewItem;

        public bool IsDroppableTargetOver {
            get => (bool) this.GetValue(IsDroppableTargetOverProperty);
            set => this.SetValue(IsDroppableTargetOverProperty, value.Box());
        }

        public BaseResourceViewModel ViewModel => (BaseResourceViewModel) this.DataContext;

        private bool isProcessingAsyncDrop;
        private bool isDragDropping;
        private Point originMousePoint;
        private bool isDragActive;

        public ResourceTreeViewItem() {
            this.AllowDrop = true;
        }

        static ResourceTreeViewItem() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ResourceTreeViewItem), new FrameworkPropertyMetadata(typeof(ResourceTreeViewItem)));
        }

        public static bool CanBeginDragDrop() {
            return !KeyboardUtils.AreAnyModifiersPressed(ModifierKeys.Control, ModifierKeys.Shift);
        }

        private bool CanExpandNextMouseUp;

        protected override void OnMouseDown(MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left) {
                MultiSelectTreeView parent = this.ParentTreeView;
                int count = parent.SelectedItems.Count;
                if (count < 1 || !this.IsSelected) {
                    if (count < 1 || !KeyboardUtils.AreAnyModifiersPressed(ModifierKeys.Control)) {
                        this.ParentTreeView.ClearSelection();
                        this.IsSelected = true;
                    }
                }

                if (e.ClickCount > 1) {
                    this.CanExpandNextMouseUp = true;
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

            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e) {
            if (this.isDragActive && (e.ChangedButton == MouseButton.Left || e.ChangedButton == MouseButton.Right)) {
                this.isDragActive = false;
                if (this.IsMouseCaptured) {
                    this.ReleaseMouseCapture();
                }

                MultiSelectTreeView parent = this.ParentTreeView;
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
                if (!(this.DataContext is BaseResourceViewModel resource) || resource.Manager == null) {
                    return;
                }

                ObservableCollection<BaseResourceViewModel> selection = resource.Manager.SelectedItems;

                if (selection.Count < 1 || !selection.Contains(resource)) {
                    this.IsSelected = true;
                }

                List<BaseResourceViewModel> list = selection.ToList();

                try {
                    this.isDragDropping = true;
                    DragDrop.DoDragDrop(this, new DataObject(ResourceListControl.ResourceDropType, list), DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link);
                }
                catch (Exception ex) {
                    AppLogger.WriteLine("Exception while executing resource tree item drag drop: " + ex.GetToString());
                }
                finally {
                    this.isDragDropping = false;
                }
            }
        }

        protected override void OnDragEnter(DragEventArgs e) => this.OnDragOver(e);

        protected override void OnDragOver(DragEventArgs e) {
            if (this.DataContext is ResourceFolderViewModel self) {
                this.IsDroppableTargetOver = ResourceFolderControl.HandleDragOver(self, e);
            }
        }

        protected override void OnDrop(DragEventArgs e) {
            if (!this.isProcessingAsyncDrop && this.DataContext is ResourceFolderViewModel self) {
                if (ResourceFolderControl.CanHandleDrop(self, e, out List<BaseResourceViewModel> list, out EnumDropType effects)) {
                    this.HandleOnDropResources(self, list, effects);
                    this.IsDroppableTargetOver = false;
                }
            }
        }

        protected override void OnDragLeave(DragEventArgs e) {
            this.Dispatcher.Invoke(() => this.IsDroppableTargetOver = false, DispatcherPriority.Loaded);
        }

        private async void HandleOnDropResources(ResourceFolderViewModel folder, List<BaseResourceViewModel> selection, EnumDropType dropType) {
            await folder.OnDropResources(selection, dropType);
            this.ClearValue(IsDroppableTargetOverProperty);
            this.isProcessingAsyncDrop = false;
        }

        protected override bool IsItemItsOwnContainerOverride(object item) {
            return item is ResourceTreeViewItem;
        }

        protected override DependencyObject GetContainerForItemOverride() {
            return new ResourceTreeViewItem();
        }
    }
}