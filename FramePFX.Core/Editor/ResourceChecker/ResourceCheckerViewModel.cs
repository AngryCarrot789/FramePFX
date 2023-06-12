using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using FramePFX.Core.Editor.ResourceManaging.ViewModels;
using FramePFX.Core.Editor.ViewModels;
using FramePFX.Core.Utils;
using FramePFX.Core.Views.Dialogs;

namespace FramePFX.Core.Editor.ResourceChecker {
    public class ResourceCheckerViewModel : BaseViewModel {
        private readonly ObservableCollectionEx<InvalidResourceViewModel> resources;
        public ReadOnlyObservableCollection<InvalidResourceViewModel> Resources { get; }

        private InvalidResourceViewModel currentItem;
        public InvalidResourceViewModel CurrentItem {
            get => this.currentItem;
            set => this.RaisePropertyChanged(ref this.currentItem, value);
        }

        private int currentIndex;
        public int CurrentIndex {
            get => this.currentIndex;
            set => this.RaisePropertyChanged(ref this.currentIndex, value);
        }

        public AsyncRelayCommand CancelCommand { get; }
        public AsyncRelayCommand OfflineCurrentCommand { get; }
        public AsyncRelayCommand OfflineAllCommand { get; }

        public IDialog Dialog { get; set; }

        public ResourceCheckerViewModel() {
            this.resources = new ObservableCollectionEx<InvalidResourceViewModel>();
            this.Resources = new ReadOnlyObservableCollection<InvalidResourceViewModel>(this.resources);
            this.CancelCommand = new AsyncRelayCommand(this.CancelAction);
            this.OfflineCurrentCommand = new AsyncRelayCommand(this.OfflineCurrentAction);
            this.OfflineAllCommand = new AsyncRelayCommand(this.OfflineAllAction);
        }

        public static Task<bool> ProcessProjectForInvalidResources(ProjectViewModel project, bool forceValidate) {
            return Task.FromResult(true);
            // return ProcessResources(project.ResourceManager.Resources, forceValidate);
        }

        /// <summary>
        /// Processes the given resources and checks if they are all valid. If not, it shows a window to try and fix them
        /// <para>
        /// The user can cancel the action, cancelling the project from being loaded, causing the task to return false. Otherwise, returns true
        /// </para>
        /// </summary>
        /// <param name="resources"></param>
        /// <param name="forceValidate">If the resource was forced offline by the user, setting this to true will force it to be validated anyway</param>
        /// <returns>Whether the UI operation was successful or cancelled</returns>
        public static async Task<bool> ProcessResources(IEnumerable<ResourceItemViewModel> resources, bool forceValidate = false) {
            ResourceCheckerViewModel checker = new ResourceCheckerViewModel();
            using (ExceptionStack stack = new ExceptionStack(false)) {
                foreach (ResourceItemViewModel resource in resources) {
                    if (forceValidate || !resource.Model.IsOfflineByUser) {
                        bool isOnline = await resource.ValidateOnlineState(checker, stack);
                        if (isOnline != resource.Model.IsOnline) {
                            resource.Model.IsOnline = isOnline;
                            resource.Model.OnIsOnlineStateChanged();
                        }
                    }
                }

                if (stack.TryGetException(out Exception exception)) {
                    await IoC.MessageDialogs.ShowMessageExAsync("Exceptions", "One or more exceptions occurred while checking the resource validation states. This can be ignored", exception.GetToString());
                }
            }

            if (checker.Resources.Count < 1) {
                return true;
            }

            return await IoC.Provide<IResourceCheckerService>().ShowCheckerDialog(checker);
        }

        private async Task CancelAction() {
            await this.Dialog.CloseDialogAsync(false);
        }

        private async Task OfflineCurrentAction() {
            int index = this.currentIndex;
            if (index >= 0 && index < this.resources.Count) {
                InvalidResourceViewModel resource = this.resources[index];
                await resource.SetResourceOfflineAsync();
                resource.Checker = null;
                this.resources.RemoveAt(index);
            }

            if (this.resources.Count < 1) {
                await this.Dialog.CloseDialogAsync(true);
            }
        }

        private async Task OfflineAllAction() {
            foreach (InvalidResourceViewModel item in this.resources) {
                await item.SetResourceOfflineAsync();
                item.Checker = null;
            }

            this.resources.Clear();
            await this.Dialog.CloseDialogAsync(true);
        }

        public async Task<bool> RemoveItemAction(InvalidResourceViewModel item) {
            int index = this.resources.IndexOf(item);
            if (index < 0) {
                return false;
            }

            item.Checker = null;
            this.resources.RemoveAt(index);
            if (this.resources.Count < 1) {
                await this.Dialog.CloseDialogAsync(true);
            }

            return true;
        }

        public void Add(InvalidResourceViewModel item) {
            item.Checker = this;
            this.resources.Add(item);
        }
    }
}
