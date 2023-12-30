using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editor.ResourceManaging.ViewModels.Resources;
using Gpic.Core.Services;
using SkiaSharp;

namespace FramePFX.Editor.ResourceManaging.Actions {
    [ActionRegistration("action.resources.ChangeResourceColour")]
    public class ChangeResourceColourAction : ContextAction {
        public override bool CanExecute(ContextActionEventArgs e) {
            return e.DataContext.HasContext<ResourceColourViewModel>();
        }

        public override async Task ExecuteAsync(ContextActionEventArgs e) {
            if (e.DataContext.TryGetContext(out ResourceColourViewModel resource)) {
                if (IoC.GetService<IColourPicker>().PickARGB(resource.Colour) is SKColor colour) {
                    resource.Colour = colour;
                }
            }
        }
    }
}