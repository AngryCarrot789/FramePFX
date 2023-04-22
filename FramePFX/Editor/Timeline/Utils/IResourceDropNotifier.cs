using System.Threading.Tasks;
using FramePFX.ResourceManaging.ViewModels;

namespace FramePFX.Editor.Timeline {
    public interface IResourceDropNotifier {
        Task OnVideoResourceDropped(ResourceItemViewModel resource, long frameBegin);
    }
}