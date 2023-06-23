using FramePFX.Core.Editor.ViewModels.Timeline;
using FramePFX.Core.Editor.ViewModels.Timeline.Clips;

namespace FramePFX.Editor.Properties.Pages {
    public class PropertyPageRegistry : PageRegistry<ClipViewModel> {
        public static PropertyPageRegistry Instance { get; } = new PropertyPageRegistry();

        private PropertyPageRegistry() {
            this.Register<ClipViewModel>((x) => new PropertyPageBaseClip());
            this.Register<VideoClipViewModel>((x) => new PropertyPageVideoClip());
            this.Register<ShapeClipViewModel>((x) => new PropertyPageShapeClip());
            this.Register<TextClipViewModel>((x) => new PropertyPageTextClip());
        }
    }
}