using FramePFX.Core.Editor.Timeline.VideoClips;

namespace FramePFX.Core.Editor.ViewModels.Timeline.Clips.Pages {
    public class ClipPageViewModel : BaseClipPropertyPageViewModel {
        public ClipPageViewModel(ClipViewModel target) : base(target, "Clip Data") {
        }
    }

    public class VideoClipPageViewModel : BaseClipPropertyPageViewModel {
        public new VideoClipViewModel Target => (VideoClipViewModel) base.Target;
        public VideoClipPageViewModel(VideoClipViewModel target) : base(target, "Visual Clip Data") {

        }
    }

    public class ShapeClipPageViewModel : BaseClipPropertyPageViewModel {
        public static readonly ShapeClipPageViewModel Dummy = new ShapeClipPageViewModel(new ShapeClipViewModel(new ShapeClipModel()));

        public new ShapeClipViewModel Target => (ShapeClipViewModel) base.Target;
        public ShapeClipPageViewModel(ShapeClipViewModel target) : base(target, "Shape Info") {

        }
    }

    public class TextClipPageViewModel : BaseClipPropertyPageViewModel {
        public static readonly TextClipPageViewModel Dummy = new TextClipPageViewModel(new TextClipViewModel(new TextClipModel()));

        public new TextClipViewModel Target => (TextClipViewModel) base.Target;
        public TextClipPageViewModel(TextClipViewModel target) : base(target, "Text Info") {

        }
    }

    public class ImageClipPageViewModel : BaseClipPropertyPageViewModel {
        public static readonly ImageClipPageViewModel Dummy = new ImageClipPageViewModel(new ImageClipViewModel(new ImageClipModel()));

        public new ImageClipViewModel Target => (ImageClipViewModel) base.Target;
        public ImageClipPageViewModel(ImageClipViewModel target) : base(target, "Image Info") {

        }
    }

    public class MediaClipPageViewModel : BaseClipPropertyPageViewModel {
        public static readonly MediaClipPageViewModel Dummy = new MediaClipPageViewModel(new MediaClipViewModel(new MediaClipModel()));

        public new MediaClipViewModel Target => (MediaClipViewModel) base.Target;
        public MediaClipPageViewModel(MediaClipViewModel target) : base(target, "Media Info") {

        }
    }
}