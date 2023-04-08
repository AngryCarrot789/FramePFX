using System.Threading.Tasks;
using FramePFX.ResourceManaging;
using FramePFX.Timeline.ViewModels.Layer;

namespace FramePFX.Timeline {
    public interface IResourceDropNotifier {
        Task OnResourceDropped(ResourceItemViewModel resource);
    }
}