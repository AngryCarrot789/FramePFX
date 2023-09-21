using System.Threading.Tasks;
using FramePFX.Editor.ResourceManaging.ViewModels;

namespace FramePFX.Editor {
    public interface ITrackResourceDropHandler {
        /// <summary>
        /// Gets if this track can accept the given resource to be dropped
        /// </summary>
        bool CanDropResource(ResourceItemViewModel resource);

        /// <summary>
        /// Invoked when the resource is dropped at the given frame on this track
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="frame"></param>
        /// <returns></returns>
        Task OnResourceDropped(ResourceItemViewModel resource, long frame);
    }
}