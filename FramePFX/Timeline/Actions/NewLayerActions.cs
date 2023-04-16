using System.Threading.Tasks;
using FramePFX.Core;
using FramePFX.Core.Actions;
using FramePFX.Core.Views.Dialogs.UserInputs;
using FramePFX.Timeline.ViewModels;

namespace FramePFX.Timeline.Actions {
    [ActionRegistration("actions.editor.timeline.NewVideoLayer")]
    public class NewVideoLayerAction : AnAction {
        public NewVideoLayerAction() : base(() => "New video layer", () => "Creates a new video layer for video clips") {

        }

        public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
            EditorTimeline timeline = EditorActionUtils.FindTimeline(e.DataContext);
            if (timeline == null) {
                if (e.IsUserInitiated) {
                    await CoreIoC.MessageDialogs.ShowMessageAsync("No timeline available", "Create a new project to add a new video layer");
                }

                return true;
            }

            string name = null;
            if (e.IsUserInitiated) {
                name = CoreIoC.UserInput.ShowSingleInputDialog("New video layer", "Input a layer name:", "New Layer", timeline.LayerNameValidator);
                if (string.IsNullOrEmpty(name)) {
                    return true;
                }
            }

            timeline.CreateVideoLayer(name);
            return true;
        }
    }
}