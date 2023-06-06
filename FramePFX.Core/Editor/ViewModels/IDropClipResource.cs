using System.Threading.Tasks;
using FramePFX.Core.Editor.ResourceManaging.ViewModels;

namespace FramePFX.Core.Editor.ViewModels {
    public interface IDropClipResource {
        bool CanDropResource(ResourceItemViewModel resource);

        Task OnDropResource(ResourceItemViewModel resource);
    }
}