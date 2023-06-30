using System;
using FramePFX.Core.Editor.Timelines;
using FramePFX.Core.Editor.Timelines.Tracks;
using FramePFX.Core.Editor.ViewModels.Timelines;
using FramePFX.Core.Editor.ViewModels.Timelines.Tracks;

namespace FramePFX.Core.Editor.Registries {
    /// <summary>
    /// The registry for tracks; audio, video, etc
    /// </summary>
    public class TrackRegistry : ModelRegistry<Track, TrackViewModel> {
        public static TrackRegistry Instance { get; } = new TrackRegistry();

        private TrackRegistry() {
            this.Register<VideoTrack, VideoTrackViewModel>("t_vid");
            this.Register<AudioTrack, AudioTrackViewModel>("t_aud");
        }

        public new void Register<TModel, TViewModel>(string id) where TModel : Track where TViewModel : TrackViewModel {
            base.Register<TModel, TViewModel>(id);
        }

        public Track CreateModel(Timeline timeline, string id) {
            return (Track) Activator.CreateInstance(base.GetModelType(id), timeline);
        }

        public TrackViewModel CreateViewModel(TimelineViewModel timeline, string id) {
            return (TrackViewModel) Activator.CreateInstance(base.GetViewModelType(id), timeline);
        }

        public TrackViewModel CreateViewModelFromModel(TimelineViewModel timeline, Track model) {
            if (!ReferenceEquals(timeline.Model, model.Timeline)) {
                throw new ArgumentException("Timeline models do not match");
            }

            return (TrackViewModel) Activator.CreateInstance(base.GetViewModelTypeFromModel(model), timeline, model);
        }
    }
}