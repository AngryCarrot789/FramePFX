using FramePFX.Core.PropertyPages;

namespace FramePFX.Core.Editor.ViewModels.Timelines.Clips.Pages {
    public class ClipPageFactory : PropertyPageFactory<ClipViewModel, BaseClipPropertyPageViewModel> {
        public static ClipPageFactory Instance { get; } = new ClipPageFactory();

        private ClipPageFactory() {
            this.RegisterPage<ClipViewModel, ClipPageViewModel>();
            this.RegisterPage<VideoClipViewModel, VideoClipPageViewModel>();
            this.RegisterPage<ShapeClipViewModel, ShapeClipPageViewModel>();
            this.RegisterPage<TextClipViewModel, TextClipPageViewModel>();
            this.RegisterPage<ImageClipViewModel, ImageClipPageViewModel>();
            this.RegisterPage<MediaClipViewModel, MediaClipPageViewModel>();
        }
    }
}