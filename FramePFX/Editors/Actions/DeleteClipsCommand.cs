using System.Collections.Generic;
using FramePFX.CommandSystem;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Interactivity.DataContexts;

namespace FramePFX.Editors.Actions {
    public class DeleteClipsCommand : Command {
        public override void Execute(CommandEventArgs e) {
            HashSet<Clip> clips = new HashSet<Clip>();
            if (DataKeys.ClipKey.TryGetContext(e.DataContext, out Clip focusedClip)) {
                clips.Add(focusedClip);
            }

            Timeline timeline;
            if ((timeline = focusedClip?.Timeline) != null || DataKeys.TimelineKey.TryGetContext(e.DataContext, out timeline)) {
                foreach (Clip clip in timeline.SelectedClips) {
                    clips.Add(clip);
                }
            }

            foreach (Clip clip in clips) {
                clip.Destroy();
                clip.Track.RemoveClip(clip);
            }
        }
    }
}