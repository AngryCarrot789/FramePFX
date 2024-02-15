using FramePFX.Interactivity.DataContexts;
using System.Threading.Tasks;
using FramePFX.Commands;

namespace FramePFX.Editors.Actions {
    public class ToggleTrackAutomationCommand : Command {
        public override Task ExecuteAsync(CommandEventArgs e) {
            if (!e.DataContext.TryGetContext(DataKeys.VideoEditorKey, out VideoEditor editor))
                return Task.CompletedTask;
            editor.ShowTrackAutomation = !editor.ShowTrackAutomation;
            return Task.CompletedTask;
        }
    }
}
