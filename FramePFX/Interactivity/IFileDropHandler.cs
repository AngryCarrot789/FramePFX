using System.Threading.Tasks;

namespace FramePFX.Interactivity {
    public interface IFileDropNotifier {
        /// <summary>
        /// Ges the allow drop type(s) from the given dragged paths and input allowed drop types
        /// </summary>
        /// <param name="paths">The paths being dragged (non-null and has at least 1 entry)</param>
        /// <param name="dropType">Drop type the user wants to perform</param>
        /// <returns></returns>
        EnumDropType GetFileDropType(string[] paths, EnumDropType dropType);

        Task OnFilesDropped(string[] paths, EnumDropType dropType);
    }
}