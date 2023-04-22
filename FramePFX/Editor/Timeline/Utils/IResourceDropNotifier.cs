using System.Threading.Tasks;
using FramePFX.ResourceManaging.ViewModels;

namespace FramePFX.Editor.Timeline.Utils {
    public interface IResourceDropNotifier {
        Task OnVideoResourceDropped(ResourceItemViewModel resource, long frameBegin);
    }
}