using System.Threading.Tasks;

namespace FramePFX.ResourceManaging {
    public interface IFileDropNotifier {
        Task OnFilesDropped(string[] files);
    }
}