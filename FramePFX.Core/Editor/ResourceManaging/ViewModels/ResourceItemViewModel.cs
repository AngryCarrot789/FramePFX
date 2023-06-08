using System;

namespace FramePFX.Core.Editor.ResourceManaging.ViewModels {
    public abstract class ResourceItemViewModel : BaseViewModel, IDisposable {
        public ResourceItem Model { get; }

        public ResourceManagerViewModel Manager { get; }

        public string UniqueId => this.Model.UniqueId;

        public bool IsRegistered => this.Model.IsRegistered;

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