using FramePFX.Editors;
using FramePFX.Editors.ResourceManaging;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Effects;
using FramePFX.Editors.Timelines.Tracks;
using SkiaSharp;

namespace FramePFX.Interactivity.DataContexts {
    public static class DataKeys {
        public static readonly DataKey<VideoEditor> EditorKey = new DataKey<VideoEditor>("VideoEditor");
        public static readonly DataKey<Project> ProjectKey = new DataKey<Project>("Project");
        public static readonly DataKey<Timeline> TimelineKey = new DataKey<Timeline>("Timeline");
        public static readonly DataKey<Track> TrackKey = new DataKey<Track>("Track");
        public static readonly DataKey<Clip> ClipKey = new DataKey<Clip>("Clip");
        public static readonly DataKey<BaseEffect> EffectKey = new DataKey<BaseEffect>("Effect");
        public static readonly DataKey<long> TrackMouseFrameKey = new DataKey<long>("TrackFrameMPOS");

        public static readonly DataKey<ResourceManager> ResourceManagerKey = new DataKey<ResourceManager>("ResourceManager");
        public static readonly DataKey<BaseResource> ResourceObjectKey = new DataKey<BaseResource>("ResourceObject");
    }
}