using System;
using System.Threading.Tasks;

namespace FramePFX.Core.Editor.ResourceManaging.ViewModels {
    public abstract class BaseResourceObjectViewModel : BaseViewModel, IDisposable {
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
            this.RenameCommand = new AsyncRelayCommand(this.RenameSelfAction, this.CanRename);
            this.DeleteCommand = new AsyncRelayCommand(this.DeleteSelfAction, this.CanDelete);
        }

        public virtual void SetParent(ResourceGroupViewModel newParent) {
            this.Model.SetParent(newParent?.Model);
            this.Parent = newParent;
        }

        public virtual void SetManager(ResourceManagerViewModel newManager) {
            this.Model.SetManager(newManager?.Model);
            this.Manager = newManager;
        }

        public abstract Task<bool> RenameSelfAction();

        public abstract Task<bool> DeleteSelfAction();

        protected virtual bool CanRename() {
            return true;
        }

        protected virtual bool CanDelete() {
            return this.Parent != null;
        }

        public virtual void Dispose() {
            this.Model.Dispose();
        }
    }
}