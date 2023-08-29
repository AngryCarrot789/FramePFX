using FramePFX.Actions.Contexts;
using FramePFX.Editor.ViewModels;
using FramePFX.Editor.ViewModels.Timelines;

namespace FramePFX.Editor.Actions {
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