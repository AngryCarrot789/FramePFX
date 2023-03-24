using FramePFX.Core;

namespace FramePFX.Timeline.Layer.Clips {
    public abstract class ClipViewModel : BaseViewModel {
        /// <summary>
        /// The container that contains this clip
        /// </summary>
        public ClipContainerViewModel Container { get; set; }

        protected ClipViewModel() {

        }
    }
}