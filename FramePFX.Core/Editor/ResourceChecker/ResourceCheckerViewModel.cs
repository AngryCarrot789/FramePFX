using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using FramePFX.Core.Editor.ResourceManaging.ViewModels;
using FramePFX.Core.Editor.ViewModels;
using FramePFX.Core.Utils;
using FramePFX.Core.Views.Dialogs;

namespace FramePFX.Core.Editor.ResourceChecker
{
    public class ResourceCheckerViewModel : BaseViewModel
    {
        private readonly ObservableCollectionEx<InvalidResourceViewModel> resources;
        public ReadOnlyObservableCollection<InvalidResourceViewModel> Resources { get; }

        private InvalidResourceViewModel currentItem;

        public InvalidResourceViewModel CurrentItem
        {
            get => this.currentItem;
            set => this.RaisePropertyChanged(ref this.currentItem, value);
        }

        private int currentIndex;

        public int CurrentIndex
        {
            get => this.currentIndex;
            set => this.RaisePropertyChanged(ref this.currentIndex, value);
        }

        public AsyncRelayCommand CancelCommand { get; }
        public AsyncRelayCommand OfflineCurrentCommand { get; }
        public AsyncRelayCommand OfflineAllCommand { get; }

        public IDialog Dialog { get; set; }

        public ResourceCheckerViewModel()
        {
            this.resources = new ObservableCollectionEx<InvalidResourceViewModel>();
            this.Resources = new ReadOnlyObservableCollection<InvalidResourceViewModel>(this.resources);
            this.CancelCommand = new AsyncRelayCommand(this.CancelAction);
            this.OfflineCurrentCommand = new AsyncRelayCommand(this.OfflineCurrentAction);
            this.OfflineAllCommand = new AsyncRelayCommand(this.OfflineAllAction);
        }

        public static Task<bool> LoadProjectResources(ProjectViewModel project, bool forceValidate)
        {
            return LoadResources(project.ResourceManager.Root.Items, forceValidate);
        }

        /// <summary>
        /// Processes the given resources and checks if they are all valid. If not, it shows a window to try and fix them
        /// <para>
        /// The user can cancel the action, cancelling the project from being loaded, causing the task to return false. Otherwise, returns true
        /// </para>
        /// </summary>
        /// <param name="resources"></param>
        /// <param name="ignoreUserOffline">If the resource was forced offline by the user, setting this to true will force it to be validated anyway</param>
        /// <returns>Whether the UI operation was successful or cancelled</returns>
        public static async Task<bool> LoadResources(IEnumerable<BaseResourceObjectViewModel> resources, bool ignoreUserOffline = false)
        {
            ResourceCheckerViewModel checker = new ResourceCheckerViewModel();
            using (ExceptionStack stack = new ExceptionStack(false))
            {
                foreach (BaseResourceObjectViewModel resourceObject in resources.ToList())
                {
                    await LoadResourcesRecursive(checker, resourceObject, stack, ignoreUserOffline);
                }

                if (stack.TryGetException(out Exception exception))
                {
                    await IoC.MessageDialogs.ShowMessageExAsync("Exceptions", "One or more exceptions occurred while checking the resource validation states. This can be ignored", exception.GetToString());
                }
            }

            if (checker.Resources.Count < 1)
            {
                return true;
            }

            return await IoC.Provide<IResourceCheckerService>().ShowCheckerDialog(checker);
        }

        private static async Task LoadResourcesRecursive(ResourceCheckerViewModel checker, BaseResourceObjectViewModel resourceObject, ExceptionStack stack, bool ignoreUserOffline = false)
        {
            if (resourceObject is ResourceItemViewModel item)
            {
                if (!item.Model.IsOnline && (ignoreUserOffline || !item.Model.IsOfflineByUser))
                {
                    bool isOnline = await item.LoadResource(checker, stack);
                    if (isOnline != item.Model.IsOnline)
                    {
                        item.Model.IsOnline = isOnline;
                        item.Model.OnIsOnlineStateChanged();
                    }
                }
            }
            else if (resourceObject is ResourceGroupViewModel group)
            {
                foreach (BaseResourceObjectViewModel obj in group.Items)
                {
                    await LoadResourcesRecursive(checker, obj, stack, ignoreUserOffline);
                }
            }
        }

        private async Task CancelAction()
        {
            await this.Dialog.CloseDialogAsync(false);
        }

        private async Task OfflineCurrentAction()
        {
            int index = this.currentIndex;
            if (index >= 0 && index < this.resources.Count)
            {
                InvalidResourceViewModel resource = this.resources[index];
                await resource.SetResourceOfflineAsync();
                resource.Checker = null;
                this.resources.RemoveAt(index);
            }

            if (this.resources.Count < 1)
            {
                await this.Dialog.CloseDialogAsync(true);
            }
        }

        private async Task OfflineAllAction()
        {
            foreach (InvalidResourceViewModel item in this.resources)
            {
                await item.SetResourceOfflineAsync();
                item.Checker = null;
            }

            this.resources.Clear();
            await this.Dialog.CloseDialogAsync(true);
        }

        public async Task<bool> RemoveItemAction(InvalidResourceViewModel item)
        {
            int index = this.resources.IndexOf(item);
            if (index < 0)
            {
                return false;
            }

            item.Checker = null;
            this.resources.RemoveAt(index);
            if (this.resources.Count < 1)
            {
                await this.Dialog.CloseDialogAsync(true);
            }

            return true;
        }

        public void Add(InvalidResourceViewModel item)
        {
            item.Checker = this;
            this.resources.Add(item);
        }
    }
}