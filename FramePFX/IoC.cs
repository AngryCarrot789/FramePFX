using FramePFX.Services.Files;
using FramePFX.Services.Messages;

namespace FramePFX {
    public static class IoC {
        /// <summary>
        /// Gets the application's message dialog service, for showing messages to the user
        /// </summary>
        public static IMessageDialogService MessageService => ApplicationCore.Instance.Services.GetService<IMessageDialogService>();

        /// <summary>
        /// Gets the application's user input dialog service, for querying basic inputs from the user
        /// </summary>
        public static IUserInputDialogService UserInputService => ApplicationCore.Instance.Services.GetService<IUserInputDialogService>();

        /// <summary>
        /// Gets the application's file picking service, for picking files and directories to open/save
        /// </summary>
        public static IFilePickDialogService FilePickService => ApplicationCore.Instance.Services.GetService<IFilePickDialogService>();
    }
}