using FramePFX.Automation.Keys;
using FramePFX.Automation.ViewModels;
using FramePFX.Automation.ViewModels.Keyframe;
using FramePFX.Editor.Timelines.Effects.ViewModels;
using FramePFX.Editor.ViewModels;
using FramePFX.Editor.ViewModels.Timelines;

namespace FramePFX.Utils {
    public static class AutomationUtils {
        /// <summary>
        /// Whether or not a key frame can be added when a view model property is modified
        /// </summary>
        /// <param name="timeline"></param>
        /// <returns></returns>
        public static bool CanAddKeyFrame(TimelineViewModel timeline, IAutomatableViewModel automatable, AutomationKey key) {
            if (timeline == null) {
                return false;
            }

            if (automatable is ClipViewModel clip) {
                if (!clip.Model.GetRelativeFrame(timeline.PlayHeadFrame, out long _)) {
                    return false;
                }
            }
            else if (automatable is BaseEffectViewModel effect) {
                if (effect.OwnerClip != null && !effect.OwnerClip.Model.GetRelativeFrame(timeline.PlayHeadFrame, out long _)) {
                    return false;
                }
            }

            AutomationSequenceViewModel active = automatable.AutomationData.ActiveSequence;
            VideoEditorViewModel editor = timeline.Project.Editor;
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