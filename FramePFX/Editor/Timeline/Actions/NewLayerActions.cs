using System.Threading.Tasks;
using FramePFX.Core;
using FramePFX.Core.Actions;
using FramePFX.Editor.Timeline.ViewModels;

namespace FramePFX.Editor.Timeline.Actions {
    [ActionRegistration("actions.editor.timeline.NewVideoLayer")]
    public class NewVideoLayerAction : AnAction {
        public NewVideoLayerAction() {

        }

        public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
            PFXTimeline timeline = EditorActionUtils.FindTimeline(e.DataContext);
            if (timeline == null) {
                if (e.IsUserInitiated) {
                    await IoC.MessageDialogs.ShowMessageAsync("No timeline available", "Create a new project to add a new video layer");
                }

                return true;
            }

            string name = null;
            if (e.IsUserInitiated) {
                name = await IoC.UserInput.ShowSingleInputDialogAsync("New video layer", "Input a layer name:", "New Layer", timeline.LayerNameValidator);
                if (string.IsNullOrEmpty(name)) {
                    return true;
                }
            }

            timeline.CreateVideoLayer(name);
            return true;
        }
    }
}