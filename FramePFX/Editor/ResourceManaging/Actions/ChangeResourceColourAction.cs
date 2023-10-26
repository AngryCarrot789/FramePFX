using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editor.ResourceManaging.ViewModels.Resources;
using Gpic.Core.Services;
using SkiaSharp;

namespace FramePFX.Editor.ResourceManaging.Actions {
    [ActionRegistration("action.resources.ChangeResourceColour")]
    public class ChangeResourceColourAction : AnAction {
        public override bool CanExecute(AnActionEventArgs e) {
            return e.DataContext.HasContext<ResourceColourViewModel>();
        }

        public override async Task<bool> ExecuteAsync(AnActionEventArgs e) {
            if (!e.DataContext.TryGetContext(out ResourceColourViewModel resource)) {
                return false;
            }

            if (Services.GetService<IColourPicker>().PickARGB(resource.Colour) is SKColor colour) {
                resource.Colour = colour;
            }

            return true;
        }
    }
}