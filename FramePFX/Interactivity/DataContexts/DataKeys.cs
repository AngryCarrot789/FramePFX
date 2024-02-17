using FramePFX.Editors;
using FramePFX.Editors.ResourceManaging;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Effects;
using FramePFX.Editors.Timelines.Tracks;

namespace FramePFX.Interactivity.DataContexts {
    public static class DataKeys {
        public static readonly DataKey<VideoEditor> VideoEditorKey = DataKey<VideoEditor>.Create("VideoEditor");
        public static readonly DataKey<Project> ProjectKey = DataKey<Project>.Create("Project");
        public static readonly DataKey<Timeline> TimelineKey = DataKey<Timeline>.Create("Timeline");
        public static readonly DataKey<Track> TrackKey = DataKey<Track>.Create("Track");
        public static readonly DataKey<Clip> ClipKey = DataKey<Clip>.Create("Clip");
        public static readonly DataKey<BaseEffect> EffectKey = DataKey<BaseEffect>.Create("Effect");

        /// <summary>
        /// A data key for the location of the mouse cursor, in frames, when a context menu
        /// was opened (well, specifically when the track was right clicked)
        /// </summary>
        public static readonly DataKey<long> TrackContextMouseFrameKey = DataKey<long>.Create("TrackFrameContextMousePos");

        /// <summary>
        /// A data key for the data object drop location, in frames. This is basically where the mouse
        /// cursor was when the drop occurred converted into frames
        /// </summary>
        public static readonly DataKey<long> TrackDropFrameKey = DataKey<long>.Create("TrackFrameDropPos");

        public static readonly DataKey<ResourceManager> ResourceManagerKey = DataKey<ResourceManager>.Create("ResourceManager");
        public static readonly DataKey<BaseResource> ResourceObjectKey = DataKey<BaseResource>.Create("ResourceObject");
    }
}