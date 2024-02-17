using System.Collections.Generic;
using System.Linq;
using FramePFX.CommandSystem;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Interactivity.DataContexts;

namespace FramePFX.Editors.Actions {
    public class ToggleClipVisibilityCommand : Command {
        public ToggleClipVisibilityCommand() {
        }

        public override void Execute(CommandEventArgs e) {
            if (!DataKeys.ClipKey.TryGetContext(e.DataContext, out Clip keyedClip) || !(keyedClip is VideoClip focusedClip)) {
                return;
            }

            if (!(focusedClip.Timeline is Timeline timeline) || timeline.GetSelectedClipCountWith(focusedClip) == 1) {
                VideoClip.IsVisibleParameter.SetValue(focusedClip, !VideoClip.IsVisibleParameter.GetValue(focusedClip));
            }
            else {
                int sum = 0;
                List<VideoClip> clips = timeline.GetSelectedClipsWith(focusedClip).Where(x => x is VideoClip).Cast<VideoClip>().ToList();
                foreach (VideoClip theClip in clips) {
                    if (VideoClip.IsVisibleParameter.GetValue(theClip)) {
                        sum++;
                    }
                    else {
                        sum--;
                    }
                }

                bool value = sum <= 0;
                foreach (VideoClip theClip in clips) {
                    VideoClip.IsVisibleParameter.SetValue(theClip, value);
                }
            }
        }
    }
}