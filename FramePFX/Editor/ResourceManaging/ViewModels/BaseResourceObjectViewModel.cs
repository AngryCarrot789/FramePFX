using System;
using System.Linq;
using System.Threading.Tasks;
using FramePFX.Utils;
using FramePFX.Views.Dialogs.Message;
using FramePFX.Views.Dialogs.UserInputs;

namespace FramePFX.Editor.ResourceManaging.ViewModels {
    public abstract class BaseResourceObjectViewModel : BaseViewModel, IRenameTarget {
        internal ResourceManagerViewModel manager;
        internal ResourceGroupViewModel parent;

        /// <summary>
        /// The manager that this resource is currently associated with
        /// </summary>
        public ResourceManagerViewModel Manager {
            get => this.manager;
            protected set => this.RaisePropertyChanged(ref this.manager, value);
        }

        /// <summary>
        /// This resource object's parent
        /// </summary>
        public ResourceGroupViewModel Parent {
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

        protected BaseResourceObjectViewModel(BaseResourceObject model) {
            this.Model = model;
            this.RenameCommand = new AsyncRelayCommand(this.RenameAsync, () => true);
            this.DeleteCommand = new AsyncRelayCommand(this.DeleteSelfAction, () => this.Parent != null);
        }

        public virtual void SetParent(ResourceGroupViewModel newParent) {
            this.Parent = newParent;
            this.OnParentChainChanged();
        }

        protected internal virtual void OnParentChainChanged() {
        }

        public virtual void SetManager(ResourceManagerViewModel newManager) {
            this.Manager = newManager;
        }

        public virtual async Task<bool> DeleteSelfAction() {
            int index;
            if (this.Parent == null || (index = this.Parent.Items.IndexOf(this)) == -1) {
                await IoC.MessageDialogs.ShowMessageAsync("Invalid item", "This resource is not located anywhere...?");
                return false;
            }

            if (this is ResourceItemViewModel resource) {
                if (await IoC.MessageDialogs.ShowDialogAsync("Delete resource?", $"Delete resource{(this.DisplayName != null ? $"'{this.DisplayName}'" : "")}?", MsgDialogType.OKCancel) != MsgDialogResult.OK)
                    return false;
            }
            else if (this is ResourceGroupViewModel group) {
                int total = ResourceGroupViewModel.CountRecursive(group.Items);
                if (total > 0 && await IoC.MessageDialogs.ShowDialogAsync("Delete selection?", $"Are you sure you want to delete this resource group? It has {total} sub-item{Lang.S(total)}?", MsgDialogType.OKCancel) != MsgDialogResult.OK)
                    return false;
            }

#if DEBUG
            this.Parent.DisposeAndRemoveItemAt(index);
#else
            try
            {
                this.Parent.DisposeAndRemoveItemAt(index);
            }
            catch (Exception e)
            {
                await IoC.MessageDialogs.ShowMessageExAsync("Error deleting item", "An exception occurred while deleting this item", e.GetToString());
            }
#endif

            return true;
        }

        /// <summary>
        /// Called when the model associated with this view model is disposed
        /// </summary>
        /// <param name="list"></param>
        protected virtual void OnModelDisposed(ErrorList list) {
        }

        public async Task<bool> RenameAsync() {
            string result = await IoC.UserInput.ShowSingleInputDialogAsync("Rename group", "Input a new name for this group", this.DisplayName, Validators.ForNonWhiteSpaceString());
            if (string.IsNullOrWhiteSpace(result)) {
                return false;
            }
            else if (this.Parent != null) {
                this.DisplayName = TextIncrement.GetNextText(this.Parent.Items.OfType<ResourceGroupViewModel>().Select(x => x.DisplayName), result);
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

                this.OnModelDisposed(list);
            }
        }
    }
}