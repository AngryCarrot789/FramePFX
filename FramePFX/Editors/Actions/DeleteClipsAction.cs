using System.Collections.Generic;
using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Interactivity.DataContexts;

namespace FramePFX.Editors.Actions {
    public class DeleteClipsAction : AnAction {
        public override Task ExecuteAsync(AnActionEventArgs e) {
            HashSet<Clip> clips = new HashSet<Clip>();
            if (e.DataContext.TryGetContext(DataKeys.ClipKey, out Clip focusedClip)) {
                clips.Add(focusedClip);
            }

            Timeline timeline;
            if ((timeline = focusedClip.Timeline) != null || e.DataContext.TryGetContext(DataKeys.TimelineKey, out timeline)) {
                foreach (Clip clip in timeline.SelectedClips) {
                    clips.Add(clip);
                }
            }

            foreach (Clip clip in clips) {
                clip.Destroy();
                clip.Track.RemoveClip(clip);
            }

            return Task.CompletedTask;
        }
    }
}