using System.Windows.Input;
using FramePFX.Core;
using FramePFX.ResourceManaging.UI;

namespace FramePFX.ResourceManaging.ViewModels {
    public class ResourceItemViewModel : BaseViewModel {
        public ResourceManagerViewModel Manager { get; }

        private string id;
        public string Id {
            get => this.id;
            set => this.RaisePropertyChanged(ref this.id, value);
        }

        public IResourceControl Handle { get; set; }

        public ICommand RenameCommand { get; }
        public ICommand DeleteCommand { get; }

        public ResourceItemViewModel(ResourceManagerViewModel manager) {
            this.Manager = manager;
            this.RenameCommand = new RelayCommand(async () => await this.Manager.RenameResourceAction(this));
            this.DeleteCommand = new RelayCommand(async () => await this.Manager.DeleteResourceAction(this));
        }

        public bool TryGetResource(out ResourceItem item) {
            if (string.IsNullOrWhiteSpace(this.Id)) {
                item = null;
                return false;
            }

            return (item = this.Manager.Manager.GetResource(this.Id)) != null;
        }
    }
}