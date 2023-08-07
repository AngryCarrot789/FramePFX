using System.Threading.Tasks;

namespace FramePFX.Core.Utils {
    /// <summary>
    /// An interface that can be generally renamed by the user pressing rename hotkeys
    /// </summary>
    public interface IRenameTarget {
        /// <summary>
        /// Renames this object, showing it's own custom dialog
        /// </summary>
        /// <returns>True if the rename was a success, otherwise false meaning the object was not renamed</returns>
        Task<bool> RenameAsync();
    }
}