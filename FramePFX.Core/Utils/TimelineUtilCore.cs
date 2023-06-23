using FramePFX.Core.Automation.Keys;
using FramePFX.Core.Automation.ViewModels;
using FramePFX.Core.Automation.ViewModels.Keyframe;
using FramePFX.Core.Editor.ViewModels;
using FramePFX.Core.Editor.ViewModels.Timeline;

namespace FramePFX.Core.Utils {
    public static class TimelineUtilCore {
        /// <summary>
        /// Whether or not a key frame can be added when a view model property is modified
        /// </summary>
        /// <param name="timeline"></param>
        /// <returns></returns>
        public static bool CanAddKeyFrame(TimelineViewModel timeline, IAutomatableViewModel automatable, AutomationKey key) {
            if (timeline == null) {
                return false;
            }

            VideoEditorViewModel editor = timeline.Project.Editor;
            if (editor == null) {
                return true; // CanAddKeyFrameForPropertyModification defaults to true
            }

            AutomationSequenceViewModel active = automatable.AutomationData.ActiveSequence;
            if (active == null || active.Key != key || active.IsOverrideEnabled) {
                return false;
            }

            if (editor.CanAddKeyFrameForPropertyModification) {
                return !timeline.Project.Editor.Playback.IsPlaying || timeline.Project.Editor.RecordKeyFrames;
            }

            return false;
        }
    }
}