using FramePFX.Core;

namespace FramePFX.Timeline.Layer.Clips {
    /// <summary>
    /// The base class for actual data clips
    /// </summary>
    public abstract class ClipViewModel : BaseViewModel {
        /// <summary>
        /// The container that contains this clip
        /// </summary>
        public ClipContainerViewModel Container { get; set; }

        protected ClipViewModel() {

        }

        public static void SetContainer(ClipViewModel clip, ClipContainerViewModel container) {
            ClipContainerViewModel oldContainer = clip.Container;
            if (oldContainer != null) {
                clip.Container = null;
                clip.OnRemovedFromContainer(oldContainer, container != null);
            }

            if (container != null) {
                clip.Container = container;
                clip.OnAddedToContainer(container);
            }
        }

        public virtual void OnRemovedFromContainer(ClipContainerViewModel container, bool isReplace) {
            if (container.ClipContent != null) {
                ClipContainerViewModel.SetClipContent(container, null);
            }
        }

        public virtual void OnAddedToContainer(ClipContainerViewModel container) {
            if (container.ClipContent != this) {
                ClipContainerViewModel.SetClipContent(container, this);
            }
        }
    }
}