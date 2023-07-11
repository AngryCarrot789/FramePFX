using System.Linq;
using System.Threading.Tasks;
using FramePFX.Core.Utils;
using FramePFX.Core.Views.Dialogs.UserInputs;

namespace FramePFX.Core.Editor.ResourceManaging.ViewModels {
    public abstract class BaseResourceObjectViewModel : BaseViewModel {
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
        /// This resource view model's underlying model object
        /// </summary>
        public BaseResourceObject Model { get; }

        public ResourceGroupViewModel Parent {
            get => this.parent;
            private set => this.RaisePropertyChanged(ref this.parent, value);
        }

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
            this.RenameCommand = new AsyncRelayCommand(this.RenameSelfAction, () => true);
            this.DeleteCommand = new AsyncRelayCommand(this.DeleteSelfAction, () => this.Parent != null);
        }

        public virtual void SetParent(ResourceGroupViewModel newParent) {
            this.Model.SetParent(newParent?.Model);
            this.Parent = newParent;
        }

        public virtual void SetManager(ResourceManagerViewModel newManager) {
            this.Model.SetManager(newManager?.Manager);
            this.Manager = newManager;
        }

        public async Task<bool> RenameSelfAction() {
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

        public abstract Task<bool> DeleteSelfAction();

        public void Dispose() {
            this.OnDisposing();
            this.Model.Dispose();
        }

        protected virtual void OnDisposing() {

        }
    }
}