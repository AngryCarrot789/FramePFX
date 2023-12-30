using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editor.ViewModels;
using FramePFX.Editor.ViewModels.Timelines;

namespace FramePFX.Editor.Actions.Clips {
    [ActionRegistration("actions.timeline.ToggleEnableClips")]
    public class ToggleClipsEnabledAction : ContextAction {
        public override bool CanExecute(ContextActionEventArgs e) {
            return EditorActionUtils.HasClipOrTimeline(e.DataContext);
        }

        public override async Task ExecuteAsync(ContextActionEventArgs e) {
            if (!EditorActionUtils.GetClipWithSelection(e.DataContext, out List<ClipViewModel> clips))
                return;

            bool isEnabled = clips.Select(x => x.Model.IsRenderingEnabled).Count(x => x) < clips.Count;

            VideoEditorViewModel editor = null;
            foreach (ClipViewModel clip in clips) {
                clip.Model.IsRenderingEnabled = isEnabled;
                clip.RaisePropertyChanged(nameof(clip.IsRenderingEnabled));
                if (editor == null)
                    editor = clip.Editor;
            }

            TimelineViewModel timeline = clips.FirstOrDefault(x => x.Timeline != null)?.Timeline;
            if (editor != null && timeline != null) {
                await editor.DoDrawRenderFrame(timeline.Model, false);
            }
        }

        public static bool GetDominantBool(IReadOnlyList<bool> bools) {
            return bools.Count(x => x) >= bools.Count;
        }
    }
}