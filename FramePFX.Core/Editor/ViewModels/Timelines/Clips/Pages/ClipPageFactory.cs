using FramePFX.Core.PropertyPages;

namespace FramePFX.Core.Editor.ViewModels.Timelines.Clips.Pages {
    public class ClipPageFactory : PropertyPageFactory<ClipViewModel, BaseClipPropertyPageViewModel> {
        public static ClipPageFactory Instance { get; } = new ClipPageFactory();

        private ClipPageFactory() {
            this.AddPage<ClipViewModel, ClipPageViewModel>();
            this.AddPage<VideoClipViewModel, VideoClipPageViewModel>();
            this.AddPage<ShapeClipViewModel, ShapeClipPageViewModel>();
            this.AddPage<TextClipViewModel, TextClipPageViewModel>();
            this.AddPage<ImageClipViewModel, ImageClipPageViewModel>();
            this.AddPage<OldMediaClipViewModel, MediaClipPageViewModel>();
        }
    }
}