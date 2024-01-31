using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Effects;
using FramePFX.Editors.Timelines.Tracks;

namespace FramePFX.Editors.Automation {
    public static class AutomationEngine {
        public static void UpdateValues(Timeline timeline) {
            UpdateValues(timeline, timeline.PlayHeadPosition);
        }

        public static void UpdateValues(Timeline timeline, long playHead) {
            foreach (Track track in timeline.Tracks) {
                track.AutomationData.Update(playHead);
                foreach (Clip clip in track.GetClipsAtFrame(playHead)) {
                    long relative = clip.ConvertTimelineToRelativeFrame(playHead, out _);
                    clip.AutomationData.Update(relative);
                    foreach (BaseEffect effect in clip.Effects) {
                        effect.AutomationData.Update(relative);
                    }
                }
            }
        }
    }
}