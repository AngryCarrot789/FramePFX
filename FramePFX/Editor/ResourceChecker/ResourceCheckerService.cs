using System.Threading.Tasks;
using FramePFX.Core;
using FramePFX.Core.Editor.ResourceChecker;

namespace FramePFX.Editor.ResourceChecker
{
    [ServiceImplementation(typeof(IResourceCheckerService))]
    public class ResourceCheckerService : IResourceCheckerService
    {
        public async Task<bool> ShowCheckerDialog(ResourceCheckerViewModel checkerViewModel)
        {
            ResourceCheckerDialog dialog = new ResourceCheckerDialog {DataContext = checkerViewModel};
            checkerViewModel.Dialog = dialog;
            return await dialog.ShowDialogAsync() == true;
        }
    }
}