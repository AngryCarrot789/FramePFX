using FramePFX.Editor.Timelines.VideoClips;

namespace FramePFX.Editor.ViewModels.Timelines.VideoClips {
    public class MpegMediaVideoClipViewModel : VideoClipViewModel {
        public new MpegMediaVideoClip Model => (MpegMediaVideoClip) ((ClipViewModel) this).Model;

        public MpegMediaVideoClipViewModel(MpegMediaVideoClip model) : base(model) {
        }
    }
}