using System;
using FramePFX.Core.Editor.Timeline;
using FramePFX.Core.Editor.Timeline.VideoClips;
using FramePFX.Core.Editor.ViewModels.Timeline;
using FramePFX.Core.Editor.ViewModels.Timeline.Clips;

namespace FramePFX.Core.Editor.Registries {
    /// <summary>
    /// The registry for clips; audio, video, etc
    /// </summary>
    public class ClipRegistry : ModelRegistry<ClipModel, ClipViewModel> {
        public static ClipRegistry Instance { get; } = new ClipRegistry();

        private ClipRegistry() {
            this.Register<ImageClipModel, ImageClipViewModel>("cl_img");
            this.Register<ShapeClipModel, ShapeClipViewModel>("cl_square");
            this.Register<TextClipModel, TextClipViewModel>("cl_txt");
            this.Register<MediaClipModel, MediaClipViewModel>("cl_media");
        }

        public new void Register<TModel, TViewModel>(string id) where TModel : ClipModel where TViewModel : ClipViewModel {
            base.Register<TModel, TViewModel>(id);
        }

        public ClipModel CreateModel(string id) {
            return (ClipModel) Activator.CreateInstance(base.GetModelType(id));
        }

        public ClipViewModel CreateViewModel(string id) {
            return (ClipViewModel) Activator.CreateInstance(base.GetViewModelType(id));
        }

        public ClipViewModel CreateViewModelFromModel(ClipModel model) {
            return (ClipViewModel) Activator.CreateInstance(base.GetViewModelTypeFromModel(model), model);
        }
    }
}