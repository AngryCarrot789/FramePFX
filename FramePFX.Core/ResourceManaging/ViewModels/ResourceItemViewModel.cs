using System;

namespace FramePFX.Core.ResourceManaging.ViewModels {
    public abstract class ResourceViewModel : BaseViewModel, IDisposable {
        public ResourceItem Item { get; }

        protected ResourceViewModel(ResourceItem item) {
            this.Item = item;
        }

        public virtual void Dispose() {
            this.Item.Dispose();
        }
    }
}