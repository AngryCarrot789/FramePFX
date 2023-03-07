using FrameControl.Core;
using FramePFX.Core.Timeline;

namespace FramePFX {
    public class MainViewModel : BaseViewModel {
        public TimelineViewModel Timeline { get; }

        public MainViewModel() {
            this.Timeline = new TimelineViewModel();
        }
    }
}
