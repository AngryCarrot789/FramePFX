namespace FramePFX.Services.Files {
    /// <summary>
    /// An interface that provides file picking services, such as picking a file to open or a file to save. This also includes for directories too
    /// </summary>
    public interface IFilePickDialogService {
        /// <summary>
        /// Shows a dialog that allows the user to pick a single file
        /// </summary>
        /// <param name="message">A message to show to the user, usually the dialog's caption/titlebar</param>
        /// <param name="filter">A filter which is used to only show specific files. Defaults to any file</param>
        /// <param name="initialDirectory"></param>
        /// <param name="defaultPathPath">
        /// The initial directory that the dialog shows (e.g. desktop or system32 or whatever).
        /// Null by default, meaning the operating system decides what the initial directory is
        /// </param>
        /// <returns>
        /// The selected file path, or null if the user selected nothing or cancelled the operation.
        /// Will not be an empty string or consist of only whitespaces
        /// </returns>
        string OpenFile(string message, string filter = null, string initialDirectory = null);

        /// <summary>
        /// Shows a dialog that allows the user to pick one or more files
        /// </summary>
        /// <param name="message">A message to show to the user, usually the dialog's caption/titlebar</param>
        /// <param name="filter">A filter which is used to only show specific files. Defaults to any file</param>
        /// <param name="defaultPathPath">
        /// The initial directory that the dialog shows (e.g. desktop or system32 or whatever).
        /// Null by default, meaning the operating system decides what the initial directory is
        /// </param>
        /// <returns>
        /// An array containing all of the selected file paths, or null if the user selected nothing or cancelled the
        /// operation. When non-null, it will always have at least one or more elements, never an empty array
        /// </returns>
        string[] OpenMultipleFiles(string message, string filter = null, string initialDirectory = null);

        /// <summary>
        /// Shows a dialog that allows the user to specify a file path to save. This method won't actually save any data of course
        /// </summary>
        /// <param name="message">A message to show to the user, usually the dialog's caption/titlebar</param>
        /// <param name="filter">A filter which is used to only show specific files. Defaults to any file</param>
        /// <param name="initialFilePath">
        /// The file path that will be saved by default. If non-null, then if the user were to just click Save, this method would
        /// return this value. If null, then the initial file path is unspecified and the user has to specify one
        /// </param>
        /// <returns>
        /// The specified file path, or null if the user didn't specify a path or cancelled the
        /// operation. Will not be an empty string or consist of only whitespaces
        /// </returns>
        string SaveFile(string message, string filter = null, string initialFilePath = null);
    }
}