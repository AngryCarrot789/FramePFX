using FramePFX.Core.Editor.ViewModels;
using FramePFX.Core.Editor.ViewModels.Timeline;

namespace FramePFX.Core.Utils {
    public static class TimelineUtilCore {
        /// <summary>
        /// Whether or not a key frame can be added when a view model property is modified
        /// </summary>
        /// <param name="timeline"></param>
        /// <returns></returns>
        public static bool CanAddKeyFrame(TimelineViewModel timeline) {
            VideoEditorViewModel editor;
            if (timeline == null || (editor = timeline.Project.Editor) == null) {
                return true; // CanAddKeyFrameForPropertyModification defaults to true
            }

            if (editor.CanAddKeyFrameForPropertyModification) {
                return !timeline.Project.Editor.Playback.IsPlaying || timeline.Project.Editor.RecordKeyFrames;
            }

            return false;
        }
    }
}