using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using FramePFX.Editor.ResourceManaging.ViewModels;
using FramePFX.Interactivity;
using FramePFX.Utils;

namespace FramePFX.WPF.Editor.Resources {
    public class ResourceGroupControl : BaseResourceItemControl {
        public INavigatableResource Resource => this.DataContext as INavigatableResource;

        private bool isProcessingAsyncDrop;

        public ResourceGroupControl() {
            this.AllowDrop = true;
        }

        protected override void OnDragEnter(DragEventArgs e) {
            base.OnDragEnter(e);
            this.OnDragOver(e);
        }

        protected override void OnDragOver(DragEventArgs e) {
            e.Handled = true;
            if (e.Data.GetData(ResourceListControl.ResourceDropType) is List<BaseResourceObjectViewModel> list) {
                if (this.DataContext is BaseResourceObjectViewModel vm && !list.Contains(vm)) {
                    e.Effects = (DragDropEffects) DropUtils.GetDropAction((int) e.KeyStates, (EnumDropType) e.Effects);
                    if (e.Effects != DragDropEffects.None) {
                        this.IsDroppableTargetOver = true;
                    }

                    return;
                }
            }

            this.ClearValue(IsDroppableTargetOverProperty);
            e.Effects = DragDropEffects.None;
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
                    this.HandleOnDropResources(target, resources, DropUtils.GetDropAction((int) e.KeyStates, (EnumDropType) e.Effects));
                    e.Handled = true;
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
            if (e.ChangedButton != MouseButton.Left) {
                return;
            }

            this.Resource?.OnNavigate();
        }
    }
}