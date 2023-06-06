using System.Threading.Tasks;
using FramePFX.Core;
using FramePFX.Core.Actions;
using FramePFX.Core.Editor.ViewModels.Timeline;
using FramePFX.Core.Editor.ViewModels.Timeline.Layers;

namespace FramePFX.Editor.Timeline.Actions {
    [ActionRegistration("actions.editor.NewVideoLayer")]
    public class NewVideoLayerAction : AnAction {
        public NewVideoLayerAction() {

        }

        public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
            TimelineViewModel timeline = EditorActionUtils.FindTimeline(e.DataContext);
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

            VideoLayerViewModel layer = await timeline.AddVideoLayerAction();
            layer.Name = name;
            return true;
        }
    }
}