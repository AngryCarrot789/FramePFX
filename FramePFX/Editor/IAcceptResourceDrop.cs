using System.Threading.Tasks;
using FramePFX.Editor.ResourceManaging.ViewModels;

namespace FramePFX.Editor {
    public interface IAcceptResourceDrop {
        bool CanDropResource(BaseResourceObjectViewModel resource);

        /// <summary>
        /// Called when a resource item is dropped. <see cref="CanDropResource"/> is always called just before this method is invoked
        /// </summary>
        /// <param name="resource">The dropped resource (not null, but it's owning manager should be checked)</param>
        /// <returns>A task to await</returns>
        Task OnDropResource(BaseResourceObjectViewModel resource);
    }
}