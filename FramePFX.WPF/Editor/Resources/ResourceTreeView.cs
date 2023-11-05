using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using FramePFX.Editor.ResourceManaging.ViewModels;
using FramePFX.Interactivity;
using FramePFX.Utils;
using FramePFX.WPF.Controls.TreeViews.Controls;
using FramePFX.WPF.Interactivity;

namespace FramePFX.WPF.Editor.Resources {
    internal class ResourceTreeView : MultiSelectTreeView {
        public static readonly DependencyProperty IsDroppableTargetOverProperty = DependencyProperty.Register("IsDroppableTargetOver", typeof(bool), typeof(ResourceTreeView), new PropertyMetadata(BoolBox.False));

        public ResourceManagerViewModel ViewModel => (ResourceManagerViewModel) this.DataContext;

        public bool IsDroppableTargetOver {
            get => (bool) this.GetValue(IsDroppableTargetOverProperty);
            set => this.SetValue(IsDroppableTargetOverProperty, value.Box());
        }

        private bool isProcessingAsyncDrop;

        public ResourceTreeView() {
            this.AllowDrop = true;
        }

        static ResourceTreeView() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ResourceTreeView), new FrameworkPropertyMetadata(typeof(ResourceTreeView)));
        }

        protected override void OnDragEnter(DragEventArgs e) => this.OnDragOver(e);

        protected override void OnDragOver(DragEventArgs e) {
            if (this.DataContext is ResourceManagerViewModel manager) {
                this.IsDroppableTargetOver = ResourceFolderControl.ProcessCanDragOver(manager.Root, e);
            }
        }

        protected override async void OnDrop(DragEventArgs e) {
            e.Handled = true;
            if (this.isProcessingAsyncDrop || !(this.DataContext is ResourceManagerViewModel manager)) {
                return;
            }

            try {
                this.isProcessingAsyncDrop = true;
                if (ResourceFolderControl.GetDropResourceListForEvent(e, out List<BaseResourceViewModel> list, out EnumDropType effects)) {
                    await BaseResourceViewModel.DropRegistry.OnDropped(manager.Root, list, effects);
                }
                else if (!await BaseResourceViewModel.DropRegistry.OnDroppedNative(manager.Root, new DataObjectWrapper(e.Data), effects)) {
                    await IoC.DialogService.ShowMessageAsync("Unknown data", "Unknown dropped item. Drop files here");
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