using System.Threading.Tasks;

namespace FramePFX.Utils
{
    /// <summary>
    /// An interface for an object that can be generally renamed by the user pressing generic rename hotkeys (F2, CTRL+R, etc.)
    /// </summary>
    public interface IRenameTarget
    {
        /// <summary>
        /// Renames this object, showing it's own custom dialog
        /// </summary>
        /// <returns>True if the rename was a success, otherwise false meaning the object was not renamed</returns>
        Task<bool> RenameAsync();
    }
}