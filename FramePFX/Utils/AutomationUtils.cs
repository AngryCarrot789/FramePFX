using System;
using System.Numerics;
using FramePFX.Automation.Keyframe;
using FramePFX.Automation.Keys;
using FramePFX.Automation.ViewModels;
using FramePFX.Automation.ViewModels.Keyframe;
using FramePFX.Editor.Timelines;
using FramePFX.Editor.ViewModels.Timelines;

namespace FramePFX.Utils {
    public static class AutomationUtils {
        public static bool GetSuitableFrameForAutomatable(IAutomatableViewModel automatable, AutomationKey key, out long frame) {
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

            return true;
        }

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

        public static void SetValue(KeyFrame keyFrame, object value) {
            switch (keyFrame.DataType) {
                case AutomationDataType.Float:   keyFrame.SetFloatValue((float) value); break;
                case AutomationDataType.Double:  keyFrame.SetDoubleValue((double) value); break;
                case AutomationDataType.Long:    keyFrame.SetLongValue((long) value); break;
                case AutomationDataType.Boolean: keyFrame.SetBooleanValue((bool) value); break;
                case AutomationDataType.Vector2: keyFrame.SetVector2Value((Vector2) value); break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        public static KeyFrameViewModel GetKeyFrameForPropertyChanged(IAutomatableViewModel automatable, AutomationKey key) {
            if (automatable.IsAutomationRefreshInProgress) {
                throw new Exception("Object is currently refreshing an automation value");
            }

            if (GetNewKeyFrameTime(automatable, key, out long frame))
                return automatable.AutomationData[key].GetActiveKeyFrameOrCreateNew(frame);
            return automatable.AutomationData[key].GetOverride();
        }
    }
}