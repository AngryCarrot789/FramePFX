using System.Threading.Tasks;
using FramePFX.Core.Editor.ResourceManaging.ViewModels;

namespace FramePFX.Core.Editor {
    public interface IResourceDropNotifier {
        Task OnVideoResourceDropped(ResourceItemViewModel resource, long frameBegin);
    }
}