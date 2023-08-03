using System.Threading.Tasks;

namespace FramePFX.Core.Editor {
    public interface IRenameable {
        Task<bool> RenameAsync();
    }
}