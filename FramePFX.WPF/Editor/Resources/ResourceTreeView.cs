using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using FramePFX.Editor.ResourceManaging.ViewModels;
using FramePFX.Interactivity;
using FramePFX.Utils;
using FramePFX.WPF.Controls.TreeViews.Controls;

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
                this.IsDroppableTargetOver = ResourceFolderControl.HandleDragOver(manager.Root, e);
            }
        }

        protected override void OnDrop(DragEventArgs e) {
            if (!this.isProcessingAsyncDrop && this.DataContext is ResourceManagerViewModel manager) {
                if (ResourceFolderControl.CanHandleDrop(manager.Root, e, out List<BaseResourceViewModel> list, out EnumDropType effects)) {
                    this.HandleOnDropResources(manager.Root, list, effects);
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