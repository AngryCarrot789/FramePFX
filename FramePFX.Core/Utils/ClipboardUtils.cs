using System.Threading.Tasks;

namespace FramePFX.Core.Utils {
    public static class ClipboardUtils {
        public static async Task<bool> SetClipboardOrShowErrorDialog(string text) {
            if (CoreIoC.Clipboard == null) {
                await CoreIoC.MessageDialogs.ShowMessageAsync("No clipboard", "Clipboard is unavailable.\n" + text);
                return false;
            }
            else {
                CoreIoC.Clipboard.ReadableText = text;
                return true;
            }
        }
    }
}