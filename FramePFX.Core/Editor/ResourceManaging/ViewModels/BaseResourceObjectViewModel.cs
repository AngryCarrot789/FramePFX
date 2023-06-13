using System;
using System.Reflection;
using System.Threading.Tasks;

namespace FramePFX.Core.Editor.ResourceManaging.ViewModels {
    public abstract class BaseResourceObjectViewModel : BaseViewModel, IDisposable {
        internal ResourceManagerViewModel manager;
        internal ResourceGroupViewModel group;

        /// <summary>
        /// The manager that this resource is currently associated with
        /// </summary>
        public ResourceManagerViewModel Manager {
            get => this.manager;
            private set => this.RaisePropertyChanged(ref this.manager, value);
        }

        /// <summary>
        /// This resource view model's underlying model object
        /// </summary>
        public BaseResourceObject Model { get; }

        public ResourceGroupViewModel Group {
            get => this.group;
            private set => this.RaisePropertyChanged(ref this.group, value);
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

        public virtual void SetGroup(ResourceGroupViewModel newGroup) {
            this.Group = newGroup;
            this.Model.SetGroup(newGroup?.Model);
        }

        public virtual void SetManager(ResourceManagerViewModel newManager) {
            this.Manager = newManager;
            this.Model.SetManager(newManager?.Model);
        }

        public abstract Task<bool> RenameSelfAction();

        public abstract Task<bool> DeleteSelfAction();

        protected virtual bool CanRename() {
            return true;
        }

        protected virtual bool CanDelete() {
            return true;
        }

        public virtual void Dispose() {
            this.Model.Dispose();
        }
    }
}