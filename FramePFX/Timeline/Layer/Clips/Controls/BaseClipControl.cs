using System.Windows.Controls;

namespace FramePFX.Timeline.Layer.Clips.Controls {
    public class BaseClipControl : Control {
        public TimelineClipContainerControl ParentContainer => this.Parent as TimelineClipContainerControl;
    }
}