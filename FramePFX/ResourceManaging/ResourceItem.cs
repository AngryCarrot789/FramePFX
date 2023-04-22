using System;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Resources {
    public class ResourceItem : IDisposable {
        private readonly ResourceManager manager;
        public string Id { get; set; }

        public bool IsDisposed { get; private set;  }

        public bool IsRegistered => this.manager.IsRegistered(this);

        public ResourceItem(ResourceManager manager) {
            this.manager = manager ?? throw new ArgumentNullException(nameof(manager), "Manager cannot be null");
        }

        public void Dispose() {
            this.EnsureNotDisposed("Resource is already disposed. It cannot be disposed again");
            try {
                using (ExceptionStack stack = ExceptionStack.Push()) {
                    this.DisposeResource(stack);
                }
            }
            finally {
                this.IsDisposed = true;
            }
        }

        protected virtual void DisposeResource(ExceptionStack stack) {

        }

        protected void EnsureNotDisposed(string msg = "Resource is disposed; it cannot be used") {
            if (this.IsDisposed) {
                throw new ObjectDisposedException(this.GetType().Name, msg);
            }
        }
    }
}