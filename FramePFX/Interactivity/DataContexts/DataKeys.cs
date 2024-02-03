using FramePFX.Editors;
using FramePFX.Editors.ResourceManaging;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Effects;
using FramePFX.Editors.Timelines.Tracks;

namespace FramePFX.Interactivity.DataContexts {
    public static class DataKeys {
        public static readonly DataKey<VideoEditor> VideoEditorKey = new DataKey<VideoEditor>("VideoEditor");
        public static readonly DataKey<Project> ProjectKey = new DataKey<Project>("Project");
        public static readonly DataKey<Timeline> TimelineKey = new DataKey<Timeline>("Timeline");
        public static readonly DataKey<Track> TrackKey = new DataKey<Track>("Track");
        public static readonly DataKey<Clip> ClipKey = new DataKey<Clip>("Clip");
        public static readonly DataKey<BaseEffect> EffectKey = new DataKey<BaseEffect>("Effect");

        /// <summary>
        /// A data key for the location of the mouse cursor, in frames, when a context menu
        /// was opened (well, specifically when the track was right clicked)
        /// </summary>
        public static readonly DataKey<long> TrackContextMouseFrameKey = new DataKey<long>("TrackFrameContextMousePos");

        /// <summary>
        /// A data key for the data object drop location, in frames. This is basically where the mouse
        /// cursor was when the drop occurred converted into frames
        /// </summary>
        public static readonly DataKey<long> TrackDropFrameKey = new DataKey<long>("TrackFrameDropPos");

        public static readonly DataKey<ResourceManager> ResourceManagerKey = new DataKey<ResourceManager>("ResourceManager");
        public static readonly DataKey<BaseResource> ResourceObjectKey = new DataKey<BaseResource>("ResourceObject");
    }
}