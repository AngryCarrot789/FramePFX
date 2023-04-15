using System;
using System.Threading.Tasks;
using System.Windows.Input;
using FramePFX.Core;
using FramePFX.ResourceManaging.Items;

namespace FramePFX.ResourceManaging {
    public class ResourceItemViewModel : BaseViewModel, IDisposable {
        private string uniqueId;
        private bool isRegistered;
        private ResourceManagerViewModel manager;

        /// <summary>
        /// The unique ID of this item. There should not be any other resource items with this ID
        /// </summary>
        public string UniqueID {
            get => this.uniqueId;
            set => this.RaisePropertyChanged(ref this.uniqueId, value);
        }

        /// <summary>
        /// Whether this resource is actually a valid resource registered in the manager or not
        /// </summary>
        public bool IsRegistered {
            get => this.isRegistered;
            set => this.RaisePropertyChanged(ref this.isRegistered, value);
        }

        /// <summary>
        /// The manager associated with this item. If <see cref="IsRegistered"/> is false, then this should be null
        /// </summary>
        public ResourceManagerViewModel Manager {
            get => this.manager;
            set => this.RaisePropertyChanged(ref this.manager, value);
        }

        private bool isDisposed;

        public bool IsDisposed {
            get => this.isDisposed;
            private set => this.RaisePropertyChanged(ref this.isDisposed, value);
        }

        public ICommand RenameCommand { get; }

        public ICommand DeleteCommand { get; }
        
        public INativeResource Resource { get; set; }

        public ResourceItemViewModel() {
            this.RenameCommand = new RelayCommand(async () => await this.RenameAction());
            this.DeleteCommand = new RelayCommand(async () => await this.DeleteAction());
        }

        private Task RenameAction() {
            return this.manager?.RenameResourceAction(this);
        }

        private Task DeleteAction() {
            return this.manager?.DeleteResourceAction(this);
        }

        public void Dispose() {
            if (this.IsDisposed) {
                throw new InvalidOperationException("Already disposed");
            }

            try {
                this.DisposeResource();
            }
            finally {
                this.IsDisposed = true;
            }
        }

        public virtual void DisposeResource() {

        }
    }
}