using System;
using FramePFX.Editor.Timelines;
using FramePFX.Editor.Timelines.AudioClips;
using FramePFX.Editor.Timelines.VideoClips;
using FramePFX.Editor.ViewModels.Timelines;
using FramePFX.Editor.ViewModels.Timelines.Clips;

namespace FramePFX.Editor.Registries {
    /// <summary>
    /// The registry for clips; audio, video, etc
    /// </summary>
    public class ClipRegistry : ModelRegistry<Clip, ClipViewModel> {
        public static ClipRegistry Instance { get; } = new ClipRegistry();

        private ClipRegistry() {
            #region video

            this.Register<ImageVideoClip, ImageClipViewModel>("cv_img");
            this.Register<ShapeVideoClip, ShapeClipViewModel>("cv_square");
            this.Register<TextVideoClip, TextClipViewModel>("cv_txt");
            this.Register<AVMediaVideoClip, AVMediaClipViewModel>("cv_av_media");
            this.Register<MpegMediaVideoClip, MpegMediaVideoClipViewModel>("cv_media");

            #endregion

            #region Audio

            this.Register<SinewaveClip, SinewaveClipViewModel>("ca_sine");

            #endregion
        }

        public new void Register<TModel, TViewModel>(string id) where TModel : Clip where TViewModel : ClipViewModel {
            base.Register<TModel, TViewModel>(id);
        }

        public Clip CreateModel(string id) {
            return (Clip) Activator.CreateInstance(base.GetModelType(id));
        }

        public ClipViewModel CreateViewModel(string id) {
            return (ClipViewModel) Activator.CreateInstance(base.GetViewModelType(id));
        }

        public ClipViewModel CreateViewModelFromModel(Clip model) {
            return (ClipViewModel) Activator.CreateInstance(base.GetViewModelTypeFromModel(model), model);
        }
    }
}