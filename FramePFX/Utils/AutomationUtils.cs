using FramePFX.Automation.Keys;
using FramePFX.Automation.ViewModels;
using FramePFX.Automation.ViewModels.Keyframe;
using FramePFX.Editor.Timelines;
using FramePFX.Editor.ViewModels;
using FramePFX.Editor.ViewModels.Timelines;
using FramePFX.Editor.ViewModels.Timelines.Effects;

namespace FramePFX.Utils {
    public static class AutomationUtils {
        public static bool GetNewKeyFrameTime(IAutomatableViewModel automatable, AutomationKey key, out long frame) {
            TimelineViewModel timeline = automatable.Timeline;
            if (timeline == null) {
                frame = 0;
                return false;
            }

            frame = timeline.PlayHeadFrame;
            if (automatable is IStrictFrameRange range) {
                frame = range.ConvertTimelineToRelativeFrame(frame, out bool isValid);
                if (!isValid) {
                    return false;
                }
            }

            AutomationSequenceViewModel active = automatable.AutomationData.ActiveSequence;
            VideoEditorViewModel editor = timeline.Project?.Editor;
            if (editor != null && editor.IsRecordingKeyFrames) {
                return active == null || !active.IsOverrideEnabled;
            }
            else {
                if (active != null && active.Key == key) {
                    return !active.IsOverrideEnabled;
                }

                AutomationSequenceViewModel modifiedSequence = automatable.AutomationData[key];
                if (modifiedSequence.IsActive) {
                    return !modifiedSequence.IsOverrideEnabled;
                }

                return false;
            }
        }
    }
}