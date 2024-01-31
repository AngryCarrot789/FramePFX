namespace FramePFX.Services.Messages {
    public interface IMessageDialogService {
        /// <summary>
        /// Shows a dialog message, blocking until it closes. This must be called on the application main thread
        /// </summary>
        /// <param name="caption">The window titlebar message</param>
        /// <param name="message">The main message content</param>
        void ShowMessage(string caption, string message);

        /// <summary>
        /// Shows a dialog message, blocking until it closes. This must be called on the application main thread
        /// </summary>
        /// <param name="caption">The window titlebar message</param>
        /// <param name="header">A message presented in bold above the message, a less concise caption but still short</param>
        /// <param name="message">The main message content</param>
        void ShowMessage(string caption, string header, string message);
    }
}