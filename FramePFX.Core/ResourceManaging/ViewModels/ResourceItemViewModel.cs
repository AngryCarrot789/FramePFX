using System;
using System.Windows.Input;

namespace FramePFX.Core.ResourceManaging.ViewModels {
    public abstract class ResourceItemViewModel : BaseViewModel, IDisposable {
        public ResourceItem Model { get; }

        public ResourceManagerViewModel Manager { get; }

        public string Id {
            get => this.Model.Id;
            set {
                this.Model.Id = value;
                this.RaisePropertyChanged();
            }
        }

        public AsyncRelayCommand RenameCommand { get; }
        public AsyncRelayCommand DeleteCommand { get; }

        protected ResourceItemViewModel(ResourceManagerViewModel manager, ResourceItem model) {
            this.Manager = manager;
            this.Model = model;
            this.RenameCommand = new AsyncRelayCommand(async () => await this.Manager.RenameResourceAction(this));
            this.DeleteCommand = new AsyncRelayCommand(async () => await this.Manager.DeleteResourceAction(this));
        }

        public virtual void Dispose() {
            this.Model.Dispose();
        }
    }
}