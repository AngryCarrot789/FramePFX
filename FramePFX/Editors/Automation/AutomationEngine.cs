using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Tracks;

namespace FramePFX.Editors.Automation {
    public static class AutomationEngine {
        public static void UpdateValues(Timeline timeline) {
            long playHead = timeline.PlayHeadPosition;
            foreach (Track track in timeline.Tracks) {
                track.AutomationData.Update(playHead);
                foreach (Clip clip in track.GetClipsAtFrame(playHead)) {
                    long relative = clip.ConvertTimelineToRelativeFrame(playHead, out bool isValid);
                    if (isValid) // should always be true based on logic from GetClipsAtFrame
                        clip.AutomationData.Update(relative);
                }
            }
        }
    }
}