using FramePFX.Editor.Timelines;
using FramePFX.Editor.Timelines.AudioClips;
using FramePFX.Editor.Timelines.VideoClips;
using FramePFX.Editor.ViewModels.Timelines;
using FramePFX.Editor.ViewModels.Timelines.AudioClips;
using FramePFX.Editor.ViewModels.Timelines.VideoClips;

namespace FramePFX.Editor.Registries
{
    /// <summary>
    /// The registry for clips; audio, video, etc
    /// </summary>
    public class ClipFactory : ModelFactory<Clip, ClipViewModel>
    {
        public static ClipFactory Instance { get; } = new ClipFactory();

        private ClipFactory()
        {
            #region video

            // clipvideo_type
            this.Register<ImageVideoClip, ImageVideoClipViewModel>("cv_img");
            this.Register<ShapeSquareVideoClip, ShapeSquareVideoClipViewModel>("cv_square");
            this.Register<TextVideoClip, TextVideoClipViewModel>("cv_txt");
            this.Register<AVMediaVideoClip, AVMediaVideoClipViewModel>("cv_av_media");
            this.Register<MpegMediaVideoClip, MpegMediaVideoClipViewModel>("cv_media");
            this.Register<CompositionVideoClip, CompositionVideoClipViewModel>("cv_comp");
            this.Register<AdjustmentVideoClip, AdjustmentVideoClipViewModel>("cv_adjust");

            #endregion

            #region Audio

            // clipaudio_type
            this.Register<SinewaveClip, SinewaveClipViewModel>("ca_sine");

            #endregion
        }

        public new void Register<TModel, TViewModel>(string id) where TModel : Clip where TViewModel : ClipViewModel
        {
            base.Register<TModel, TViewModel>(id);
        }

        public new Clip CreateModel(string id) => base.CreateModel(id);

        public new ClipViewModel CreateViewModel(string id) => base.CreateViewModel(id);

        public new ClipViewModel CreateViewModelFromModel(Clip model) => base.CreateViewModelFromModel(model);
    }
}