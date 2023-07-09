using System;
using System.Diagnostics;
using FramePFX.Core.Editor.ViewModels.Timelines.Clips;

namespace FramePFX.Editor.Timeline.Controls {
    public class VideoTrackControl : TimelineTrackControl {
        public VideoTrackControl() {

        }

        protected override bool IsItemAContainer(object item) => item is VideoClipControl;

        protected override TimelineClipControl GetContainerForItem(object item) {
            if (item == null || item is VideoClipViewModel)
                return new VideoClipControl();
            #if DEBUG
            Debugger.Break();
            #endif
            throw new Exception($"Tried to generate {nameof(VideoClipControl)} for {item.GetType()}");
        }
    }
}
