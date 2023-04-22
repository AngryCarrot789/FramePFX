using FramePFX.Core.Actions.Contexts;
using FramePFX.Timeline.ViewModels;
using FramePFX.Timeline.ViewModels.Clips;
using FramePFX.Timeline.ViewModels.Layer;

namespace FramePFX.Timeline.Actions {
    public static class EditorActionUtils {
        public static TimelineViewModel FindTimeline(IDataContext context) {
            if (context.TryGetContext(out VideoEditor editor)) {
                return editor.ActiveProject?.Timeline;
            }
            else if (context.TryGetContext(out TimelineViewModel timeline)) {
                return timeline;
            }
            else if (context.TryGetContext(out BaseTimelineLayer layer)) {
                return layer.Timeline;
            }
            else if (context.TryGetContext(out BaseTimelineClip clip)) {
                return clip.Layer?.Timeline;
            }
            else {
                return null;
            }
        }
    }
}