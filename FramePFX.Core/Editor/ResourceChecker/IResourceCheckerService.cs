using System.Threading.Tasks;

namespace FramePFX.Core.Editor.ResourceChecker {
    public interface IResourceCheckerService {
        Task<bool> ShowCheckerDialog(ResourceCheckerViewModel checkerViewModel);
    }
}