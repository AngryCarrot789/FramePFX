using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Policy;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using FramePFX.Editor.ResourceManaging.ViewModels;
using FramePFX.Interactivity;
using FramePFX.PropertyEditing;
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

        public BaseResourceObjectViewModel ViewModel => (BaseResourceObjectViewModel) this.DataContext;

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

        protected override void OnMouseDown(MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left && CanBeginDragDrop() && !e.Handled) {
                if ((this.IsFocused || this.Focus()) && !this.isDragDropping) {
                    this.CaptureMouse();
                    this.originMousePoint = e.GetPosition(this);
                    this.isDragActive = true;
                    e.Handled = true;
                    return;
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

                e.Handled = true;
                if (e.ChangedButton == MouseButton.Left) {
                    this.ParentTreeView.Selection.Select(this);
                }
                else if (!this.IsSelected) {
                    this.ParentTreeView.Selection.Select(this);
                }
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
                if (!(this.DataContext is BaseResourceObjectViewModel resource) || resource.Manager == null) {
                    return;
                }

                ObservableCollection<BaseResourceObjectViewModel> selection = resource.Manager.SelectedItems;

                if (selection.Count < 1 || !selection.Contains(resource)) {
                    this.IsSelected = true;
                }

                List<BaseResourceObjectViewModel> list = selection.ToList();

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

        protected override void OnDragEnter(DragEventArgs e) {
            base.OnDragEnter(e);
            this.OnDragOver(e);
        }

        protected override void OnDragOver(DragEventArgs e) {
            if (!this.isDragDropping && e.Data.GetData(ResourceListControl.ResourceDropType) is List<BaseResourceObjectViewModel> list) {
                if (this.DataContext is BaseResourceObjectViewModel vm && !list.Contains(vm)) {
                    this.IsDroppableTargetOver = true;
                    e.Effects = (DragDropEffects) DropUtils.GetDropAction((int) e.KeyStates, (EnumDropType) e.Effects);
                    e.Handled = true;
                    goto end;
                }
            }

            this.ClearValue(IsDroppableTargetOverProperty);
            e.Effects = DragDropEffects.None;

            end:
            e.Handled = true;
            base.OnDragOver(e);
        }

        protected override void OnDragLeave(DragEventArgs e) {
            base.OnDragLeave(e);
            this.Dispatcher.Invoke(() => {
                this.ClearValue(IsDroppableTargetOverProperty);
            }, DispatcherPriority.Loaded);
        }

        protected override void OnDrop(DragEventArgs e) {
            if (this.isProcessingAsyncDrop) {
                return;
            }

            if (e.Data.GetDataPresent(ResourceListControl.ResourceDropType)) {
                object obj = e.Data.GetData(ResourceListControl.ResourceDropType);
                if (obj is List<BaseResourceObjectViewModel> resources && this.DataContext is ResourceGroupViewModel target) {
                    if (!resources.Contains(target)) {
                        this.HandleOnDropResources(target, resources, DropUtils.GetDropAction((int) e.KeyStates, (EnumDropType) e.Effects));
                        e.Handled = true;
                    }
                }
            }
        }

        private async void HandleOnDropResources(ResourceGroupViewModel group, List<BaseResourceObjectViewModel> selection, EnumDropType dropType) {
            await group.OnDropResources(selection, dropType);
            this.ClearValue(IsDroppableTargetOverProperty);
            this.isProcessingAsyncDrop = false;
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e) {
            base.OnMouseDoubleClick(e);
            // if (e.ChangedButton != MouseButton.Left) {
            //     return;
            // }
            // if (!KeyboardUtils.AreModifiersPressed(ModifierKeys.Control)) {
            //     if (this.DataContext is ResourceGroupViewModel group) {
            //         group.OnNavigate();
            //     }
            // }
        }

        protected override bool IsItemItsOwnContainerOverride(object item) {
            return item is ResourceTreeViewItem;
        }

        protected override DependencyObject GetContainerForItemOverride() {
            return new ResourceTreeViewItem();
        }
    }
}