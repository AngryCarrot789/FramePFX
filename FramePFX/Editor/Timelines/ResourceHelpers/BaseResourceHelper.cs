using FramePFX.Editor.ResourceManaging;
using FramePFX.Editor.ResourceManaging.Events;

namespace FramePFX.Editor.Timelines.ResourceHelpers
{
    /// <summary>
    /// The base class for resource helpers
    /// </summary>
    public abstract class BaseResourceHelper
    {
        /// <summary>
        /// An event fired when the online state of a resource changes (e.g. user set it to offline or online)
        /// </summary>
        public event ResourceItemEventHandler OnlineStateChanged;

        protected BaseResourceHelper()
        {
        }

        public virtual void Dispose()
        {
        }

        protected virtual void OnOnlineStateChanged(ResourceManager manager, ResourceItem item)
        {
            this.OnlineStateChanged?.Invoke(manager, item);
        }
    }
}