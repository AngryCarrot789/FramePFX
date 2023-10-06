using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using FramePFX.Editor.ResourceManaging.ViewModels;
using FramePFX.Interactivity;

namespace FramePFX.WPF.Editor.Resources {
    public class ResourceFolderControl : BaseResourceItemControl {
        public INavigatableResource Resource => this.DataContext as INavigatableResource;

        private bool isProcessingAsyncDrop;

        public ResourceFolderControl() {
            this.AllowDrop = true;
        }

        protected override void OnDragEnter(DragEventArgs e) => this.OnDragOver(e);

        protected override void OnDragOver(DragEventArgs e) {
            if (this.DataContext is ResourceFolderViewModel self) {
                this.IsDroppableTargetOver = HandleDragOver(self, e);
            }
        }

        protected override void OnDrop(DragEventArgs e) {
            if (!this.isProcessingAsyncDrop && this.DataContext is ResourceFolderViewModel self) {
                if (CanHandleDrop(self, e, out List<BaseResourceViewModel> list, out EnumDropType effects)) {
                    this.HandleOnDropResources(self, list, effects);
                    this.IsDroppableTargetOver = false;
                }
            }
        }

        protected override void OnDragLeave(DragEventArgs e) {
            this.Dispatcher.Invoke(() => this.IsDroppableTargetOver = false, DispatcherPriority.Loaded);
        }

        public static bool HandleDragOver(ResourceFolderViewModel folder, DragEventArgs e) {
            if (!e.Data.GetDataPresent(ResourceListControl.ResourceDropType)) {
                return false;
            }

            object obj = e.Data.GetData(ResourceListControl.ResourceDropType);
            if (!(obj is List<BaseResourceViewModel> resources)) {
                return false;
            }

            e.Handled = true;
            EnumDropType effects = DropUtils.GetDropAction((int) e.KeyStates, (EnumDropType) e.Effects);
            e.Effects = (DragDropEffects) BaseResourceViewModel.CanDropItems(resources, folder, effects);
            return e.Effects != DragDropEffects.None;
        }

        public static bool CanHandleDrop(ResourceFolderViewModel folder, DragEventArgs e, out List<BaseResourceViewModel> resources, out EnumDropType effects) {
            if (!e.Data.GetDataPresent(ResourceListControl.ResourceDropType)) {
                effects = EnumDropType.None;
                resources = null;
                return false;
            }

            object obj = e.Data.GetData(ResourceListControl.ResourceDropType);
            if ((resources = obj as List<BaseResourceViewModel>) != null) {
                e.Handled = true;
                effects = DropUtils.GetDropAction((int) e.KeyStates, (EnumDropType) e.Effects);
                effects = BaseResourceViewModel.CanDropItems(resources, folder, effects);
                return effects != EnumDropType.None;
            }

            effects = EnumDropType.None;
            return false;
        }

        private async void HandleOnDropResources(ResourceFolderViewModel folder, List<BaseResourceViewModel> selection, EnumDropType dropType) {
            await folder.OnDropResources(selection, dropType);
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