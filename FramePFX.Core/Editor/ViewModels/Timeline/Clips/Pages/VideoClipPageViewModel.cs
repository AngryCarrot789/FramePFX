namespace FramePFX.Core.Editor.ViewModels.Timeline.Clips.Pages {
    public class VideoClipPageViewModel : BaseClipPropertyPageViewModel {
        public new VideoClipViewModel Target => (VideoClipViewModel) base.Target;
        public VideoClipPageViewModel(VideoClipViewModel target) : base(target, "Clip Data") {

        }
    }

    public class ShapeClipPageViewModel : BaseClipPropertyPageViewModel {
        public new ShapeClipViewModel Target => (ShapeClipViewModel) base.Target;
        public ShapeClipPageViewModel(ShapeClipViewModel target) : base(target, "Shape Info") {

        }
    }

    public class TextClipPageViewModel : BaseClipPropertyPageViewModel {
        public new TextClipViewModel Target => (TextClipViewModel) base.Target;
        public TextClipPageViewModel(TextClipViewModel target) : base(target, "Text Info") {

        }
    }

    public class ImageClipPageViewModel : BaseClipPropertyPageViewModel {
        public new ImageClipViewModel Target => (ImageClipViewModel) base.Target;
        public ImageClipPageViewModel(ImageClipViewModel target) : base(target, "Image Info") {

        }
    }

    public class MediaClipPageViewModel : BaseClipPropertyPageViewModel {
        public new MediaClipViewModel Target => (MediaClipViewModel) base.Target;
        public MediaClipPageViewModel(MediaClipViewModel target) : base(target, "Media Info") {

        }
    }
}