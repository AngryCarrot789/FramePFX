using System.Threading.Tasks;
using FramePFX.ResourceManaging;

namespace FramePFX.Timeline {
    public interface IResourceDropNotifier {
        Task OnVideoResourceDropped(ResourceItem resource, long frameBegin);
    }
}