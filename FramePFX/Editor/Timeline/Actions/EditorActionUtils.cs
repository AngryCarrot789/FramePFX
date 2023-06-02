using FramePFX.Core.Actions.Contexts;
using FramePFX.Editor.Timeline.ViewModels;
using FramePFX.Editor.Timeline.ViewModels.Clips;
using FramePFX.Editor.Timeline.ViewModels.Layer;

namespace FramePFX.Editor.Timeline.Actions {
    public static class EditorActionUtils {
        public static PFXTimeline FindTimeline(IDataContext context) {
            if (context.TryGetContext(out PFXVideoEditor editor)) {
                return editor.ActiveProject?.Timeline;
            }
            else if (context.TryGetContext(out PFXTimeline timeline)) {
                return timeline;
            }
            else if (context.TryGetContext(out PFXTimelineLayer layer)) {
                return layer.Timeline;
            }
            else if (context.TryGetContext(out PFXClipViewModel clip)) {
                return clip.Layer?.Timeline;
            }
            else {
                return null;
            }
        }
    }
}