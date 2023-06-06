using System.Threading.Tasks;

namespace FramePFX.Core.History {
    /// <summary>
    /// An action that can be undone and then also redone (only after being undone)
    /// </summary>
    public interface IHistoryAction {
        /// <summary>
        /// Undoes the action. E.g. you created a file. Calling this will then delete that file
        /// </summary>
        /// <returns></returns>
        Task UndoAsync();

        /// <summary>
        /// Redoes an action that was undone. E.g. you created a file and then deleted
        /// it, redoing will then re-create that file as if you never undid anything
        /// </summary>
        /// <returns></returns>
        Task RedoAsync();
    }
}