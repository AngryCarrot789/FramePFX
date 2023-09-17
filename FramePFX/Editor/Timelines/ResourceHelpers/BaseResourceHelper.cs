using System;
using FramePFX.Editor.ResourceManaging;
using FramePFX.Editor.ResourceManaging.Events;
using FramePFX.Utils;

namespace FramePFX.Editor.Timelines.ResourceHelpers {
    /// <summary>
    /// The base class for resource helpers
    /// </summary>
    public abstract class BaseResourceHelper {
        /// <summary>
        /// An event fired when the online state of a resource changes (e.g. user set it to offline or online)
        /// </summary>
        public event ResourceItemEventHandler OnlineStateChanged;

        protected BaseResourceHelper() {
        }

        ~BaseResourceHelper() => this.Dispose(false);

        public void Dispose(bool isDisposing = true) {
            using (ErrorList list = new ErrorList())
                this.Dispose(list, isDisposing);
        }

        public void Dispose(ErrorList list, bool isDisposing = true) {
            try {
                this.DisposeCore(list, isDisposing);
            }
            catch (Exception e) {
                list.Add(new Exception("Unexpected exception", e));
            }
            finally {
                GC.SuppressFinalize(this);
            }
        }

        protected virtual void DisposeCore(ErrorList list, bool disposing) {
        }

        protected virtual void OnOnlineStateChanged(ResourceManager manager, ResourceItem item) {
            this.OnlineStateChanged?.Invoke(manager, item);
        }
    }
}