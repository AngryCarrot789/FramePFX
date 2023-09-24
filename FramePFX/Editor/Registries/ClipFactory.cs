using FramePFX.Editor.Timelines;
using FramePFX.Editor.Timelines.AudioClips;
using FramePFX.Editor.Timelines.VideoClips;
using FramePFX.Editor.ViewModels.Timelines;
using FramePFX.Editor.ViewModels.Timelines.AudioClips;
using FramePFX.Editor.ViewModels.Timelines.VideoClips;

namespace FramePFX.Editor.Registries {
    /// <summary>
    /// The registry for clips; audio, video, etc
    /// </summary>
    public class ClipFactory : ModelFactory<Clip, ClipViewModel> {
        public static ClipFactory Instance { get; } = new ClipFactory();

        private ClipFactory() {
            #region video

            // clipvideo_type
            this.Register<ImageVideoClip, ImageClipViewModel>("cv_img");
            this.Register<ShapeVideoClip, ShapeClipViewModel>("cv_square");
            this.Register<TextVideoClip, TextClipViewModel>("cv_txt");
            this.Register<AVMediaVideoClip, AVMediaClipViewModel>("cv_av_media");
            this.Register<MpegMediaVideoClip, MpegMediaVideoClipViewModel>("cv_media");
            this.Register<CompositionVideoClip, CompositionVideoClipViewModel>("cv_comp");

            #endregion

            #region Audio

            // clipaudio_type
            this.Register<SinewaveClip, SinewaveClipViewModel>("ca_sine");

            #endregion
        }

        public new void Register<TModel, TViewModel>(string id) where TModel : Clip where TViewModel : ClipViewModel {
            base.Register<TModel, TViewModel>(id);
        }

        public new Clip CreateModel(string id) => base.CreateModel(id);

        public new ClipViewModel CreateViewModel(string id) => base.CreateViewModel(id);

        public new ClipViewModel CreateViewModelFromModel(Clip model) => base.CreateViewModelFromModel(model);
    }
}