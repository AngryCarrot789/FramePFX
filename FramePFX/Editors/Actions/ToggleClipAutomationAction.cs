using FramePFX.Actions;
using FramePFX.Interactivity.DataContexts;
using System.Threading.Tasks;

namespace FramePFX.Editors.Actions {
    public class ToggleClipAutomationAction : AnAction {
        public override Task ExecuteAsync(AnActionEventArgs e) {
            if (!e.DataContext.TryGetContext(DataKeys.EditorKey, out VideoEditor editor))
                return Task.CompletedTask;
            editor.ShowClipAutomation = !editor.ShowClipAutomation;
            return Task.CompletedTask;
        }
    }
}
