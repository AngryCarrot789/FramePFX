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
            this.Register<ImageClipModel, ImageClipViewModel>("image_clip");
            this.Register<ShapeClipModel, ShapeClipViewModel>("square_clip");
            this.Register<TextClipModel, TextClipViewModel>("text_clip");
            this.Register<MediaClipModel, MediaClipViewModel>("media_clip");
        }

        public new void Register<TModel, TViewModel>(string id) where TModel : ClipModel where TViewModel : ClipViewModel {
            base.Register<TModel, TViewModel>(id);
        }

        public ClipModel CreateLayerModel(string id) {
            return (ClipModel) Activator.CreateInstance(base.GetModelType(id));
        }

        public ClipViewModel CreateLayerViewModel(string id) {
            return (ClipViewModel) Activator.CreateInstance(base.GetViewModelType(id));
        }

        public ClipViewModel CreateViewModelFromModel(ClipModel model) {
            return (ClipViewModel) Activator.CreateInstance(base.GetViewModelTypeFromModel(model), model);
        }
    }
}