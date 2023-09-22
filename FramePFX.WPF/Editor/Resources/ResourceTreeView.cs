using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using FramePFX.Editor.ResourceManaging.ViewModels;
using FramePFX.Interactivity;
using FramePFX.WPF.Controls.TreeViews.Controls;

namespace FramePFX.WPF.Editor.Resources {
    internal class ResourceTreeView : MultiSelectTreeView {
        public ResourceManagerViewModel ViewModel => (ResourceManagerViewModel) this.DataContext;

        private bool isProcessingAsyncDrop;

        public ResourceTreeView() {
            this.AllowDrop = true;
        }

        static ResourceTreeView() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ResourceTreeView), new FrameworkPropertyMetadata(typeof(ResourceTreeView)));
        }

        protected override void OnDragEnter(DragEventArgs e) {
            base.OnDragEnter(e);
            this.OnDragOver(e);
        }

        protected override void OnDragOver(DragEventArgs e) {
            ResourceManagerViewModel manager = this.ViewModel;
            if (manager == null) {
                return;
            }

            e.Handled = true;
            if (e.Data.GetData(ResourceListControl.ResourceDropType) is List<BaseResourceObjectViewModel> list) {
                if (!list.Contains(manager.Root)) {
                    e.Effects = (DragDropEffects) DropUtils.GetDropAction((int) e.KeyStates, (EnumDropType) e.Effects);
                    return;
                }
            }

            e.Effects = DragDropEffects.None;
        }

        protected override void OnDrop(DragEventArgs e) {
            if (this.isProcessingAsyncDrop) {
                return;
            }

            ResourceManagerViewModel manager = this.ViewModel;
            if (manager == null) {
                return;
            }

            if (e.Data.GetDataPresent(ResourceListControl.ResourceDropType)) {
                object obj = e.Data.GetData(ResourceListControl.ResourceDropType);
                if (obj is List<BaseResourceObjectViewModel> resources && !resources.Contains(manager.Root)) {
                    this.HandleOnDropResources(manager.Root, resources, DropUtils.GetDropAction((int) e.KeyStates, (EnumDropType) e.Effects));
                    e.Handled = true;
                }
            }
        }

        private async void HandleOnDropResources(ResourceGroupViewModel group, List<BaseResourceObjectViewModel> selection, EnumDropType dropType) {
            await group.OnDropResources(selection, dropType);
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
