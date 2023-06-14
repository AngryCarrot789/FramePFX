using FramePFX.Core;
using FramePFX.Core.Editor.ViewModels.Timeline.Clips;
using FramePFX.Editor.Timeline.Layer.Clips;

namespace FramePFX.Editor.Timeline.Controls {
    public class VideoClipControl : TimelineClipControl {
        public new VideoLayerControl Layer => (VideoLayerControl) base.Layer;

        public bool IsMovingControl { get; set; }

        public ClipDragData DragData { get; set; }

        public VideoClipControl() {
            this.DataContextChanged += (sender, args) => {
                if (args.NewValue is VideoClipViewModel vm) {
                    BaseViewModel.SetInternalData(vm, typeof(IClipHandle), this);
                }
            };
        }

        public override string ToString() {
            return $"{nameof(VideoClipControl)}({this.Span})";
        }
    }
}
