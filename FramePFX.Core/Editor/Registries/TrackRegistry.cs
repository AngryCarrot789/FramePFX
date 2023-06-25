using System;
using FramePFX.Core.Editor.Timeline;
using FramePFX.Core.Editor.Timeline.Tracks;
using FramePFX.Core.Editor.ViewModels.Timeline;
using FramePFX.Core.Editor.ViewModels.Timeline.Tracks;

namespace FramePFX.Core.Editor.Registries {
    /// <summary>
    /// The registry for tracks; audio, video, etc
    /// </summary>
    public class TrackRegistry : ModelRegistry<TrackModel, TrackViewModel> {
        public static TrackRegistry Instance { get; } = new TrackRegistry();

        private TrackRegistry() {
            this.Register<VideoTrackModel, VideoTrackViewModel>("t_vid");
            this.Register<AudioTrackModel, AudioTrackViewModel>("t_aud");
        }

        public new void Register<TModel, TViewModel>(string id) where TModel : TrackModel where TViewModel : TrackViewModel {
            base.Register<TModel, TViewModel>(id);
        }

        public TrackModel CreateModel(TimelineModel timeline, string id) {
            return (TrackModel) Activator.CreateInstance(base.GetModelType(id), timeline);
        }

        public TrackViewModel CreateViewModel(TimelineViewModel timeline, string id) {
            return (TrackViewModel) Activator.CreateInstance(base.GetViewModelType(id), timeline);
        }

        public TrackViewModel CreateViewModelFromModel(TimelineViewModel timeline, TrackModel model) {
            if (!ReferenceEquals(timeline.Model, model.Timeline)) {
                throw new ArgumentException("Timeline models do not match");
            }

            return (TrackViewModel) Activator.CreateInstance(base.GetViewModelTypeFromModel(model), timeline, model);
        }
    }
}