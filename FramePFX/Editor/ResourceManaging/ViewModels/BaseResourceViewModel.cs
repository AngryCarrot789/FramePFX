using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FramePFX.Commands;
using FramePFX.Interactivity;
using FramePFX.Utils;
using FramePFX.Views.Dialogs.Message;
using FramePFX.Views.Dialogs.UserInputs;

namespace FramePFX.Editor.ResourceManaging.ViewModels {
    public abstract class BaseResourceViewModel : BaseViewModel, IRenameTarget {
        private ResourceManagerViewModel manager;
        private ResourceFolderViewModel parent;

        /// <summary>
        /// The manager that this resource is currently associated with
        /// </summary>
        public ResourceManagerViewModel Manager {
            get => this.manager;
            private set {
                if (this.manager != value)
                    this.RaisePropertyChanged(ref this.manager, value);
            }
        }

        /// <summary>
        /// This resource object's parent
        /// </summary>
        public ResourceFolderViewModel Parent {
            get => this.parent;
            private set => this.RaisePropertyChanged(ref this.parent, value);
        }

        /// <summary>
        /// This resource view model's underlying model object
        /// </summary>
        public BaseResourceObject Model { get; }

        public string DisplayName {
            get => this.Model.DisplayName;
            set {
                this.Model.DisplayName = value;
                this.RaisePropertyChanged();
            }
        }

        public AsyncRelayCommand RenameCommand { get; }

        public AsyncRelayCommand DeleteCommand { get; }

        protected BaseResourceViewModel(BaseResourceObject model) {
            this.Model = model;
            this.RenameCommand = new AsyncRelayCommand(this.RenameAsync, () => true);
            this.DeleteCommand = new AsyncRelayCommand(this.DeleteSelfAction, () => this.Parent != null);
        }

        public static void PreSetParent(BaseResourceViewModel obj, ResourceFolderViewModel parent) {
            obj.parent = parent;
        }

        public static void PostSetParent(BaseResourceViewModel obj, ResourceFolderViewModel parent, bool myParent) {
            obj.RaisePropertyChanged(nameof(obj.Parent));
            obj.OnParentChainChanged(myParent);
        }

        public static void SetParent(BaseResourceViewModel obj, ResourceFolderViewModel parent) {
            PreSetParent(obj, parent);
            PostSetParent(obj, parent, true);
        }

        /// <summary>
        /// Invoked when a parent in our hierarchy has changed (may not be our actual <see cref="Parent"/>)
        /// </summary>
        /// <param name="myParent">
        /// True when our actual parent changed, false when the parent changed
        /// higher up our hierarchy (e.g. the parent of our parent)
        /// </param>
        protected internal virtual void OnParentChainChanged(bool myParent) {
        }

        public virtual void SetManager(ResourceManagerViewModel newManager) {
            this.Manager = newManager;
        }

        public virtual async Task<bool> DeleteSelfAction() {
            int index;
            if (this.Parent == null || (index = this.Parent.Items.IndexOf(this)) == -1) {
                await Services.DialogService.ShowMessageAsync("Invalid item", "This resource is not located anywhere...?");
                return false;
            }

            if (this is ResourceItemViewModel) {
                if (await Services.DialogService.ShowDialogAsync("Delete resource?", $"Delete resource{(this.DisplayName != null ? $"'{this.DisplayName}'" : "")}?", MsgDialogType.OKCancel) != MsgDialogResult.OK)
                    return false;
            }
            else if (this is ResourceFolderViewModel group) {
                int total = ResourceFolderViewModel.CountRecursive(group.Items);
                if (total > 0 && await Services.DialogService.ShowDialogAsync("Delete selection?", $"Are you sure you want to delete this resource folder? It has {total} sub-item{Lang.S(total)}?", MsgDialogType.OKCancel) != MsgDialogResult.OK)
                    return false;
            }

#if DEBUG
            this.Parent.RemoveItemAndDisposeAt(index);
#else
            try
            {
                this.Parent.DisposeAndRemoveItemAt(index);
            }
            catch (Exception e)
            {
                await Services.DialogService.ShowMessageExAsync("Error deleting item", "An exception occurred while deleting this item", e.GetToString());
            }
#endif

            return true;
        }

        /// <summary>
        /// Called when the model associated with this view model is disposed
        /// </summary>
        protected virtual void OnModelDisposed() {
        }

        public async Task<bool> RenameAsync() {
            // testing that the UI functions can be called from other threads
            string result = null;
            await Task.Run(async () => {
                result = await Services.UserInput.ShowSingleInputDialogAsync("Rename group", "Input a new name for this group", this.DisplayName, Validators.ForNonWhiteSpaceString());
            });

            if (string.IsNullOrWhiteSpace(result)) {
                return false;
            }
            else if (this.Parent != null) {
                this.DisplayName = TextIncrement.GetNextText(this.Parent.Items.OfType<ResourceFolderViewModel>().Select(x => x.DisplayName), result);
            }
            else {
                this.DisplayName = result;
            }

            return true;
        }

        /// <summary>
        /// Disposes the model, and then calls <see cref="OnModelDisposed"/>
        /// </summary>
        public void Dispose() {
            using (ErrorList list = new ErrorList()) {
                try {
                    this.Model.Dispose();
                }
                catch (Exception e) {
                    list.Add(new Exception("Failed to dispose model", e));
                }

                this.OnModelDisposed();
            }
        }

        public static bool CanDropItems(List<BaseResourceViewModel> items, ResourceFolderViewModel target, EnumDropType dropType) {
            if (items.Count == 1) {
                BaseResourceViewModel item = items[0];
                if (item is ResourceFolderViewModel folder && folder.IsParentInHierarchy(target)) {
                    return false;
                }
                else if (ReferenceEquals(item, target)) {
                    return false;
                }
                else if (dropType != EnumDropType.Copy && dropType != EnumDropType.Link) {
                    if (target.Items.Contains(item)) {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}