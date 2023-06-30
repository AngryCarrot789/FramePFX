using FramePFX.Core.Actions.Contexts;
using FramePFX.Core.Editor.ViewModels;
using FramePFX.Core.Editor.ViewModels.Timelines;

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
            else if (context.TryGetContext(out TrackViewModel track)) {
                return track.Timeline;
            }
            else if (context.TryGetContext(out ClipViewModel clip)) {
                return clip.Track?.Timeline;
            }
            else {
                return null;
            }
        }
    }
}