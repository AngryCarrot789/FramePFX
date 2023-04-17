using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using FramePFX.Core;
using FramePFX.Core.Utils;
using FramePFX.ResourceManaging.Items;
using FramePFX.Utils;

namespace FramePFX.ResourceManaging {
    public class ResourceItem : BaseViewModel, IDisposable {
        private ResourceManager manager;
        private string uniqueId;
        private bool isRegistered;
        private bool isDisposed;
        private bool isDisposing;

        public delegate void ResourceModifiedHandler(string propetyName);
        public event ResourceModifiedHandler OnResourceModified;

        /// <summary>
        /// The manager associated with this item. If <see cref="IsRegistered"/> is false, then this should be null
        /// </summary>
        public ResourceManager Manager {
            get => this.manager;
            set => this.RaisePropertyChanged(ref this.manager, value);
        }

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
        /// Whether this resource is disposed or not
        /// </summary>
        public bool IsDisposed {
            get => this.isDisposed;
            private set => this.RaisePropertyChanged(ref this.isDisposed, value);
        }

        /// <summary>
        /// Whether this resource is currently being disposed
        /// </summary>
        public bool IsDisposing {
            get => this.isDisposing;
            private set => this.RaisePropertyChanged(ref this.isDisposing, value);
        }

        public ICommand RenameCommand { get; }

        public ICommand DeleteCommand { get; }

        /// <summary>
        /// Used to store a reference to the underlying control. This is mainly used to reduce the linear 
        /// lookup that the ItemContainerGenerator has to do in order to map a view model to a control
        /// </summary>
        public IResourceControl Handle { get; set; }

        public ResourceItem() {
            this.RenameCommand = new RelayCommand(async () => await this.RenameAction());
            this.DeleteCommand = new RelayCommand(async () => await this.DeleteAction());
        }

        public void RaiseResourceModified(string propertyName) {
            this.OnResourceModified?.Invoke(propertyName);
        }

        public void RaiseResourceModifiedAuto([CallerMemberName] string propertyName = null) {
            this.RaiseResourceModified(propertyName);
        }

        private Task RenameAction() {
            return this.manager?.RenameResourceAction(this);
        }

        private Task DeleteAction() {
            return this.manager?.DeleteResourceAction(this);
        }

        public void Dispose() {
            this.ThrowIfDisposed();
            try {
                this.IsDisposing = true;
                using (ExceptionStack stack = ExceptionStack.Push("Exception while disposing clip")) {
                    this.DisposeResource(stack);
                }
            }
            finally {
                if (this.isDisposing) { // just in case setting IsDisposing throws for some weird reason
                    this.IsDisposed = true;
                    this.IsDisposing = false;
                }
            }
        }

        protected virtual void DisposeResource(ExceptionStack stack) {

        }

        protected void ThrowIfDisposed() {
            if (this.IsDisposed) {
                throw new ObjectDisposedException(this.GetType().Name);
            }
        }
    }
}