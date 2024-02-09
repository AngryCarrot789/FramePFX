using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editors.Automation.Keyframes;
using FramePFX.Editors.Controls.Automation;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Interactivity.DataContexts;
using FramePFX.Utils;

namespace FramePFX.Editors.Actions {
    public class ToggleClipVisibilityAction : AnAction {
        public ToggleClipVisibilityAction() {
        }

        public override Task ExecuteAsync(AnActionEventArgs e) {
            if (!e.DataContext.TryGetContext(DataKeys.ClipKey, out Clip keyedClip) || !(keyedClip is VideoClip focusedClip)) {
                return Task.CompletedTask;
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

            return Task.CompletedTask;
        }
    }
}