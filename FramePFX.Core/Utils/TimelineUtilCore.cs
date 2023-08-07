using FramePFX.Core.Automation.Keys;
using FramePFX.Core.Automation.ViewModels;
using FramePFX.Core.Automation.ViewModels.Keyframe;
using FramePFX.Core.Editor.ViewModels;
using FramePFX.Core.Editor.ViewModels.Timelines;

namespace FramePFX.Core.Utils
{
    public static class TimelineUtilCore
    {
        /// <summary>
        /// Whether or not a key frame can be added when a view model property is modified
        /// </summary>
        /// <param name="timeline"></param>
        /// <returns></returns>
        public static bool CanAddKeyFrame(TimelineViewModel timeline, IAutomatableViewModel automatable, AutomationKey key)
        {
            if (timeline == null)
            {
                return false;
            }

            if (automatable is ClipViewModel clip && !clip.Model.GetRelativeFrame(timeline.PlayHeadFrame, out long _))
            {
                return false;
            }

            AutomationSequenceViewModel active = automatable.AutomationData.ActiveSequence;
            VideoEditorViewModel editor = timeline.Project.Editor;
            if (editor != null && editor.IsRecordingKeyFrames)
            {
                return active == null || !active.IsOverrideEnabled;
            }
            else
            {
                if (active != null && active.Key == key)
                {
                    return !active.IsOverrideEnabled;
                }

                AutomationSequenceViewModel modifiedSequence = automatable.AutomationData[key];
                if (modifiedSequence.IsActive)
                {
                    return !modifiedSequence.IsOverrideEnabled;
                }

                return false;
            }
        }
    }
}