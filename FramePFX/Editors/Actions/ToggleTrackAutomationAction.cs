using FramePFX.Actions;
using FramePFX.Interactivity.DataContexts;
using System.Threading.Tasks;

namespace FramePFX.Editors.Actions {
    public class ToggleTrackAutomationAction : AnAction {
        public override Task ExecuteAsync(AnActionEventArgs e) {
            if (!e.DataContext.TryGetContext(DataKeys.VideoEditorKey, out VideoEditor editor))
                return Task.CompletedTask;
            editor.ShowTrackAutomation = !editor.ShowTrackAutomation;
            return Task.CompletedTask;
        }
    }
}
