using System;
using System.Linq;
using System.Threading.Tasks;
using FramePFX.Commands;
using FramePFX.Interactivity;
using FramePFX.Utils;
using FramePFX.Utils.Collections;
using FramePFX.Views.Dialogs.Message;
using FramePFX.Views.Dialogs.UserInputs;

namespace FramePFX.Editor.ResourceManaging.ViewModels {
    public abstract class BaseResourceViewModel : BaseViewModel, IRenameTarget {
        protected static readonly PropertyMap PropertyMap;

        private ResourceManagerViewModel manager;
        private ResourceFolderViewModel parent;

        public static DragDropRegistry<BaseResourceViewModel> DropRegistry { get; }

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
        public BaseResource Model { get; }

        public string DisplayName {
            get => this.Model.DisplayName;
            set => this.Model.DisplayName = value;
        }

        public AsyncRelayCommand RenameCommand { get; }

        public AsyncRelayCommand DeleteCommand { get; }

        protected BaseResourceViewModel(BaseResource model) {
            this.Model = model ?? throw new ArgumentNullException(nameof(model));
            this.RenameCommand = new AsyncRelayCommand(this.RenameAsync, () => true);
            this.DeleteCommand = new AsyncRelayCommand(this.DeleteSelfAction, () => this.Parent != null);
            model.ViewModel = this;
            model.DataModified += this.OnModelDataModified;
        }

        protected virtual void OnModelDataModified(BaseResource item, string property) {
            if (PropertyMap.GetPropertyForModel(item.GetType(), property, out string prop)) {
                this.RaisePropertyChanged(prop);
            }
        }

        static BaseResourceViewModel() {
            PropertyMap = new PropertyMap();
            DropRegistry = new DragDropRegistry<BaseResourceViewModel>();

            AddPropertyTranslation<BaseResource>(nameof(BaseResource.DisplayName), nameof(DisplayName));
        }

        protected static void AddPropertyTranslation<T>(string modelProperty, string viewModelProperty) where T : BaseResource {
            PropertyMap.AddTranslation(typeof(T), modelProperty, viewModelProperty);
        }

        public static void PreSetParent(BaseResourceViewModel obj, ResourceFolderViewModel parent) {
            obj.parent = parent;
        }

        public static void PostSetParent(BaseResourceViewModel obj, ResourceFolderViewModel parent) {
            obj.RaisePropertyChanged(nameof(obj.Parent));
        }

        public static void SetParent(BaseResourceViewModel obj, ResourceFolderViewModel parent) {
            PreSetParent(obj, parent);
            PostSetParent(obj, parent);
        }

        public virtual void SetManager(ResourceManagerViewModel newManager) {
            this.Manager = newManager;
        }

        public virtual async Task<bool> DeleteSelfAction() {
            int index;
            if (this.Parent == null || (index = this.Parent.Items.IndexOf(this)) == -1) {
                await IoC.DialogService.ShowMessageAsync("Invalid item", "This resource is not located anywhere...?");
                return false;
            }

            if (this is ResourceItemViewModel) {
                if (await IoC.DialogService.ShowDialogAsync("Delete resource?", $"Delete resource{(this.DisplayName != null ? $"'{this.DisplayName}'" : "")}?", MsgDialogType.OKCancel) != MsgDialogResult.OK)
                    return false;
            }
            else if (this is ResourceFolderViewModel group) {
                int total = ResourceFolderViewModel.CountRecursive(group.Items);
                if (total > 0 && await IoC.DialogService.ShowDialogAsync("Delete selection?", $"Are you sure you want to delete this resource folder? It has {total} sub-item{Lang.S(total)}?", MsgDialogType.OKCancel) != MsgDialogResult.OK)
                    return false;
            }

#if DEBUG
            this.Parent.Model.UnregisterDisposeAndRemoveItemAt(index);
#else
            try {
                this.Parent.Model.UnregisterDisposeAndRemoveItemAt(index);
            }
            catch (Exception e) {
                await IoC.DialogService.ShowMessageExAsync("Error deleting item", "An exception occurred while deleting this item", e.GetToString());
            }
#endif

            return true;
        }

        public async Task<bool> RenameAsync() {
            string result = await IoC.UserInput.ShowSingleInputDialogAsync("Rename group", "Input a new name for this group", this.DisplayName, Validators.ForNonWhiteSpaceString());
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
    }
}