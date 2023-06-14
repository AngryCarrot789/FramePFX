using FramePFX.Core;
using FramePFX.Core.Editor.ViewModels.Timeline.Clips;
using FramePFX.Editor.Timeline.Layer.Clips;

namespace FramePFX.Editor.Timeline.Controls {
    public class TimelineVideoClipControl : TimelineClipControl {
        public new VideoLayerControl Layer => (VideoLayerControl) base.Layer;

        public bool IsMovingControl { get; set; }

        public ClipDragData DragData { get; set; }

        public TimelineVideoClipControl() {
            this.DataContextChanged += (sender, args) => {
                if (args.NewValue is VideoClipViewModel vm) {
                    BaseViewModel.SetInternalData(vm, typeof(IClipHandle), this);
                }
            };
        }

        public override string ToString() {
            return $"TimelineClipControl({this.Span})";
        }
    }
}
