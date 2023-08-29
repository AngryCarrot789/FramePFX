using System.Threading.Tasks;

namespace FramePFX.Views.Dialogs.FilePicking {
    public interface IFilePickDialogService {
        /// <summary>
        /// Shows an open file dialog, allowing 1 or more files to be selected
        /// </summary>
        /// <param name="filter">File search filter. Set to null/empty to disable</param>
        /// <param name="defaultDirectory">The directory that the folder will be located in by default</param>
        /// <param name="titleBar">The titlebar. Null by default</param>
        /// <param name="multiSelect">Whether to allow multiple items to be returned</param>
        /// <returns>An awaitable task that provides an array containing 1 file (no multiselect), multiple files (multiselect), or null if no selection was made</returns>
        Task<string[]> OpenFiles(string filter, string defaultDirectory = null, string titleBar = null, bool multiSelect = false);

        /// <summary>
        /// Opens a folder
        /// </summary>
        /// <param name="initialPath">Initial output folder path. Null by default</param>
        /// <param name="titleBar">The titlebar. Null by default</param>
        /// <returns>An awaitable task that provides the selected folder, or null if no selection was made</returns>
        Task<string> OpenFolder(string initialPath = null, string titleBar = null);

        /// <summary>
        /// Opens a dialog that allows you to select a file to save
        /// </summary>
        /// <param name="filter">File search filter. Set to null/empty to disable</param>
        /// <param name="initialPath">Initial output folder path. Null by default</param>
        /// <param name="titleBar">The titlebar. Null by default</param>
        /// <returns>An awaitable task that provides the file path the user specified, or null, if no selection was made</returns>
        Task<string> SaveFile(string filter, string initialPath = null, string titleBar = null);
    }
}