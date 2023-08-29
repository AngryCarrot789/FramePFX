using System.Threading.Tasks;

namespace FramePFX.Interactivity {
    public interface IFileDropNotifier {
        Task<bool> CanDrop(string[] paths, ref FileDropType type);

        Task OnFilesDropped(string[] paths);
    }
}