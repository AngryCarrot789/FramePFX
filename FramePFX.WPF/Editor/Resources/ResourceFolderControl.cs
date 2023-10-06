using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using FramePFX.Editor.ResourceManaging.ViewModels;
using FramePFX.Interactivity;
using FramePFX.WPF.Interactivity;

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
                this.IsDroppableTargetOver = ProcessCanDragOver(self, e);
            }
        }

        protected override async void OnDrop(DragEventArgs e) {
            e.Handled = true;
            if (this.isProcessingAsyncDrop || !(this.DataContext is ResourceFolderViewModel self)) {
                return;
            }

            try {
                this.isProcessingAsyncDrop = true;
                if (GetDropResourceListForEvent(e, out List<BaseResourceViewModel> list, out EnumDropType effects)) {
                    await BaseResourceViewModel.DropRegistry.OnDropped(self, list, effects);
                }
                else if (!await BaseResourceViewModel.DropRegistry.OnDroppedNative(self, new DataObjectWrapper(e.Data), effects)) {
                    await Services.DialogService.ShowMessageAsync("Unknown data", "Unknown dropped item. Drop files here");
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

        public static bool ProcessCanDragOver(ResourceFolderViewModel folder, DragEventArgs e) {
            e.Handled = true;
            if (GetDropResourceListForEvent(e, out List<BaseResourceViewModel> resources, out EnumDropType effects)) {
                e.Effects = (DragDropEffects) BaseResourceViewModel.DropRegistry.CanDrop(folder, resources, effects);
            }
            else {
                e.Effects = (DragDropEffects) BaseResourceViewModel.DropRegistry.CanDropNative(folder, new DataObjectWrapper(e.Data), effects);
            }

            return e.Effects != DragDropEffects.None;
        }

        /// <summary>
        /// Tries to get the list of resources being drag-dropped from the given drag event. Provides the
        /// effects currently applicable for the event regardless of this method's return value
        /// </summary>
        /// <param name="e">Drag event (enter, over, drop, etc.)</param>
        /// <param name="resources">The resources in the drag event</param>
        /// <param name="effects">The effects applicable based on the event's effects and user's keys pressed</param>
        /// <returns>True if there were resources available, otherwise false, meaning no resources are being dragged</returns>
        public static bool GetDropResourceListForEvent(DragEventArgs e, out List<BaseResourceViewModel> resources, out EnumDropType effects) {
            effects = DropUtils.GetDropAction((int) e.KeyStates, (EnumDropType) e.Effects);
            if (e.Data.GetDataPresent(ResourceListControl.ResourceDropType)) {
                object obj = e.Data.GetData(ResourceListControl.ResourceDropType);
                if ((resources = obj as List<BaseResourceViewModel>) != null) {
                    return true;
                }
            }

            resources = null;
            return false;
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