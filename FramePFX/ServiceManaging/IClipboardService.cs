namespace FramePFX.ServiceManaging
{
    public interface IClipboardService
    {
        /// <summary>
        /// Attempts to set the clipboard to the given string
        /// </summary>
        /// <param name="text">The string to set, or null to clear the clipboard</param>
        /// <returns>True if the clipboard was set, otherwise false if an error occurred</returns>
        bool SetText(string text);

        /// <summary>
        /// Attempts to get text stored in the clipboard
        /// </summary>
        /// <param name="text"></param>
        /// <param name="convert">Converts whatever is in the clipboard into text... which may not be what the user wants</param>
        /// <returns></returns>
        bool GetText(out string text, bool convert = false);
    }
}