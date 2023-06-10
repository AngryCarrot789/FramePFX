using System.Threading.Tasks;
using FramePFX.Core.Editor.ResourceManaging;

namespace FramePFX.Core.Editor {
    public interface IResourceDropHandler {
        Task OnResourceDropped(ResourceItem resource, long frameBegin);
    }
}