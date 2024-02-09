using FramePFX.Editors.ResourceManaging.Resources;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Clips.Core;
using FramePFX.Editors.Timelines.Effects;
using FramePFX.Editors.Timelines.Tracks;

namespace FramePFX.Editors.Automation {
    public static class AutomationEngine {
        public static void UpdateValues(Timeline timeline) => UpdateValues(timeline, timeline.PlayHeadPosition);

        public static void UpdateValues(Timeline timeline, long playHead) {
            foreach (Track track in timeline.Tracks) {
                track.AutomationData.UpdateAllAutomated(playHead);
                foreach (Clip clip in track.GetClipsAtFrame(playHead)) {
                    UpdateValues(clip, clip.ConvertTimelineToRelativeFrame(playHead, out _));
                }
            }
        }

        public static void UpdateValues(Clip clip, long relativePlayHead) {
            clip.AutomationData.UpdateAllAutomated(relativePlayHead);
            foreach (BaseEffect effect in clip.Effects) {
                effect.AutomationData.UpdateAllAutomated(relativePlayHead);
            }

            if (clip is ICompositionClip compClip && compClip.ResourceCompositionKey.TryGetResource(out ResourceComposition compResource)) {
                UpdateValues(compResource.Timeline, relativePlayHead);
            }
        }
    }
}