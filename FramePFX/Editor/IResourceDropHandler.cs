using System.Threading.Tasks;
using FramePFX.Editor.ResourceManaging.ViewModels;
using FramePFX.Interactivity;

namespace FramePFX.Editor {
    public interface IResourceDropHandler {
        /// <summary>
        /// Gets whether the given resource can be dropped on this handler.
        /// This is called every time the mouse moves
        /// </summary>
        bool CanDropResource(BaseResourceViewModel resource);

        /// <summary>
        /// Called when a resource item is dropped. <see cref="CanDropResource"/> may not get called
        /// </summary>
        /// <param name="resource">
        /// The dropped resource (not null, but it's owning manager and parent hierarchy should be checked)
        /// </param>
        /// <param name="dropType"></param>
        /// <returns>A task to await</returns>
        Task OnDropResource(BaseResourceViewModel resource, EnumDropType dropType);
    }
}