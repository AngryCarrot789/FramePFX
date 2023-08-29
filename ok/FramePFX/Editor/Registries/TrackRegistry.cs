using System;
using FramePFX.Editor.Timelines;
using FramePFX.Editor.Timelines.Tracks;
using FramePFX.Editor.ViewModels.Timelines;
using FramePFX.Editor.ViewModels.Timelines.Tracks;

namespace FramePFX.Editor.Registries {
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

        public Track CreateModel(string id) {
            return (Track) Activator.CreateInstance(base.GetModelType(id));
        }

        public TrackViewModel CreateViewModel(string id) {
            return (TrackViewModel) Activator.CreateInstance(base.GetViewModelType(id));
        }

        public TrackViewModel CreateViewModelFromModel(Track model) {
            return (TrackViewModel) Activator.CreateInstance(base.GetViewModelTypeFromModel(model), model);
        }
    }
}