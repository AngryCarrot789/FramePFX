using System.Collections.Generic;
using FramePFX.CommandSystem;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Tracks;
using FramePFX.Interactivity.DataContexts;

namespace FramePFX.Editors.Actions {
    public class SliceClipsCommand : Command {
        public override void Execute(CommandEventArgs e) {
            if (DataKeys.ClipKey.TryGetContext(e.DataContext, out Clip clip) && clip.Timeline is Timeline timeline) {
                SliceClip(clip, timeline.PlayHeadPosition);
            }
            else if (DataKeys.TimelineKey.TryGetContext(e.DataContext, out timeline)) {
                List<Clip> clips = new List<Clip>();
                foreach (Track track in timeline.Tracks) {
                    clips.AddRange(track.GetClipsAtFrame(timeline.PlayHeadPosition));
                }

                for (int i = clips.Count - 1; i >= 0; i--) {
                    SliceClip(clips[i], timeline.PlayHeadPosition);
                }
            }
        }

        public static void SliceClip(Clip clip, long playHead) {
            if (clip.IntersectsFrameAt(playHead) && playHead != clip.FrameSpan.Begin && playHead != clip.FrameSpan.EndIndex) {
                clip.CutAt(playHead - clip.FrameSpan.Begin);
            }
        }
    }
}