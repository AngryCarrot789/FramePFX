using System;
using FramePFX.Core.Editor.Timelines;
using FramePFX.Core.Editor.Timelines.AudioClips;
using FramePFX.Core.Editor.Timelines.VideoClips;
using FramePFX.Core.Editor.ViewModels.Timelines;
using FramePFX.Core.Editor.ViewModels.Timelines.Clips;

namespace FramePFX.Core.Editor.Registries {
    /// <summary>
    /// The registry for clips; audio, video, etc
    /// </summary>
    public class ClipRegistry : ModelRegistry<Clip, ClipViewModel> {
        public static ClipRegistry Instance { get; } = new ClipRegistry();

        private ClipRegistry() {
            #region video
            this.Register<ImageClip, ImageClipViewModel>("cv_img");
            this.Register<ShapeClip, ShapeClipViewModel>("cv_square");
            this.Register<TextClip, TextClipViewModel>("cv_txt");
            this.Register<MediaClip, MediaClipViewModel>("cv_media");
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