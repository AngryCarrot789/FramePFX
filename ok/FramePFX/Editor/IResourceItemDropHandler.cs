using System.Threading.Tasks;
using FramePFX.Editor.ResourceManaging.ViewModels;

namespace FramePFX.Editor {
    public interface IResourceItemDropHandler {
        bool CanDropResource(ResourceItemViewModel resource);

        Task OnResourceDropped(ResourceItemViewModel resource, long frameBegin);
    }
}