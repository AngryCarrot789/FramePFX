using System.Threading.Tasks;

namespace FramePFX.Editor.ResourceChecker
{
    public interface IResourceCheckerService
    {
        /// <summary>
        /// Shows the resource checker dialog which lets the user select how to load invalid resources
        /// </summary>
        /// <param name="checkerViewModel"></param>
        /// <returns>
        /// True if all problems were resolved (e.g. ignored or fixed), otherwise false if the user cancelled the operation
        /// </returns>
        Task<bool> ShowCheckerDialog(ResourceCheckerViewModel checkerViewModel);
    }
}