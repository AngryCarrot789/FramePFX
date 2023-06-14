using System.Windows;

namespace FramePFX.Editor.Timeline.Controls {
    public class VideoTimelineLayerControl : TimelineLayerControl {

        //           Width
        // ---------------------------
        // UnitZoom * MaxFrameDuration

        // /// <summary>
        // /// Gets or sets the maximum duration (in frames) of this timeline layer based on it's visual/actual pixel width
        // /// <para>
        // /// Setting this will modify the <see cref="UnitZoom"/> property as ActualWidth / MaxFrameDuration
        // /// </para>
        // /// </summary>
        // public double MaxFrameDuration {
        //     get => this.ActualWidth / this.UnitZoom;
        //     set => this.UnitZoom = this.ActualWidth / value;
        // }

        public VideoTimelineLayerControl() {

        }

        public override bool CanAcceptClip(TimelineClipControl clip) {
            return clip is TimelineVideoClipControl;
        }
    }
}
