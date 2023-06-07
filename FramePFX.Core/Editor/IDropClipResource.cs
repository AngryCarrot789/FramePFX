using System.Threading.Tasks;
using FramePFX.Core.Editor.ResourceManaging;

namespace FramePFX.Core.Editor.ViewModels {
    public interface IDropClipResource {
        bool CanDropResource(ResourceItem resource);

        /// <summary>
        /// Called when a resource item is dropped. <see cref="CanDropResource"/> is always called just before this method is invoked
        /// </summary>
        /// <param name="resource">The dropped resource (not null, but it's owning manager should be checked)</param>
        /// <returns>A task to await</returns>
        Task OnDropResource(ResourceItem resource);
    }
}