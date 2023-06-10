using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using FramePFX.Core;
using FramePFX.Core.Editor.ViewModels.Timeline.Clips;
using FramePFX.Core.Utils;
using FramePFX.Editor.Timeline.Layer;
using FramePFX.Editor.Timeline.Layer.Clips;
using FramePFX.Editor.Timeline.Utils;

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

        public override IEnumerable<TimelineClipControl> GetClipsThatIntersect(FrameSpan span) {
            return this.GetClipContainers<TimelineVideoClipControl>().Where(x => x.Span.Intersects(span));
        }

        protected override DependencyObject GetContainerForItemOverride() {
            return new TimelineVideoClipControl();
        }

        protected override bool IsItemItsOwnContainerOverride(object item) {
            return item is TimelineVideoClipControl;
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item) {
            base.PrepareContainerForItemOverride(element, item);
            if (element is IClipHandle clip) {
                if (item is VideoClipViewModel viewModel) {
                    BaseViewModel.SetInternalData(viewModel, typeof(IClipHandle), clip);
                }
            }
        }
    }
}
