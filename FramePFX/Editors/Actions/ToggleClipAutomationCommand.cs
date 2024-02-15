using FramePFX.Interactivity.DataContexts;
using System.Threading.Tasks;
using FramePFX.Commands;

namespace FramePFX.Editors.Actions {
    public class ToggleClipAutomationCommand : Command {
        public override Task ExecuteAsync(CommandEventArgs e) {
            if (!e.DataContext.TryGetContext(DataKeys.VideoEditorKey, out VideoEditor editor))
                return Task.CompletedTask;
            editor.ShowClipAutomation = !editor.ShowClipAutomation;
            return Task.CompletedTask;
        }
    }
}
