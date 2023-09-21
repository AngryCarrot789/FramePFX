using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using FramePFX.Commands;
using FramePFX.Editor.ResourceManaging.ViewModels;
using FramePFX.Editor.ViewModels;
using FramePFX.Utils;
using FramePFX.Views.Dialogs;

namespace FramePFX.Editor.ResourceChecker {
    public class ResourceCheckerViewModel : BaseViewModel {
        private readonly ObservableCollectionEx<InvalidResourceViewModel> resources;
        private InvalidResourceViewModel currentItem;
        private string caption;

        /// <summary>
        /// A collection of invalid resource resolvers that are present
        /// </summary>
        public ReadOnlyObservableCollection<InvalidResourceViewModel> Resources { get; }

        /// <summary>
        /// The item that is currently selected in the UI
        /// </summary>
        public InvalidResourceViewModel CurrentItem {
            get => this.currentItem;
            set => this.RaisePropertyChanged(ref this.currentItem, value);
        }

        /// <summary>
        /// A piece of text to show in the caption/titlebar
        /// </summary>
        public string Caption {
            get => this.caption;
            set => this.RaisePropertyChanged(ref this.caption, value);
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
            this.caption = "Resolve resource errors";
        }

        public static Task<bool> LoadProjectResources(ProjectViewModel project, bool forceOnline) {
            return LoadProjectResources(new ResourceCheckerViewModel(), project, forceOnline);
        }

        public static Task<bool> LoadProjectResources(ResourceCheckerViewModel checker, ProjectViewModel project, bool forceOnline) {
            return LoadResources(checker, project.ResourceManager.Root.Items.ToList(), forceOnline);
        }

        public static Task<bool> LoadResources(IEnumerable<BaseResourceObjectViewModel> resources, bool forceOnline = false) {
            return LoadResources(new ResourceCheckerViewModel(), resources, forceOnline);
        }

        /// <summary>
        /// Processes the given resources and checks if they are all valid. If not, it shows a window to try and fix them
        /// <para>
        /// The user can cancel the action, cancelling the project from being loaded, causing the task to return false. Otherwise, returns true
        /// </para>
        /// </summary>
        /// <param name="resources"></param>
        /// <param name="forceOnline">Force loads a resource if it's offline, ignoring if the user set it to offline</param>
        /// <returns>Whether the UI operation was successful or cancelled</returns>
        public static async Task<bool> LoadResources(ResourceCheckerViewModel checker, IEnumerable<BaseResourceObjectViewModel> resources, bool forceOnline = false) {
            if (checker == null) {
                throw new ArgumentNullException(nameof(checker));
            }

            foreach (BaseResourceObjectViewModel obj in resources) {
                await LoadResourcesRecursive(checker, obj, forceOnline);
            }

            if (checker.Resources.Count < 1) {
                return true;
            }

            return await Services.GetService<IResourceCheckerService>().ShowCheckerDialog(checker);
        }

        private static async Task LoadResourcesRecursive(ResourceCheckerViewModel checker, BaseResourceObjectViewModel resource, bool forceOnline = false) {
            if (resource is ResourceItemViewModel item) {
                if (!item.IsOnline && (forceOnline || !item.IsOfflineByUser)) {
                    await ResourceItemViewModel.TryLoadResource(item, checker);
                }
            }
            else if (resource is ResourceGroupViewModel group) {
                foreach (BaseResourceObjectViewModel obj in group.Items) {
                    await LoadResourcesRecursive(checker, obj, forceOnline);
                }
            }
        }

        private async Task CancelAction() {
            await this.Dialog.CloseDialogAsync(false);
        }

        private async Task OfflineCurrentAction() {
            int index = this.resources.IndexOf(this.currentItem);
            if (index >= 0 && index < this.resources.Count) {
                InvalidResourceViewModel resource = this.currentItem;
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