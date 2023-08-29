using System.Threading.Tasks;

namespace FramePFX.Editor.ResourceChecker {
    public interface IResourceCheckerService {
        Task<bool> ShowCheckerDialog(ResourceCheckerViewModel checkerViewModel);
    }
}