using FramePFX.Automation.Keys;
using FramePFX.Automation.ViewModels;
using FramePFX.Automation.ViewModels.Keyframe;
using FramePFX.Editor.Timelines;
using FramePFX.Editor.ViewModels.Timelines;

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
                frame = range.ConvertTimelineToRelativeFrame(frame, out bool inRange);
                if (!inRange) {
                    return false;
                }
            }

            AutomationSequenceViewModel active = automatable.AutomationData.ActiveSequence;
            if (timeline.IsRecordingKeyFrames) {
                return active == null || !active.IsOverrideEnabled;
            }
            else {
                if (active != null && active.Key == key) {
                    return !active.IsOverrideEnabled && active.HasKeyFrames;
                }

                // pretty sure that that past the above code, false will always get returned...
                // when the active key does not equal the input key, then the sequence is not active...
                // oh well, just in case IsActiveSequence bugs out, this will work
                AutomationSequenceViewModel sequence = automatable.AutomationData[key];
                if (sequence.IsActiveSequence) {
                    return !sequence.IsOverrideEnabled && sequence.HasKeyFrames;
                }

                return false;
            }
        }
    }
}