using System.Threading.Tasks;

namespace FramePFX.Core.Interactivity {
    public interface IFileDropNotifier {
        Task<bool> CanDrop(string[] paths, FileDropType type);
        Task<FileDropType> OnFilesDropped(string[] paths, FileDropType type);
    }
}