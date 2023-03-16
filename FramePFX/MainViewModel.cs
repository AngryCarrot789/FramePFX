using System.IO;
using FramePFX.Core;
using FramePFX.Timeline;

namespace FramePFX {
    public class MainViewModel : BaseViewModel {
        public TimelineViewModel Timeline { get; }

        public MainViewModel() {
            this.Timeline = new TimelineViewModel();
        }
    }
}
