using FramePFX.Core.Actions.Contexts;
using FramePFX.Core.Editor.ViewModels;
using FramePFX.Core.Editor.ViewModels.Timeline;
using FramePFX.Core.Editor.ViewModels.Timeline.Clips;

namespace FramePFX.Editor.Timeline.Actions {
    public static class EditorActionUtils {
        public static TimelineViewModel FindTimeline(IDataContext context) {
            if (context.TryGetContext(out TimelineViewModel timeline)) {
                return timeline;
            }
            else if (context.TryGetContext(out ProjectViewModel project)) {
                return project.Timeline;
            }
            else if (context.TryGetContext(out VideoEditorViewModel editor)) {
                return editor.ActiveProject?.Timeline;
            }
            else if (context.TryGetContext(out LayerViewModel layer)) {
                return layer.Timeline;
            }
            else if (context.TryGetContext(out ClipViewModel clip)) {
                return clip.Layer?.Timeline;
            }
            else {
                return null;
            }
        }
    }
}