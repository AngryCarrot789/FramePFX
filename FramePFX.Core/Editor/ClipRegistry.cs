using System;
using FramePFX.Core.Editor.Timeline;
using FramePFX.Core.Editor.Timeline.Clip;
using FramePFX.Core.Editor.ViewModels.Timeline;
using FramePFX.Core.Editor.ViewModels.Timeline.Clips;

namespace FramePFX.Core.Editor {
    public class ClipRegistry : ModelRegistry<ClipModel, ClipViewModel> {
        public static ClipRegistry Instance { get; } = new ClipRegistry();

        private ClipRegistry() {
            this.Register<ImageClipModel, ImageClipViewModel>("image_clip");
            this.Register<ShapeClipModel, ShapeClipViewModel>("square_clip");
            this.Register<TextClipModel, TextClipViewModel>("text_clip");
        }

        public new void Register<TModel, TViewModel>(string id) where TModel : ClipModel where TViewModel : ClipViewModel {
            base.Register<TModel, TViewModel>(id);
        }

        public ClipModel CreateLayerModel(TimelineModel timeline, string id) {
            return (ClipModel) Activator.CreateInstance(base.GetModelType(id), timeline);
        }

        public ClipViewModel CreateLayerViewModel(TimelineViewModel timeline, string id) {
            return (ClipViewModel) Activator.CreateInstance(base.GetViewModelType(id), timeline);
        }

        public ClipViewModel CreateViewModelFromModel(ClipModel model) {
            return (ClipViewModel) Activator.CreateInstance(base.GetViewModelTypeFromModel(model), model);
        }
    }
}